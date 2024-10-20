#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <Servo.h>  // Include the Servo library
// Replace these with your WiFi credentials
const char* ssid = "Praya Labs";
const char* password = "7kwe6f5cut";

// Replace this with your MQTT broker information
const char* mqtt_server = "5.196.78.28";
const int mqtt_port = 1883; // Use port 1883 for MQTT



Servo myservo;  // Create a servo object for the first servo
Servo myservo2;  // Create a servo object for the second servo

int pos = 70;
int speed = 100;
int low_limit = 0;
int high_limit = 120;

int zero1 = 98;  // Initial position for the first servo
int zero2 = 90;  // Initial position for the second servo

// Declare the 'command' variable to store the payload as a string
String command;

WiFiClient espClient;
PubSubClient client(espClient);

void setup_wifi() {
  delay(10);
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);

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
  Serial.print("Message received from topic [");
  Serial.print(topic);
  Serial.print("]: ");

  // Print the message received
  command="";
  for (int i = 0; i < length; i++) {
    command+=(char)payload[i];
  }
  Serial.print(command);
 
  
  Serial.println();
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Create a random client ID
    String clientId = "ESP8266Client-";
    clientId += String(random(0xffff), HEX);

    // Attempt to connect to the MQTT broker
    if (client.connect(clientId.c_str())) {
      Serial.println("connected");
      // Subscribe to the "dog-control" topic
      client.subscribe("dog-control");
      Serial.println("Subscribed to topic: dog-control");
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void forward() {
  myservo.write(zero1 + 22);
  delay(speed);
  myservo2.write(zero2 - 20);
  delay(speed);
  myservo.write(zero1 - 28);
  delay(speed);
  myservo2.write(zero2 + 40);
  delay(speed);

  Serial.println("Moving forward");
}

 void back() {
   myservo.write(zero1 - 20);
   delay(speed);
   myservo2.write(zero1 - 20);
   delay(speed);
   myservo.write(zero1 + 20);
   delay(speed);
   myservo2.write(zero1 + 20);
   delay(speed);

   Serial.println("Moving backward");
 }

 void right() {
  myservo.write(zero1 + 30);  // Move servo1 forward
  delay(speed);
  myservo2.write(zero2 + 30); // Move servo2 backward
  delay(speed);
  myservo.write(zero1 - 10);  // Move servo1 slightly backward
  delay(speed);
  myservo2.write(zero2 - 10); // Move servo2 slightly forward
  delay(speed);

  Serial.println("Turning right");
}

void left() {
  myservo.write(zero1 - 30);  // Move servo1 backward
  delay(speed);
  myservo2.write(zero2 - 30); // Move servo2 forward
  delay(speed);
  myservo.write(zero1 + 10);  // Move servo1 slightly forward
  delay(speed);
  myservo2.write(zero2 + 10); // Move servo2 slightly backward
  delay(speed);

  Serial.println("Turning left");
}

void setup() {
  Serial.begin(9600);

   myservo.attach(D4);  // Attach the first servo to GPIO5 (D1)
  myservo2.attach(D5);  // Attach the second servo to GPIO4 (D2)

  myservo.write(zero1);
  myservo2.write(zero2);
  
  setup_wifi();
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);
}

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

   if(command=="move forward"){
     forward(); 
    }
    if(command=="move backward"){
     back();
    }
    if(command=="turn right"){
    right();
    }
    if(command=="turn left"){
    left();
    }
}
