const int pinLed = 13 ; 
void setup() {
  // put your setup code here, to run once:
    Serial.begin(9600);
    pinMode(pinLed,OUTPUT);
    digitalWrite(pinLed, LOW);
}

void loop() {
  String data = "";

  if(Serial.available())
  {
    data = Serial.readStringUntil('\n');
    Serial.println(data);
  }
  if ( data == "on")
  {
    digitalWrite(pinLed, HIGH);
    data = "";
    Serial.println("LED IS ON !");
  }
  else if ( data == "off")
  {
    digitalWrite(pinLed, LOW);
    data = "";
    Serial.println("LED IS OFF !");

  }
  // put your main code here, to run repeatedly:
  delay(50);
}
