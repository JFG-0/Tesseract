#include <WiFi.h>
#include <WiFiUdp.h>

#define RXD2 16
#define TXD2 17

WiFiUDP udp;
const char* ssid = "Zenfone 8_3222";
const char* password = "Papayas4";
const char* targetIP = "10.217.164.123"; //Update to IP of computer in the network
//const char* targetIP = "255.255.255.255";  // Broadcast if needed
const int udpPort = 8888;

void setup() {
  Serial.begin(115200);
  Serial2.begin(9600, SERIAL_8N1, RXD2, TXD2);

  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) delay(200);
}

void loop() {
  if (Serial2.available()) {
    String face = Serial2.readStringUntil('\n');
    face.trim();

    Serial.println("Face: " + face);

    udp.beginPacket(targetIP, udpPort);
    udp.print(face);
    udp.endPacket();
  }
}