#include <SoftwareSerial.h>

#define RX 2
#define TX 3

#define Motor1Dir 4
#define Motor1Step 5
#define Motor2Dir 6
#define Motor2Step 7

#define Laser 9

#define Button1 9
#define Button2 10
#define Button3 11
#define Button4 12

int PosX = 0;
int PosY = 0;

int Width;
int Height;
byte *image;
SoftwareSerial mySerial(RX, TX); // RX, TX

void setup() {
  Serial.begin(9600);
  while (!Serial) {
  }

  Serial.println("Goodnight moon!");
  mySerial.begin(9600);
  Serial.println("Goodnight moon!");
}
void loop()
{
  if (mySerial.available())
  {
    String message = mySerial.readString();
    if(message[0] == '6')
    {
      Width = message[2];
      Width<<8;
      Width | message[3];
      Height = message[5];
      Height<<8;
      Height | message[6];
      image = message[8];
    }
  }
}
