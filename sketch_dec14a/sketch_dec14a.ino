#include <SoftwareSerial.h>
#define espRX 2
#define espTX 3
String s;
SoftwareSerial mySerial(espRX, espTX); // RX, TX
void setup() {
  Serial.begin(9600);
  while (!Serial) {
  }

  mySerial.begin(9600);
  Serial.println("Goodnight moon!");
}
void loop()
{
  if (mySerial.available())
  {
    s = mySerial.readString();
    if(CheckData(s))
    {
      Draw(s);
    }
    Serial.print(s);
  }
}
int CheckData(String s)
{
  if(true)
  {
    return true;
  }
  else
  {
    Error();
    return false;  
  }
}
void Draw(String s)
{
  
}
String Error()
{
  mySerial.write("bla");
}
