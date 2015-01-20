With Distance
=============

With distance comes perspective. An art installation for the RunJumpDev
[Rules & Play 2015](http://runjumpdev.org/events/january-gallery-hop-rules-play/)
interactive art gallery (part of the LexArts Gallery Hop) January 16th, 2015.

## What is this?

## Usage

0. [Wire](https://code.google.com/p/arduino-new-ping/wiki/NewPing_Single_Pin_Sketch)
an ultrasonic sensor to three digital pins on your Arduino-compatible. Use one
of the sensors supported by the [NewPing library](https://code.google.com/p/arduino-new-ping/)
(the original installation used an HC-SR04 available from eBay/Amazon).
1. Edit the sketch in `firmware/` and adjust the pin numbers to match your
connections.
2. Program the Arduino-compatible with the sketch. Make sure the microcontroller
is set to act as a USB serial port.
3. Build the project in Unity 4.6+.
4. If necessary, you can create a `serial.conf` in the compiled application's
*_Data directory that contains a single newline-terminated line specifying the
serial device to use. The program defaults to COM4 (Windows), but a
[Teensy 2.0](http://pjrc.com/store/teensy_pins.html) shows up in Linux as
`/dev/ttyACM0`, for example.
5. Place the connected sensor/microcontroller facing out from the screen. In
the gallery, it was mounted to the TV stand post below a 40" TV.
5. Start the program. If the programmed Arduino and ultrasonic sensor are
attached, the display should track an object at 70-270cm in front of the
sensor. If not, simulate it by using the up and down arrow keys (does not work
as well).

Press `R` to reset to attract mode ("Come closer"). Press `D` to turn on debug
mode. While in debug mode, press `N` to go to the next story, `[` and `]` to
adjust the minimum sensor distance, `,` and `.` to adjust maximum sensor
distance. Changes to min/max are saved for future runs in Unity player
preferences.