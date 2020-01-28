#define stepPin 3
#define dirPin 4
#define timeDelay 5
void setup() {
  pinMode(stepPin,OUTPUT);
  pinMode(dirPin,OUTPUT);
  pinMode(13,OUTPUT);
  digitalWrite(dirPin,LOW);
}

void loop() {
  digitalWrite(stepPin,HIGH);
  digitalWrite(13,HIGH);
  delay(timeDelay);
  digitalWrite(stepPin,LOW);
  digitalWrite(13,LOW);
  delay(timeDelay);
}
