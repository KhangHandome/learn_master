#include <Wire.h>

#define STM32_ADDR 0x20   // I2C slave address

void setup() {
  Serial.begin(115200);
  // SDA = D2 (GPIO4), SCL = D1 (GPIO5)
  Wire.begin(4, 5);       
  Wire.setClock(10000);   // set tốc độ 10 kHz (chậm nhất khuyến nghị)
  delay(1000);
  Serial.println("ESP8266 I2C Master ready at 10kHz");
}

void loop() {
  Wire.beginTransmission(STM32_ADDR);

  // Gửi 50 byte ( 1 -> 15)
  for (uint8_t i = 1; i <= 15; i++) {
    Wire.write(i);  
  }

  Wire.endTransmission();

  Serial.println("ESP8266 sent 50 bytes to STM32");
  delay(500);
}
