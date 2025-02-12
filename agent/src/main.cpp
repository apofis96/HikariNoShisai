#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <string>
#include <sstream>

const char *ssid = "";
const char *password = "";
int l_sub = 0;
int l_main = 0;

#define LED_SUB 16
#define LED_MAIN 5
#define BUTTON_COMP 4
#define SATA 2

ESP8266WebServer server(80);

void setup() {
  pinMode(LED_SUB, OUTPUT);
  pinMode(LED_MAIN, OUTPUT);
  pinMode(BUTTON_COMP, OUTPUT);
  pinMode(SATA, OUTPUT);
  digitalWrite(LED_SUB, 0);
  digitalWrite(LED_MAIN, 0);
  digitalWrite(BUTTON_COMP, 1);
  digitalWrite(SATA, 1);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
  }
  server.begin();
  
  server.on("/led_sub_on", []() {
    l_sub = 1;
    digitalWrite(LED_SUB, l_sub);
    server.send(200);
  });
  server.on("/led_sub_off", []() {
    l_sub = 0;
    digitalWrite(LED_SUB, l_sub);
    server.send(200);
  });
  server.on("/led_main_on", []() {
    l_main = 1;
    digitalWrite(LED_MAIN, l_main);
    server.send(200);
  });
  server.on("/led_main_off", []() {
    l_main = 0;
    digitalWrite(LED_MAIN, l_main);
    server.send(200);
  });
  server.on("/button_comp", []() {
    digitalWrite(BUTTON_COMP, 0);
    delay(500);
    digitalWrite(BUTTON_COMP, 1);
    server.send(200);
  });
  server.on("/ssd", []() {
    digitalWrite(SATA, 1);
    server.send(200);
  });
  server.on("/hdd", []() {
    digitalWrite(SATA, 0);
    server.send(200);
  });
  server.on("/status", []() {
    std::string sub("Sub:");
    //std::string stored("_stored:");
    std::string main("Main:");
    //const char *cstr = (sub + std::to_string(digitalRead(LED_SUB))+ stored + std::to_string(l_sub)+ main + std::to_string(digitalRead(LED_MAIN))+ stored + std::to_string(l_main)).c_str();
    const char *cstr = (sub + std::to_string(digitalRead(LED_SUB)) + std::to_string(l_sub)).c_str();
    server.send(200, "text/plain", cstr);
  });
}
void loop() {
  server.handleClient();
}