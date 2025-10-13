#include "LiquidCrystal.h"

/* Khai bao cac chan dung cho LCD */
/* Thu tu : RS,E,D4 D5 D6 D7*/
LiquidCrystal ManHinh(13,11,10,9,8,7);

void setup() {
    // put your setup code here, to run once:
    Serial.begin(9600);
    ManHinh.begin(16,02);
    ManHinh.setCursor(0,0);
  // put your setup code here, to run once:
    ManHinh.print("LCD 16x02 Control");
    ManHinh.setCursor(0,1);
    ManHinh.print("Start connecting");
}

void loop() {
    if (Serial.available()) {
        // Đọc đến khi gặp '\n'
        String data = Serial.readStringUntil('\n');
        if (data.length() > 1) {
            char line = data.charAt(0);   // '1' hoặc '2'
            String message = data.substring(1); // phần nội dung

            if (line == '1') {
                ManHinh.setCursor(0, 0);   // dòng 1
                ManHinh.print("                "); // clear dòng
                ManHinh.setCursor(0, 0);
                ManHinh.print(message);
            }
            else if (line == '2') {
                ManHinh.setCursor(0, 1);   // dòng 2
                ManHinh.print("                "); // clear dòng
                ManHinh.setCursor(0, 1);
                ManHinh.print(message);
            }
        }
    }
}

