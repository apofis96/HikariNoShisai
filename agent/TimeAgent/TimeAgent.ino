#include <ESP8266WiFi.h>
#include <ezTime.h>
#include <Arduino.h>
#include <ESP8266WiFiMulti.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClientSecureBearSSL.h>

#include "secret.h"

const bool isDebug = false;
int segDelayMajor = 1250;
int segDelayMinor = 300;

#define SER 16
#define S_CLK 5
#define R_CLK 4

char m1;
char m2;
char m3;
char m4;
int weather;
bool isSaved = true;

void write_vector(bool arr[], bool isSeg, bool isDot = false, int delay = 0);

bool emty[] = { false, false, false, false, false, false, false, false };

bool seg0[] = { false, false, false, false, false, false, false, true };
bool seg1[] = { false, false, false, false, false, false, true, false };
bool seg2[] = { false, false, false, false, false, true, false, false };
bool seg3[] = { false, false, false, false, true, false, false, false };
bool seg4[] = { false, false, false, true, false, false, false, false };
bool seg5[] = { false, false, true, false, false, false, false, false };
bool seg6[] = { false, true, false, false, false, false, false, false };
bool seg7[] = { true, false, false, false, false, false, false, false };
bool segAll[] = { true, true, true, true, true, true, true, true };
bool segMinor[] = { true, true, true, true, false, false, false, false };

bool dig1[] = { false, false, false, true, false, false, true };
bool dig2[] = { true, false, true, true, true, true, false };
bool dig3[] = { true, false, true, true, false, true, true };
bool dig4[] = { true, true, false, true, false, false, true };
bool dig5[] = { true, true, true, false, false, true, true };
bool dig6[] = { true, true, true, false, true, true, true };
bool dig7[] = { false, false, true, true, false, false, true };
bool dig8[] = { true, true, true, true, true, true, true };
bool dig9[] = { true, true, true, true, false, true, true };
bool dig0[] = { false, true, true, true, true, true, true };
bool digMinus[] = { true, false, false, false, false, false, false };
bool digCelsium[] = { true, false, false, false, true, true, false };

bool* digits[] = { dig0, dig1, dig2, dig3, dig4, dig5, dig6, dig7, dig8, dig9 };

ESP8266WiFiMulti WiFiMulti;
Timezone tz;

char weatherUrl[256];

void debugPrint(const char* msg) {
  if (isDebug) {
    Serial.print(msg);
  }
}

int getWeather(const char* url, int retryCount = 3) {
  int value = 0;
  bool insideBrackets = false;
  bool isNegative = false;
  bool isSuccess = false;

  std::unique_ptr<BearSSL::WiFiClientSecure> client(new BearSSL::WiFiClientSecure);
  client->setInsecure();
  HTTPClient https;

  if (https.begin(*client, url)) {
    https.addHeader("x-api-key", _apiKey);
    int httpCode = https.GET();

    if (httpCode == HTTP_CODE_OK) {
      WiFiClient& stream = https.getStream();

      while (stream.connected()) {
        if (!stream.available()) continue;
        char c = stream.read();

        if (c == '<') {
          insideBrackets = true;
        } else if (c == '>' && insideBrackets) {
          insideBrackets = false;
          if (isNegative) {
            value = -value;
          }
          isSuccess = true;
          break;
        } else if (insideBrackets) {
          if (c == '-') {
            isNegative = true;
          } else if (c >= '0' && c <= '9') {
            value = value * 10 + (c - '0');
          } else {
            value = -99;
            break;
          }
        }
      }
    }
    https.end();
  }
  debugPrint("\nweather done\n");

  if (isSuccess) {
    isSaved = false;
    return value;
  }
  if (retryCount > -1) {
    return getWeather(url, retryCount -1);
  }
  
  isSaved = true;
  return weather;
}

void setup() {
  if (isDebug) {
    Serial.begin(115200);
    Serial.println("debug started");
  }

  pinMode(SER, OUTPUT);
  pinMode(S_CLK, OUTPUT);
  pinMode(R_CLK, OUTPUT);
  digitalWrite(SER, 0);
  digitalWrite(S_CLK, 0);
  digitalWrite(R_CLK, 0);

  delay(20);

  write_vector(segAll, false);
  write_vector(dig8, true, true, 500);
  delay(500);

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(_ssid, _password);

  int i = 0;
  bool incr = true;
  while ((WiFiMulti.run() != WL_CONNECTED)) {
    debugPrint(".");
    write_vector(segAll, false);
    write_vector(digits[i], true, i % 2 == 0, 500);
    if (i == 9) {
      incr = false;
    } else if (i == 0) {
      incr = true;
    }
    if (incr) {
      i++;
    } else {
      i--;
    }
    delay(250);
  }

  debugPrint("\nntp start\n");

  setServer(_ntpServer);
  waitForSync();
  tz.setLocation(_timeZone);

  snprintf(
    weatherUrl,
    sizeof(weatherUrl),
    "%s/agents/%s/weather",
    _host,
    _id);

  debugPrint("\nSetup done\n");

  delay(500);

  weather = getWeather(weatherUrl);
  transformWeather(weather);
  debugPrint(weatherUrl);

  debugPrint("\nInitial weather done\n");

  debugPrint(tz.dateTime().c_str());
  debugPrint("\nTimezone: ");
  debugPrint(tz.getTimezoneName().c_str());
}

void transformWeather(int temp) {
  bool neg = temp < 0;
  if (neg) temp = -temp;
  bool big = temp > 9;

  if (big) {
      m1 = neg ? '-' : '.';
      m2 = (temp / 10) + '0';
      m3 = (temp % 10) + '0';
      m4 = 'c';
  } else {
      m1 = '.';
      m2 = neg ? '-' : temp + '0';
      m3 = neg ? temp + '0' : 'c';
      m4 = neg ? 'c' : '.';
  }
}

void write_vector(bool arr[], bool isSeg, bool isDot, int delay) {
  int max = 7;
  if (!isSeg) {
    max = 8;
  }
  for (int i = 0; i < max; ++i) {
    if (arr[i] == isSeg) {
      digitalWrite(SER, 1);
    } else {
      digitalWrite(SER, 0);
    }
    digitalWrite(S_CLK, 1);
    digitalWrite(S_CLK, 0);
  }

  if (isSeg) {
    if (isDot) {
      digitalWrite(SER, 1);
    } else {
      digitalWrite(SER, 0);
    }
    digitalWrite(S_CLK, 1);
    digitalWrite(S_CLK, 0);
  }

  if (isSeg) {
    digitalWrite(R_CLK, 1);
    digitalWrite(R_CLK, 0);
  }
  if (delay != 0) {
    os_delay_us(delay);
  }
}

bool* get_vector(char input) {
  switch (input) {
    case '.':
      return emty;
    case 'c':
      return digCelsium;
    case '-':
      return digMinus;
    case '0':
      return dig0;
    case '1':
      return dig1;
    case '2':
      return dig2;
    case '3':
      return dig3;
    case '4':
      return dig4;
    case '5':
      return dig5;
    case '6':
      return dig6;
    case '7':
      return dig7;
    case '8':
      return dig8;
    case '9':
      return dig9;
    default:
      return emty;
  }
}

int _hour;
int minut;
bool isDot;
int oldMinut = 0;

void loop() {
  events();
  _hour = tz.hour();
  minut = tz.minute();
  isDot = tz.second() % 2 == 1;

  if (minut != oldMinut && minut % 10 == 0) {
    write_vector(segMinor, false);
    write_vector(digCelsium, true);
    oldMinut = minut;
    weather = getWeather(weatherUrl);
    write_vector(emty, false);
    write_vector(emty, true);
    transformWeather(weather);
  }

  if (_hour < 10) {
    write_vector(seg0, false);
    write_vector(dig0, true, false, segDelayMajor);
  } else {
    write_vector(seg0, false);
    write_vector(digits[_hour / 10 % 10], true, false, segDelayMajor);
  }
  write_vector(seg1, false);
  write_vector(digits[_hour % 10], true, isDot, segDelayMajor);

  if (minut < 10) {
    write_vector(seg2, false);
    write_vector(dig0, true, isDot, segDelayMajor);
  } else {
    write_vector(seg2, false);
    write_vector(digits[minut / 10 % 10], true, isDot, segDelayMajor);
  }
  write_vector(seg3, false);
  write_vector(digits[minut % 10], true, false, segDelayMajor);

  write_vector(seg4, false);
  write_vector(get_vector(m1), true, false, segDelayMinor);
  write_vector(seg5, false);
  write_vector(get_vector(m2), true, false, segDelayMinor);
  write_vector(seg6, false);
  write_vector(get_vector(m3), true, false, segDelayMinor);
  write_vector(seg7, false);
  write_vector(get_vector(m4), true, isSaved, segDelayMinor);

  write_vector(emty, false);
  write_vector(emty, true);
}