#include <WiFi.h>
#include <WebServer.h>

const char* ssid     = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

WebServer server(80);

const int COIN_ACCEPTOR_PIN = 26;
const int LED_PIN           = 2;

volatile int pulseCount     = 0;
volatile bool coinDetected  = false;
int lastCoinValue           = 0; // in centavos: 100=₱1, 500=₱5, 1000=₱10, 2000=₱20
bool acceptorActive         = false;

// Coin acceptor sends pulses — count them to determine denomination
void IRAM_ATTR onCoinPulse() {
    pulseCount++;
    coinDetected = true;
}

int pulsesToCentavos(int pulses) {
    // Adjust these based on your coin acceptor model
    if (pulses == 1)  return 100;  // ₱1
    if (pulses == 5)  return 500;  // ₱5
    if (pulses == 10) return 1000; // ₱10
    if (pulses == 20) return 2000; // ₱20
    return 0;
}

void handlePing() {
    server.send(200, "text/plain", "OK");
}

void handleStart() {
    acceptorActive = true;
    pulseCount     = 0;
    coinDetected   = false;
    lastCoinValue  = 0;
    digitalWrite(LED_PIN, HIGH);
    server.send(200, "text/plain", "Coin acceptor active");
}

void handleStop() {
    acceptorActive = false;
    digitalWrite(LED_PIN, LOW);
    server.send(200, "text/plain", "Coin acceptor stopped");
}

// App polls this endpoint to check if a coin was inserted
void handleLastCoin() {
    if (coinDetected && pulseCount > 0) {
        // Wait briefly for all pulses to finish
        delay(300);
        lastCoinValue = pulsesToCentavos(pulseCount);
        pulseCount    = 0;
        coinDetected  = false;

        server.send(200, "text/plain", String(lastCoinValue));
    } else {
        server.send(200, "text/plain", "0"); // no coin yet
    }
}

void handleNotFound() {
    server.send(404, "text/plain", "Not found");
}

void setup() {
    Serial.begin(115200);

    pinMode(LED_PIN, OUTPUT);
    pinMode(COIN_ACCEPTOR_PIN, INPUT_PULLUP);
    attachInterrupt(digitalPinToInterrupt(COIN_ACCEPTOR_PIN), onCoinPulse, FALLING);

    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println("\nConnected! IP: " + WiFi.localIP().toString());

    server.on("/ping",     handlePing);
    server.on("/start",    handleStart);
    server.on("/stop",     handleStop);
    server.on("/lastcoin", handleLastCoin);
    server.onNotFound(handleNotFound);

    server.begin();
}

void loop() {
    server.handleClient();
}
