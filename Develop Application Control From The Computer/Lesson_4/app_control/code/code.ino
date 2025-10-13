#include "LiquidCrystal.h"
#include "DHT.h"   
/* Khai bao cac chan dung cho LCD */
/* Thu tu : RS,E,D4 D5 D6 D7*/
LiquidCrystal ManHinh(13,11,10,9,8,7);

const int DHTPIN = 2;      
const int DHTTYPE = DHT11; 

const int in1 = 5;
const int in2 = 4;
const int enable = 3;

int  speed = 0 ;

void setup() {
    // put your setup code here, to run once:
    Serial.begin(9600);
    /* Setup pin for l298d*/
    pinMode(in1, OUTPUT);
    pinMode(in2, OUTPUT);
    pinMode(enable, OUTPUT);

    digitalWrite(in1, LOW);
    digitalWrite(in2, LOW);
    digitalWrite(enable, LOW);
}

void loop() {
    String data = ""; 
    if(Serial.available())
    {
        data = Serial.readStringUntil('\n');
        speed = data.toInt();
    }
    if ( speed == 0 )
    {        
        digitalWrite(in1, LOW);
        digitalWrite(in2, LOW);
        digitalWrite(enable, LOW);
    }
    else 
    {
        digitalWrite(in1, LOW);
        digitalWrite(in2, HIGH);
        analogWrite(enable, speed);
    }
    delay(50);  
}

