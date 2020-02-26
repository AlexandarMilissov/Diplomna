#include <SoftwareSerial.h>
#include <SPI.h>
#include <SD.h>

#define RX 2
#define TX 3

#define Motor1Dir 4
#define Motor1Step 5
#define Motor2Dir 6
#define Motor2Step 7
#define TimeDelay 1
#define timeToWork 10

#define Laser 9

#define Button1 14
#define Button2 15
#define Button3 16
#define Button4 17

#define CS 10

SoftwareSerial mySerial(RX, TX); // RX, TX
File myFile;

char movement = '3';
bool isSavingImage = false;
int numberPackets = 0;
int lastPacketSize = 0;

void setup() {
  
  pinMode(Motor1Step,OUTPUT);
  pinMode(Motor1Dir,OUTPUT);
  pinMode(Motor2Step,OUTPUT);
  pinMode(Motor2Dir,OUTPUT);
  pinMode(Button1,INPUT);
  pinMode(Button2,INPUT);
  pinMode(Button3,INPUT);
  pinMode(Button4,INPUT);
  
  SD.begin(CS);
  Serial.begin(9600);
  delay(1000);

  mySerial.begin(9600);
  pinMode(13,OUTPUT);

//  ResetMotors();
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
void loop()
{
  if (mySerial.available())
  {
    if(isSavingImage)
    {
      int nReads = 1000;
      String message = "";
      char c;
      if(numberPackets == 1)
      {
        nReads = lastPacketSize;
      }
      numberPackets--;
      for(int i = 0; i < nReads;i++)
      {
        c = mySerial.read();
        if(c==-1)
        {
          i--;
          continue;
        }
        message += c;
      }
      SavePartOfImage(message);
      return;
    }
    String message = mySerial.readString();
    if(message[0] == '8')
    {
      if(message[2] == '6')
      {
        analogWrite(Laser, 255);
      }
      else if(message[2] == '7')
      {
        analogWrite(Laser, 0);
      }
      else if(message[2] == '8')
      {
        String fileName = "";
        int i = 2;
        while(true)
        {
          if(message[i]=='\r')
          {
            break;
          }
          fileName += message[i];
          i++;
        }
        DrawImage(fileName);
      }
      else
      {
        movement = message[2];
      }
    }
    else if(message[0] == '6')
    {
      if(!isSavingImage)
      {
        ProcessImage(message);
      }
      else
      {
        SavePartOfImage(message);
      }
    }
  }

  
  if(digitalRead(Button1))
  {
    delay(10);
        Serial.println("Button1");
    return;
  }
 if(digitalRead(Button2))
  {
    delay(10);
        Serial.println("Button2");
    return;
  }
  if(digitalRead(Button3))
  {
    delay(10);
        Serial.println("Button3");
    return;
  }
  if(digitalRead(Button4))
  {
    delay(10);
        Serial.println("Button4");
    return;
  }
  switch(movement)
  {
    case '1':
      if(!digitalRead(Button4))
      {
       Mottor1Move(true);
      }
      else
      {
        delay(10);
        Serial.println("Button4");
      }
      break;
    case '2':
      if(!digitalRead(Button1))
      {
        Mottor2Move(false);
      }
      else
      {
        delay(10);
        Serial.println("Button1");
      }
      break;
    case '3':
      break;
    case '4':
      if(!digitalRead(Button3))
      {
        Mottor2Move(true);
      }
      else
      {
        delay(10);
        Serial.println("Button3");
      }
      break;
    case '5':
      if(!digitalRead(Button2))
      {
        Mottor1Move(false);
      }
      else
      {
        delay(10);
        Serial.println("Button2");
      }
      break;
    default:
      break;
  }
}
void SavePartOfImage(String message)
{
  int nRead = 1000;
  if(numberPackets == 1)
  {
    nRead = lastPacketSize;
  }
  for(int i = 0; i < nRead + 0; i++)
  {
    myFile.print(message[i]);
  }
  if(numberPackets == 0)
  {
    isSavingImage = false;
    myFile.close();
  }
}

void DrawImage(String fileName)
{
  myFile = SD.open(fileName, FILE_READ);
  int width = 0;
  int height = 0;
  char c = myFile.read();
  while(c != '\n')
  {
    if(c=='\r')
    {
      continue;
    }
    width *= 10;
    width += (c - '0');
    c = myFile.read();
  }
  c = myFile.read();
  while(c != '\n')
  {
    if(c=='\r')
    {
      continue;
    }
    height *= 10;
    height += (c - '0');
    c = myFile.read();
  }



  byte next8 = myFile.read();
  int count = 1;
  bool curr = next8 & 1;
  next8>>1;
  bool next = next8 & 1;
  LaserWork(curr);
  delay(timeToWork);
  for(int i = 0; i < width; i++)
  {
    for(int j = 0; j < height; j++)
    {
      if(next != curr)
      {
        LaserWork(next);
        delay(1);
      }
      Mottor1Step(true);
      curr = next;
      if(count == 7)
      {
        next8 = myFile.read();
        count = 0;
      }
      else
      {
        count++;
      }
      next = next8 & 1;
      next8>>1;
      
      if(!digitalRead(Button1)||!digitalRead(Button2))
      {
        break;
      }
    }
    if(!digitalRead(Button2)||!digitalRead(Button4))
    {
      break;
    }
  }
  ResetMotors();
  myFile.close();
}

void LaserWork(bool work)
{
  if(work)
  {
     analogWrite(Laser, 255);
  }
  else
  {
    analogWrite(Laser, 0);
  }
}
void ProcessImage(String line)
{
  isSavingImage = true;
  int i = 2;
  int width = GetNumberFromString(line,&i,'*');
  int height = GetNumberFromString(line,&i,'*');
  numberPackets = GetNumberFromString(line,&i,'*');
  lastPacketSize = GetNumberFromString(line,&i,'*');
  String fileName = "";
  while(true)
  {
    if(line[i]=='\r')
    {
      break;
    }
    fileName += line[i];
    i++;
  }
  fileName = "test.txt";
  myFile = SD.open(fileName, FILE_WRITE);
  myFile.println(width);
  myFile.println(height);
}
int GetNumberFromString(String line, int *i, char symbol)
{
  int number = 0;
  while(true)
  {
    if(line[*i]==symbol)
    {
      break;
    }
    number *= 10;
    number += (line[*i] - '0');
    *i++;
  }
  return number;
}



void Mottor1Move(bool dir)
{
  digitalWrite(Motor1Dir,dir);
  
  digitalWrite(Motor1Step,HIGH);
  delay(TimeDelay);
  
  digitalWrite(Motor1Step,LOW);
  delay(TimeDelay);
}
void ResetMotor1()
{
  while(!digitalRead(Button1)||!digitalRead(Button2))
  {
    Mottor1Move(false);
  }
}
void Mottor1Step(bool dir)
{
  for(int i = 0; i < 20; i++)
  {
    Mottor1Move(dir);
  }
}


void Mottor2Move(bool dir)
{
  digitalWrite(Motor2Dir,dir);
  
  digitalWrite(Motor2Step,HIGH);
  delay(TimeDelay);
  
  digitalWrite(Motor2Step,LOW);
  delay(TimeDelay);
}
void ResetMotor2()
{
  while(!digitalRead(Button2)||!digitalRead(Button4))
  {
    Mottor2Move(false);
  }
}
void Mottor2Step(bool dir)
{
  for(int i = 0; i < 20; i++)
  {
    Mottor2Move(dir);
  }
}

void ResetMotors()
{
  Serial.println("reseting motors");
  ResetMotor1();
  ResetMotor2();
}
