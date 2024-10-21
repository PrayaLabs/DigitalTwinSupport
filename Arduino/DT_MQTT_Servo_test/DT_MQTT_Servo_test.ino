#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <Servo.h>
#include <ArduinoJson.h> // Include the ArduinoJson library

// Update these with values suitable for your network.
const char* ssid = "Praya Labs";
const char* password = "7kwe6f5cut";
const char* mqtt_server = "91.121.93.94";

WiFiClient espClient;
PubSubClient client(espClient);
Servo servo1; // Servo1 for controlling through topic "Servo1"
Servo servo2; // Servo2 for controlling through topic "Servo2"

int servo1Pin = D5; // Servo1 connected to D5
int servo2Pin = D4; // Servo2 connected to D4
int servo1Angle = 0; // Initial angle for Servo1 (middle position)
int servo2Angle = 0; // Initial angle for Servo2 (middle position)

void setup_wifi() {
  delay(10);
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);

  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

void callback(char* topic, byte* payload, unsigned int length) {
  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("] ");
  
  // Allocate a buffer to store the incoming JSON
  const size_t capacity = JSON_OBJECT_SIZE(1) + 20;
  DynamicJsonDocument doc(capacity);

  // Copy the payload into a string (null-terminated)
  char jsonPayload[length + 1];
  memcpy(jsonPayload, payload, length);
  jsonPayload[length] = '\0';

  // Deserialize the JSON document
  DeserializationError error = deserializeJson(doc, jsonPayload);
  
  // Check if parsing succeeded
  if (error) {
    Serial.print(F("deserializeJson() failed: "));
    Serial.println(error.f_str());
    return;
  }

  // Extract the angle from the JSON object
  int angle = doc["angle"];
  Serial.print("Angle received: ");
  Serial.println(angle);

  // Control Servo1
  if (String(topic) == "Servo1") {
    servo1Angle = constrain(angle, 0, 180); // Set the servo to the received angle
    servo1.write(servo1Angle); // Write the new angle to Servo1
    Serial.print("Servo1 angle: ");
    Serial.println(servo1Angle);
  }

  // Control Servo2
  if (String(topic) == "Servo2") {
    servo2Angle = constrain(angle, 0, 180); // Set the servo to the received angle
    servo2.write(servo2Angle); // Write the new angle to Servo2
    Serial.print("Servo2 angle: ");
    Serial.println(servo2Angle);
  }
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    String clientId = "ESP8266Client-";
    clientId += String(random(0xffff), HEX);

    if (client.connect(clientId.c_str())) {
      Serial.println("connected");
      client.subscribe("Servo1"); // Subscribe to the topic for Servo1
      client.subscribe("Servo2"); // Subscribe to the topic for Servo2
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      delay(5000);
    }
  }
}

void setup() {
  Serial.begin(9600);
  setup_wifi();
  client.setServer(mqtt_server, 1883);
  client.setCallback(callback);

  servo1.attach(servo1Pin); // Attach Servo1 to pin D5
  servo2.attach(servo2Pin); // Attach Servo2 to pin D4

  servo1.write(servo1Angle); // Set initial position for Servo1
  servo2.write(servo2Angle); // Set initial position for Servo2
}

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();
}
