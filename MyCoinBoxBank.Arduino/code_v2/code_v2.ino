#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>

const char* sta_ssid     = "NAME";
const char* sta_password = "PASS";

const char* ap_ssid      = "MyCoinBoxBank";
const char* ap_password  = "12345678"; // minimum 8 chars for WPA2

ESP8266WebServer server(80);

const int COIN_ACCEPTOR_PIN = 14; // GPIO14 = D5
const int LED_PIN           = 2;  // built-in LED, often active LOW

volatile unsigned long lastPulseMs = 0;
volatile int pulseCount = 0;
volatile bool coinDetected = false;

int lastCoinValue = 0; // centavos
bool acceptorActive = false;

void ICACHE_RAM_ATTR onCoinPulse()
{
    if (!acceptorActive) return;

    unsigned long now = millis();

    // debounce / pulse filter
    if (now - lastPulseMs < 30) return;

    lastPulseMs = now;
    pulseCount++;
    coinDetected = true;
}

int pulsesToCentavos(int pulses)
{
    // Adjust to your actual coin acceptor programming
    if (pulses == 1)  return 100;   // ₱1
    if (pulses == 5)  return 500;   // ₱5
    if (pulses == 10) return 1000;  // ₱10
    if (pulses == 20) return 2000;  // ₱20
    return 0;
}

void handlePing()
{
    server.send(200, "text/plain", "OK");
}

void handleStart()
{
    acceptorActive = true;
    pulseCount = 0;
    coinDetected = false;
    lastCoinValue = 0;
    digitalWrite(LED_PIN, LOW);
    server.send(200, "text/plain", "Coin acceptor active");
}

void handleStop()
{
    acceptorActive = false;
    digitalWrite(LED_PIN, HIGH);
    server.send(200, "text/plain", "Coin acceptor stopped");
}

void handleLastCoin()
{
    if (coinDetected && pulseCount > 0)
    {
        delay(300); // allow final pulses to finish

        int pulses = pulseCount;
        pulseCount = 0;
        coinDetected = false;

        lastCoinValue = pulsesToCentavos(pulses);
        server.send(200, "text/plain", String(lastCoinValue));
    }
    else
    {
        server.send(200, "text/plain", "0");
    }
}

void handleInfo()
{
    String msg = "";
    msg += "STA IP: ";
    msg += WiFi.localIP().toString();
    msg += "\nAP IP: ";
    msg += WiFi.softAPIP().toString();
    msg += "\nSTA Status: ";
    msg += String(WiFi.status());
    msg += "\n";
    server.send(200, "text/plain", msg);
}

void handleNotFound()
{
    server.send(404, "text/plain", "Not found");
}

void setup()
{
    Serial.begin(115200);

    pinMode(LED_PIN, OUTPUT);
    digitalWrite(LED_PIN, HIGH);

    pinMode(COIN_ACCEPTOR_PIN, INPUT_PULLUP);
    attachInterrupt(digitalPinToInterrupt(COIN_ACCEPTOR_PIN), onCoinPulse, FALLING);

    // AP + STA mode
    WiFi.mode(WIFI_AP_STA);

    // Start AP first so SSID appears immediately
    bool apOk = WiFi.softAP(ap_ssid, ap_password);
    Serial.println(apOk ? "SoftAP started" : "SoftAP failed");
    Serial.print("AP IP: ");
    Serial.println(WiFi.softAPIP());

    // Then try connecting to router
    WiFi.begin(sta_ssid, sta_password);
    Serial.print("Connecting to router");

    unsigned long startAttempt = millis();
    while (WiFi.status() != WL_CONNECTED && millis() - startAttempt < 15000)
    {
        delay(500);
        Serial.print(".");
    }
    Serial.println();

    if (WiFi.status() == WL_CONNECTED)
    {
        Serial.print("Connected to router. STA IP: ");
        Serial.println(WiFi.localIP());
    }
    else
    {
        Serial.println("Router connection failed. AP mode still available.");
    }

    server.on("/ping", handlePing);
    server.on("/start", handleStart);
    server.on("/stop", handleStop);
    server.on("/lastcoin", handleLastCoin);
    server.on("/info", handleInfo);
    server.onNotFound(handleNotFound);

    server.begin();
    Serial.println("HTTP server started");
}

void loop()
{
    server.handleClient();
}