#include <Servo.h>  // Include the Servo library

Servo myservo;  // Create a servo object for the first servo
Servo myservo2;  // Create a servo object for the second servo

int pos = 70;
int speed = 100;
int low_limit = 0;
int high_limit = 120;

int zero1 = 98;  // Initial position for the first servo
int zero2 = 90;  // Initial position for the second servo

void setup() {
  Serial.begin(9600);
  myservo.attach(D4);  // Attach the first servo to GPIO5 (D1)
  myservo2.attach(D5);  // Attach the second servo to GPIO4 (D2)

  myservo.write(zero1);
  myservo2.write(zero2);
}

void loop() {
  // You can call these functions inside loop() or trigger them based on some condition
  //forward();  // Example: Move forward
  delay(200);  // Wait for 2 seconds
  // left();  // Example: Turn left
  // delay(100);
  // right();  // Example: Turn right
  // delay(200);
  // back();  // Example: Move backward
  // delay(1000);
  // happy();  // Example: Happy movement
  // delay(300);
  //  stopMovement();  // Stop servos
  //  delay(500);
// myservo2.write(zero2 - 20);
// delay(1000);
// myservo2.write(zero2 + 40);
// delay(1000);
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

// void left() {
//   myservo.write(zero1 - 40);
//   delay(speed);
//   myservo2.write(zero2 - 20);
//   delay(speed);
//   myservo.write(zero1 + 20);
//   delay(speed);
//   myservo2.write(zero2 + 20);
//   delay(speed * 2);

//   Serial.println("Turning left");
// }

// void right() {
//   myservo.write(zero1 + 40);
//   delay(speed);
//   myservo2.write(zero2 + 20);
//   delay(speed);
//   myservo.write(zero1 - 20);
//   delay(speed);
//   myservo2.write(zero2 - 20);
//   delay(speed * 2);

//   Serial.println("Turning right");
// }

// void back() {
//   myservo.write(zero1 - 20);
//   delay(speed);
//   myservo2.write(zero1 - 20);
//   delay(speed);
//   myservo.write(zero1 + 20);
//   delay(speed);
//   myservo2.write(zero1 + 20);
//   delay(speed);

//   Serial.println("Moving backward");
// }

// void happy() {
//   int swing = 12;
//   for (pos = zero1 - swing; pos <= zero1 + swing; pos += 1) {
//     myservo.write(pos);
//     delay(5);
//   }
//   for (pos = zero1 + swing; pos >= zero1 - swing; pos -= 1) {
//     myservo.write(pos);
//     delay(5);
//   }

//   Serial.println("Servo is happy");
// }

// void stopMovement() {

//   myservo.write(zero1);
//   myservo2.write(zero2);

//   Serial.println("Stopped");
// }
