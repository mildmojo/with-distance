#include <NewPing.h>

#define PING_PIN 0  // Arduino pin tied to both trigger and echo pins on the ultrasonic sensor.
#define LED_PIN 6
#define MAX_DISTANCE 400 // Maximum distance we want to ping for (in centimeters). 
                         // Maximum sensor distance is rated at 400-500cm.

NewPing sonar(PING_PIN, PING_PIN, MAX_DISTANCE); // NewPing setup of pin and maximum distance.
unsigned int last_reading = 0;

void setup() {
  Serial.begin(57600); // Open serial monitor at 115200 baud to see ping results.
  pinMode(LED_PIN, OUTPUT);
}

void loop() {
  // Rate limiting.
  delay(30);

  // Send ping, get ping time in microseconds (uS).
  // (0 means no echo or beyond max distance)
  digitalWrite(LED_PIN, last_reading > 0);
  last_reading = sonar.ping();
  serial_print(last_reading);
  digitalWrite(LED_PIN, 0);
}

void serial_print(unsigned int uS) {
  Serial.print(uS);
  Serial.print("\n");
}

