#include <ESP8266WiFi.h>
#include <ThingSpeak.h>
#include <Servo.h>

// ---------- WiFi ----------
const char* ssid = "YOUR_WIFI_NAME";
const char* password = "YOUR_WIFI_PASSWORD";

// ---------- ThingSpeak ----------
unsigned long channelNumber = YOUR_CHANNEL_ID;
const char * writeAPIKey = "YOUR_WRITE_API_KEY";

WiFiClient client;

// ---------- Pins ----------
#define IR_PIN D2
#define GREEN_PIN D1
#define RED_PIN D0
#define SERVO_PIN D4

Servo gateServo;

int sensorState;
int lastState = -1;

void setup()
{
  Serial.begin(9600);

  pinMode(IR_PIN, INPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(RED_PIN, OUTPUT);

  gateServo.attach(SERVO_PIN);

  digitalWrite(GREEN_PIN, LOW);
  digitalWrite(RED_PIN, HIGH);
  gateServo.write(0);

  // WiFi connect
  Serial.print("Connecting to WiFi");

  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  Serial.println();
  Serial.println("WiFi Connected");

  ThingSpeak.begin(client);
}

void loop()
{
  sensorState = digitalRead(IR_PIN);

  // Vehicle detected
  if(sensorState == HIGH)
  {
    digitalWrite(GREEN_PIN, HIGH);
    digitalWrite(RED_PIN, LOW);

    gateServo.write(90);

    Serial.println("Vehicle detected -> Gate OPEN");

    if(lastState != 1)
    {
      ThingSpeak.setField(1, 1);
      ThingSpeak.writeFields(channelNumber, writeAPIKey);
      lastState = 1;
    }
  }

  // No vehicle
  else
  {
    digitalWrite(GREEN_PIN, LOW);
    digitalWrite(RED_PIN, HIGH);

    gateServo.write(0);

    Serial.println("No vehicle -> Gate CLOSED");

    if(lastState != 0)
    {
      ThingSpeak.setField(1, 0);
      ThingSpeak.writeFields(channelNumber, writeAPIKey);
      lastState = 0;
    }
  }

  delay(2000); // ThingSpeak safe update delay
}