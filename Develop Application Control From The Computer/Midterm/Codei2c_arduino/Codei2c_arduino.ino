#include <Wire.h>
#include <LiquidCrystal_I2C.h>

LiquidCrystal_I2C Screen1(0x20, 16, 2);

LiquidCrystal_I2C Screen2(0x21, 16, 2);

LiquidCrystal_I2C Screen3(0x22, 16, 2);


void setup() {
  Screen1.init();
  Screen1.backlight();
  Screen1.setCursor(0,0);
  Screen1.print("KhangMT");
  Screen1.setCursor(0,1);
  Screen1.print("Test Screen On");

  Screen2.init();
  Screen2.backlight();
  Screen2.setCursor(0,0);
  Screen2.print("Development");
  Screen2.setCursor(0,1);
  Screen2.print("Three");

  Screen3.init();
  Screen3.backlight();
  Screen3.setCursor(0,0);
  Screen3.print("FPT Software");
  Screen3.setCursor(0,1);
  Screen3.print("Display");
}

void loop() {}
