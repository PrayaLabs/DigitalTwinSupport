#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <PubSubClient.h>
#include <Servo.h>

// ====== WIFI ======
const char* WIFI_SSID = "Praya Labs";
const char* WIFI_PASS = "7kwe6f5cut";

// ====== MQTT (TCP) ======
const char* MQTT_HOST = "5.196.78.28";   // <-- set your broker IP
const uint16_t MQTT_PORT = 1883;
const char* MQTT_TOPIC = "funobotz/kuttybot";

// ====== SERVO (mouth) ======
const int SERVO_PIN = 2;       // D4 = GPIO2
const int MOUTH_CLOSED = 180;  // your spec
const int MOUTH_OPEN   = 150;  // your spec
Servo mouth;

// ====== WEB SERVER (status) ======
ESP8266WebServer server(80);

// ====== MQTT Client ======
WiFiClient wifiClient;
PubSubClient mqtt(wifiClient);

// For UI
String lastLine = "(none yet)";

// --------- Timing knobs (word-level) ----------
const uint16_t BASE_PER_WORD_MS   = 220; // base open/close duration per word
const uint8_t  PER_CHAR_MS        = 25;  // add a bit for longer words
const uint16_t WORD_MIN_BEAT_MS   = 120; // don't go too short
const uint16_t SPACE_PAUSE_MS     = 80;  // gap between words
const uint16_t PUNCT_PAUSE_SHORT  = 180; // , ; :
const uint16_t PUNCT_PAUSE_LONG   = 280; // . ! ?
const uint16_t START_PAUSE_MS     = 120; // before speaking
const uint16_t END_PAUSE_MS       = 150; // after speaking

// ---------- Keep MQTT alive during waits ----------
void delayWithLoop(uint32_t ms) {
  uint32_t start = millis();
  while (millis() - start < ms) {
    mqtt.loop();
    yield();
    delay(2);
  }
}

// ---------- Primitive tokenizer helpers ----------
bool isWordChar(char c) {
  // Consider alnum/underscore as part of a word; tweak if you like
  return (c == '_' || isalnum((unsigned char)c));
}

bool isSpaceChar(char c) {
  return (c == ' ' || c == '\t' || c == '\n' || c == '\r');
}

bool isPunctChar(char c) {
  return (c == '.' || c == '!' || c == '?' || c == ',' || c == ';' || c == ':');
}

// ---------- Beat + durations ----------
void mouthBeat(uint16_t openMs) {
  // split openMs into open/close segments
  mouth.write(MOUTH_OPEN);
  delayWithLoop((uint16_t)(openMs * 0.6));
  mouth.write(MOUTH_CLOSED);
  delayWithLoop((uint16_t)(openMs * 0.4));
}

uint16_t wordDuration(const String& w) {
  // Longer words => slightly longer beat; add a tiny random jitter
  int jitter = random(-20, 25);
  int d = (int)BASE_PER_WORD_MS + (int)w.length() * (int)PER_CHAR_MS + jitter;
  if (d < (int)WORD_MIN_BEAT_MS) d = WORD_MIN_BEAT_MS;
  return (uint16_t)d;
}

// ---------- NEW: word-level mouth sync ----------
void speakWithMouthWords(const String& text) {
  mouth.write(MOUTH_CLOSED);
  delayWithLoop(START_PAUSE_MS);

  String word; word.reserve(32);
  for (size_t i = 0; i < text.length(); i++) {
    char c = text[i];

    if (isWordChar(c)) {
      // accumulate word characters
      word += c;
      continue;
    }

    // We hit a delimiter (space or punctuation). If we have a word, speak it.
    if (word.length() > 0) {
      mouthBeat(wordDuration(word));
      word = "";
    }

    // Handle delimiter pauses (closed mouth)
    if (isSpaceChar(c)) {
      delayWithLoop(SPACE_PAUSE_MS);
    } else if (isPunctChar(c)) {
      if (c == '.' || c == '!' || c == '?') {
        delayWithLoop(PUNCT_PAUSE_LONG);
      } else {
        delayWithLoop(PUNCT_PAUSE_SHORT);
      }
    } else {
      // other symbols => treat like a small pause
      delayWithLoop(SPACE_PAUSE_MS);
    }
  }

  // Flush the final word if the string ended with a word
  if (word.length() > 0) {
    mouthBeat(wordDuration(word));
  }

  mouth.write(MOUTH_CLOSED);
  delayWithLoop(END_PAUSE_MS);
}

// ---------- Status page ----------
void handleRoot() {
  String html =
    "<!doctype html><html><head><meta name='viewport' content='width=device-width,initial-scale=1'>"
    "<title>KuttyBot – MQTT Word Sync</title></head>"
    "<body style='font-family:sans-serif;max-width:700px;margin:24px auto;padding:0 12px'>"
    "<h2>KuttyBot – MQTT Word Sync</h2>"
    "<p><b>Wi-Fi:</b> " + WiFi.localIP().toString() + "</p>"
    "<p><b>MQTT:</b> " + String(mqtt.connected() ? "Connected" : "Disconnected") + "</p>"
    "<p><b>Topic:</b> " + String(MQTT_TOPIC) + "</p>"
    "<p><b>Last line:</b><br><pre style='white-space:pre-wrap;border:1px solid #ccc;padding:8px;'>" + lastLine + "</pre></p>"
    "<p>Publish text to <code>" + String(MQTT_TOPIC) + "</code>. Mouth opens once per word.</p>"
    "</body></html>";
  server.send(200, "text/html", html);
}

// ---------- MQTT Reconnect ----------
void mqttReconnect() {
  while (!mqtt.connected()) {
    Serial.print("Attempting MQTT connection...");
    String clientId = "KuttyBot-" + String(ESP.getChipId(), HEX);
    if (mqtt.connect(clientId.c_str())) {
      Serial.println("connected");
      mqtt.subscribe(MQTT_TOPIC);
      Serial.print("Subscribed to: "); Serial.println(MQTT_TOPIC);
    } else {
      Serial.print("failed, rc="); Serial.print(mqtt.state());
      Serial.println(" – retrying in 3 seconds");
      delay(3000);
    }
  }
}

// ---------- MQTT Callback ----------
void onMqttMessage(char* topic, byte* payload, unsigned int length) {
  String msg; msg.reserve(length);
  for (unsigned int i = 0; i < length; i++) msg += (char)payload[i];
  msg.trim();
  if (msg.length() == 0) return;
  if (msg.length() > 200) msg = msg.substring(0, 200); // small safety cap

  Serial.print("MQTT ["); Serial.print(topic); Serial.print("] ");
  Serial.println(msg);

  if (String(topic) == MQTT_TOPIC) {
    lastLine = msg;
    speakWithMouthWords(msg); // <-- WORD-LEVEL SYNC HERE
  }
}

void setup() {
  Serial.begin(9600);
  delay(100);

  // Servo
  mouth.attach(SERVO_PIN,500,2500);
  mouth.write(MOUTH_CLOSED);

  // WiFi
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASS);
  Serial.print("WiFi");
  while (WiFi.status() != WL_CONNECTED) { delay(300); Serial.print("."); }
  Serial.println("\nConnected. IP: " + WiFi.localIP().toString());

  // Web UI
  server.on("/", handleRoot);
  server.begin();
  Serial.println("HTTP server started");

  // MQTT
  mqtt.setServer(MQTT_HOST, MQTT_PORT);
  mqtt.setCallback(onMqttMessage);
  mqtt.setKeepAlive(30);
  mqttReconnect();
}

void loop() {
  if (!mqtt.connected()) mqttReconnect();
  mqtt.loop();
  server.handleClient();
}
