#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClientSecureBearSSL.h>

#include "secret.h"

ESP8266WiFiMulti WiFiMulti;

int ledState = LOW;
unsigned long previousMillis = 0;
unsigned long currentMillis = 0;
const unsigned long interval = 5000;
volatile bool buttonPressed = false;
unsigned long lastPress = 0;
unsigned long lastGrid = 0;
int gridState = -1;
int gridReadState = LOW;
int terminalStatus = LOW;
int terminalStatusRead = LOW;

const unsigned long debounceDelay = 3000;
char terminalUrl[256];
char statisticsUrl[128];
char buffer[256];

const int inputButton = D2;
const int inputGrid = D1;
const int outputTerminal = D3;

void IRAM_ATTR handleButtonInterrupt() {
  buttonPressed = true;
}

void setup() {
  Serial.begin(115200);
  Serial.println();

  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(inputButton, INPUT_PULLUP);
  pinMode(inputGrid, INPUT);
  pinMode(outputTerminal, OUTPUT);
  attachInterrupt(digitalPinToInterrupt(inputButton), handleButtonInterrupt, RISING);
  digitalWrite(outputTerminal, terminalStatus);

  Serial.println();

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(_ssid, _password);

  while ((WiFiMulti.run() != WL_CONNECTED)) {
    Serial.write('.');
    delay(500);
  }
  Serial.println(" connected to WiFi");

  snprintf(
    terminalUrl,
    sizeof(terminalUrl),
    "%s/terminal?agentId=%s&terminalId=%s",
    _host,
    _id,
    _terminalId);
  snprintf(
    statisticsUrl,
    sizeof(statisticsUrl),
    "%s/statistics",
    _host);
}

void loop() {
  currentMillis = millis();
  if (buttonPressed) {
    buttonPressed = false;
    if (currentMillis - lastPress > debounceDelay) {
      lastPress = currentMillis;
      terminalStatus = !terminalStatus;
      digitalWrite(outputTerminal, terminalStatus);
      setTerminalStatus(terminalUrl, terminalStatus);
    }
  }

  if (currentMillis - previousMillis >= interval || currentMillis < previousMillis) {
    previousMillis = currentMillis;
    if (ledState == LOW) {
      ledState = HIGH;
    } else {
      ledState = LOW;
    }
    digitalWrite(LED_BUILTIN, ledState);

    terminalStatusRead = getTerminalStatus(terminalUrl);
    if (terminalStatusRead == -1) {
      showError();
    } else {
      if (terminalStatusRead != terminalStatus) {
        terminalStatus = terminalStatusRead;
        digitalWrite(outputTerminal, terminalStatus);
      }
    }
  }

  gridReadState = digitalRead(inputGrid);
  if (gridReadState != gridState) {
    if (lastGrid == 0) {
      lastGrid = currentMillis;
    } else {
      if (currentMillis - lastGrid > debounceDelay) {
        gridState = gridReadState;
        lastGrid = 0;
        setAgentStatistic(gridState == 1);
      }
    }
  }
}

int getTerminalStatus(const char* url) {
  int value = -1;
  bool insideBrackets = false;
  std::unique_ptr<BearSSL::WiFiClientSecure> client(new BearSSL::WiFiClientSecure);
  client->setInsecure();
  HTTPClient https;

  if (https.begin(*client, url)) {
    https.addHeader("x-api-key", _apiKey);
    int httpCode = https.GET();

    if (httpCode == HTTP_CODE_OK) {
      Serial.printf("stream read start\n");
      WiFiClient& stream = https.getStream();

      while (stream.connected()) {
        if (!stream.available()) continue;
        char c = stream.read();

        if (c == '<') {
          insideBrackets = true;
        } else if (c == '>' && insideBrackets) {
          insideBrackets = false;
          Serial.printf("Value = %d\n", value);
          break;
        } else if (insideBrackets) {
          if (c == '-') value = -1;
          else if (c == '0') value = 0;
          else if (c == '1') value = 1;
          else {
            value = -1;
            insideBrackets = false;
          }
        }
      }
    }
    https.end();
  }
  return value;
}

int setTerminalStatus(const char* url, int status) {
  snprintf(
    buffer,
    sizeof(buffer),
    "%s&isActive=%s",
    url,
    status ? "true" : "false");

  std::unique_ptr<BearSSL::WiFiClientSecure> client(new BearSSL::WiFiClientSecure);
  client->setInsecure();
  HTTPClient https;

  if (https.begin(*client, buffer)) {
    https.addHeader("x-api-key", _apiKey);
    int httpCode = https.sendRequest("PATCH");

    if (httpCode == HTTP_CODE_NO_CONTENT) {
      https.end();

      return 1;
    }
    https.end();
  }
  return -1;
}

int setAgentStatistic(bool isGridAvailable) {
  snprintf(
    buffer,
    sizeof(buffer),
    "{\"agentId\": \"%s\",\"isGridAvailable\": %s}",
    _id,
    isGridAvailable ? "true" : "false");

  Serial.printf("%s\n", buffer);
  Serial.printf("%s\n", statisticsUrl);

  std::unique_ptr<BearSSL::WiFiClientSecure> client(new BearSSL::WiFiClientSecure);
  client->setInsecure();
  HTTPClient https;

  if (https.begin(*client, statisticsUrl)) {
    https.addHeader("x-api-key", _apiKey);
    https.addHeader("Content-Type", "application/json");
    int httpCode = https.POST(buffer);

    if (httpCode == HTTP_CODE_OK) {
      https.end();

      return 1;
    }
    https.end();
  }
  return -1;
}

void showError() {
  for (int i = 0; i <= 10; i++) {
    digitalWrite(LED_BUILTIN, LOW);
    delay(100);
    digitalWrite(LED_BUILTIN, HIGH);
    delay(100);
  }
}
