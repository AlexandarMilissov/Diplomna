#include <SoftwareSerial.h>
#include <SPI.h>
#include <SD.h>

#define RX 2
#define TX 3

#define Motor1Dir 4
#define Motor1Step 5
#define Motor2Dir 6
#define Motor2Step 7

#define Laser 9

#define Button1 A4
#define Button2 A1
#define Button3 A2
#define Button4 A3

#define CS 10
#define baud 57600

SoftwareSerial mySerial(RX, TX); // RX, TX
File myFile;

char movement = '3';
bool isSavingImage = false;
long imageSize = 0;

#define timeToHeatUp 250
int timeToWork = 250;

int PosX = 0;
int PosY = 0;

void setup() {
  
  pinMode(Laser,OUTPUT);
  LaserWork(false);
  
  Serial.begin(baud);
  mySerial.begin(baud);
  delay(1000);
  
  pinMode(Motor1Step,OUTPUT);
  pinMode(Motor1Dir,OUTPUT);
  pinMode(Motor2Step,OUTPUT);
  pinMode(Motor2Dir,OUTPUT);
  
  
  pinMode(Button1,INPUT);
  pinMode(Button2,INPUT);
  pinMode(Button3,INPUT);
  pinMode(Button4,INPUT);
  
  

  if(!SD.begin(CS))
  {Serial.println("no");}
  else
  Serial.println("yes");

}

void loop()
{ 
  if (mySerial.available() > 0)
  {
    if(isSavingImage)
    {
      char c;
      while(mySerial.available() > 0 && imageSize > 0)
      {
        c = mySerial.read();
        myFile.write(c);
        Serial.print(c);
        imageSize--;
      }
          Serial.println();
          Serial.println(imageSize);
      if(imageSize <= 0)
      {
        isSavingImage = false;
        myFile.close();
          Serial.println("end");
        while(mySerial.available() > 0 )
        {
          c = mySerial.read();
        }
      }
      return;
    }
    String message = mySerial.readStringUntil('\r');
    message+="\r";

    if(message[0] != '\r')
    {
        if(message[0] == '8')
        {
          if(message[2] == '6')
          {
            LaserWork(true);
          }
          else if(message[2] == '7')
          {
            LaserWork(false);
          }
          else if(message[2] == '8')
          {
            String fileName = "";
            int i = 4;
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
          else if(message[2] == '9')
          {
            int i = 4;
            timeToWork = GetNumberFromString(message,i);
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
        }
        else
        {
        }
      }
    }
    
   

  
 
  switch(movement)
  {
    case '1':
      if(!digitalRead(Button1))
      {
        //up
         Mottor2Move(true);
      }
      else
      {
        delay(10);
      }
      break;
    case '2':
      if(!digitalRead(Button2))
      {
        //right
        Mottor1Move(false);
      }
      else
      {
        delay(10);
      }
      break;
    case '3':
      break;
    case '4':
      if(!digitalRead(Button3))
      {
        //down
         Mottor2Move(false);
      }
      else
      {
        PosX = 0;
        delay(10);
      }
      break;
    case '5':
      if(!digitalRead(Button4))
      {
        //left
        Mottor1Move(true);
      }
      else
      {
        PosY = 0;
        delay(10);
      }
      break;
    default:
      break;
      delay(1);
  }
}
void ProcessImage(String line)
{
  Serial.println(line);
  isSavingImage = true;
  int i = 2;
  long width = GetNumberFromString(line,i);
    i++;
  long height = GetNumberFromString(line,i);
    i++;
  imageSize = (width * height) / 8;
  if((width * height) % 8 != 0)
  {
      imageSize++;
  }
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
  SD.remove(fileName);
  myFile = SD.open(fileName, FILE_WRITE);
  myFile.println(width);
  myFile.println(height);
}

int GetNumberFromString(String line, int &i)
{
  int number = 0;
  while(true)
  {
    if(line[i]=='*')
    {
      break;
    }
    number *= 10;
    number += (line[i] - '0');
    i++;
  }
  return number;
}

void DrawImage(String fileName)
{
  myFile = SD.open(fileName, FILE_READ);
  if(!myFile)
  {
    //failed to open file
    return;
  }
  int width = 0;
  int height = 0;
  char c = myFile.read();
  while(c >= '0' && c <='9')
  {
    if(c=='\r')
    {
      continue;
    }
    height *= 10;
    height += (c - '0');
    c = myFile.read();
  }
  c = myFile.read();
  c = myFile.read();
 
  while(c >= '0' && c <='9')
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


  byte byteToRead = myFile.read();
  int currPos = 0;
  bool curr = byteToRead & 128;
  curr = !curr;
  byteToRead = byteToRead<<1;
  bool prev;

  
  int j = 0;
  int toBeX = 0;
  int toBeY = 0;

  for(int i = 0; i < width; i++)
  {
    for(; j < height; j++)
    {
      if(mySerial.available() > 0) 
      {
        LaserWork(false);
        myFile.close();
        MoveTo(0,0);
        return;
      }
      toBeY++;


        Serial.print(curr);
      if(curr)
      {
        MoveTo(toBeX, toBeY);
        LaserWork(true);
        if(!prev || j == 0)
        {
          delay(timeToHeatUp);
        }
        delay(timeToWork);
      }
      else //!curr
      {
        if(prev)
        {
          LaserWork(false);
        }
      }

      
      prev = curr;
      if(currPos == 7)
      {
        byteToRead = myFile.read();
        currPos = 0;
      }
      else
      {
        currPos++;
      }
      curr = byteToRead & 128;
      curr = !curr;
      byteToRead = byteToRead<<1;
      
      
      if(digitalRead(Button1) || toBeY >= height)
      {
        toBeY = 0;
        while(j < height - 1)
        {
          prev = curr;
          if(currPos == 7)
          {
            byteToRead = myFile.read();
            currPos = 0;
          }
          else
          {
            currPos++;
          }
          curr = byteToRead & 128;
          byteToRead = byteToRead<<1;
    
          j++;
        }
        break;
      }
    }
    LaserWork(false);
    toBeX++;
    j = 0;
    if(digitalRead(Button2) || toBeX >= width)
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

void MoveTo(int NewX, int NewY)
{
  while(PosX != NewX)
  {
    if(PosX < NewX)
    {
      Mottor1Step(false);
      PosX++;
    }
    else
    {
      Mottor1Step(true);
      PosX--;
    }
  }
  while(PosY != NewY)
  {
    if(PosY < NewY)
    {
      Mottor2Step(true);
      PosY++;
    }
    else
    {
      Mottor2Step(false);
      PosY--;
    }
  }
}


void Mottor1Move(bool dir, int moveSpeed)
{
  digitalWrite(Motor1Dir,dir);
  
  digitalWrite(Motor1Step,HIGH);
  delay(moveSpeed);
  
  digitalWrite(Motor1Step,LOW);
  delay(moveSpeed);
}
void Mottor1Step(bool dir, int moveSpeed)
{
  for(int i = 0; i < 20; i++)
  {
    Mottor1Move(dir, moveSpeed);
  }
}
void ResetMotor1()
{
  while(!digitalRead(Button4))
  {
    Mottor1Move(true);
  }
  PosX = 0;
}
void Mottor1Move(bool dir)
{
  digitalWrite(Motor1Dir,dir);
  
  digitalWrite(Motor1Step,HIGH);
  delay(1);
  
  digitalWrite(Motor1Step,LOW);
  delay(1);
}
void Mottor1Step(bool dir)
{
  for(int i = 0; i < 20; i++)
  {
    Mottor1Move(dir);
  }
}

void Mottor2Move(bool dir, int moveSpeed)
{
  digitalWrite(Motor2Dir,dir);
  
  digitalWrite(Motor2Step,HIGH);
  delay(moveSpeed);
  
  digitalWrite(Motor2Step,LOW);
  delay(moveSpeed);
}
void Mottor2Step(bool dir,int moveSpeed)
{
  for(int i = 0; i < 20; i++)
  {
    Mottor2Move(dir,moveSpeed);
  }
}
void ResetMotor2()
{
  while(!digitalRead(Button3))
  {
    Mottor2Move(false);
  }
  PosY = 0;
}
void Mottor2Move(bool dir)
{
  digitalWrite(Motor2Dir,dir);
  
  digitalWrite(Motor2Step,HIGH);
  delay(1);
  
  digitalWrite(Motor2Step,LOW);
  delay(1);
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
  ResetMotor1();
  ResetMotor2();
}
