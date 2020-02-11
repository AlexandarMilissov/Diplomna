#include <SoftwareSerial.h>

#define RX 2
#define TX 3

#define Motor1Dir 4
#define Motor1Step 5
#define Motor2Dir 6
#define Motor2Step 7
#define TimeDelay 1

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
  
  pinMode(Motor1Step,OUTPUT);
  pinMode(Motor1Dir,OUTPUT);
  pinMode(Motor2Step,OUTPUT);
  pinMode(Motor2Dir,OUTPUT);
  
  Serial.begin(9600);
  while (!Serial) {
  }

  Serial.println("Goodnight moon!");
  mySerial.begin(9600);
  Serial.println("Goodnight moon!");
}

/*
 * 1 Up
 * 2 Left
 * 3 Stop
 * 4 Right
 * 5 Down
 * 6 Start
 * 7 End
 */
char movement = '3';
void loop()
{
  if (mySerial.available())
  {
    String message = mySerial.readString();
    if(message[0] == '8')
    {
      if(message[2] == '6')
      {
        analogWrite(Laser, 255);
      }
      else if(message[2] == '6')
      {
        analogWrite(Laser, 0);
      }
      else
      {
        movement = message[2];
      }
    }
    else if(message[0] == '6')
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
  switch(movement)
  {
    case '1':
      Mottor1Move(true);
      break;
    case '2':
      Mottor2Move(false);
      break;
    case '3':
      break;
    case '4':
      Mottor2Move(true);
      break;
    case '5':
      Mottor1Move(false);
      break;
    default:
      break;
  }
}
void Mottor1Move(bool dir)
{
  digitalWrite(Motor1Dir,dir);
  
  digitalWrite(Motor1Step,HIGH);
  delay(TimeDelay);
  
  digitalWrite(Motor1Step,LOW);
  delay(TimeDelay);
}

void Mottor2Move(bool dir)
{
  digitalWrite(Motor2Dir,dir);
  
  digitalWrite(Motor2Step,HIGH);
  delay(TimeDelay);
  
  digitalWrite(Motor2Step,LOW);
  delay(TimeDelay);
}
