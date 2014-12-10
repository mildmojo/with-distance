#include <NewPing.h>

#define PING_PIN 0  // Arduino pin tied to both trigger and echo pins on the ultrasonic sensor.
#define LED_PIN 6
#define MAX_DISTANCE 400 // Maximum distance we want to ping for (in centimeters). 
                         // Maximum sensor distance is rated at 400-500cm.

NewPing sonar(PING_PIN, PING_PIN, MAX_DISTANCE); // NewPing setup of pin and maximum distance.

void setup() {
  Serial.begin(57600); // Open serial monitor at 115200 baud to see ping results.
  pinMode(LED_PIN, OUTPUT);
}

void loop() {
  delay(30);
  digitalWrite(LED_PIN, 1);
  unsigned int uS = sonar.ping(); // Send ping, get ping time in microseconds (uS).
  serial_print(uS);
  digitalWrite(LED_PIN, 0);
}

void serial_print(unsigned int uS) {
  //Serial.print("Ping: ");
  Serial.print(uS);
  Serial.print("\n"); // Convert ping time to distance and print result (0 = outside set distance range, no ping echo)
  //Serial.println(" uS");
}

