#include <WiFi.h>
#include <WebServer.h>

const char* ssid     = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

WebServer server(80);

// GPIO pins
const int INSERT_PIN   = 26; // relay/servo for inserting
const int WITHDRAW_PIN = 27; // relay/servo for withdrawing
const int LED_PIN      = 2;  // built-in LED

void handlePing() {
    server.send(200, "text/plain", "OK");
}

void handleInsert() {
    Serial.println("Insert triggered");
    digitalWrite(LED_PIN, HIGH);
    digitalWrite(INSERT_PIN, HIGH);
    delay(500);
    digitalWrite(INSERT_PIN, LOW);
    digitalWrite(LED_PIN, LOW);
    server.send(200, "text/plain", "Insert triggered");
}

void handleWithdraw() {
    Serial.println("Withdraw triggered");
    digitalWrite(LED_PIN, HIGH);
    digitalWrite(WITHDRAW_PIN, HIGH);
    delay(500);
    digitalWrite(WITHDRAW_PIN, LOW);
    digitalWrite(LED_PIN, LOW);
    server.send(200, "text/plain", "Withdraw triggered");
}

void handleNotFound() {
    server.send(404, "text/plain", "Not found");
}

void setup() {
    Serial.begin(115200);

    pinMode(INSERT_PIN, OUTPUT);
    pinMode(WITHDRAW_PIN, OUTPUT);
    pinMode(LED_PIN, OUTPUT);

    digitalWrite(INSERT_PIN, LOW);
    digitalWrite(WITHDRAW_PIN, LOW);

    WiFi.begin(ssid, password);
    Serial.print("Connecting to WiFi");

    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }

    Serial.println();
    Serial.print("Connected! IP address: ");
    Serial.println(WiFi.localIP()); // <-- Copy this IP to Esp32Service.cs

    server.on("/ping",     handlePing);
    server.on("/insert",   handleInsert);
    server.on("/withdraw", handleWithdraw);
    server.onNotFound(handleNotFound);

    server.begin();
    Serial.println("HTTP server started");
}

void loop() {
    server.handleClient();
}
