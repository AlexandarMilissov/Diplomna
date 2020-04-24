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
/*
#define Button1 14
#define Button2 15
#define Button3 16
#define Button4 17
*/
#define Button1 A4
#define Button2 A1
#define Button3 A2
#define Button4 A3

#define CS 10

SoftwareSerial mySerial(RX, TX); // RX, TX
File myFile;

char movement = '3';
bool isSavingImage = false;
int numberPackets = 0;
int lastPacketSize = 0;

int timeToHeatUp = 20;
int timeToWork = 7;

char image[1000];

void setup() {
  
  pinMode(Laser,OUTPUT);
  LaserWork(false);
  
  Serial.begin(9600);
  while (!Serial) {}
  mySerial.begin(9600);
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

  ResetMotors();
}

void loop()
{  
  if (mySerial.available())
  {
    if(isSavingImage)
    {
        Serial.println(isSavingImage);
      int nReads = 1000;
      String message = "";
      char c;
      if(numberPackets == 1)
      {
        nReads = lastPacketSize;
        Serial.println(nReads);
      }
      for(int i = 0; i < nReads; i++)
      {
        c = mySerial.read();
        if(c==0)
        {
          c = '1';
        }
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
  isSavingImage = true;
  int i = 2;
  int width = GetNumberFromString(line,i);
    i++;
  int height = GetNumberFromString(line,i);
    i++;
  numberPackets = GetNumberFromString(line,i);
    i++;
  lastPacketSize = GetNumberFromString(line,i);
    i++;
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
  numberPackets--;
  
  if(numberPackets == 0)
  {
    isSavingImage = false;
    myFile.close();
  }
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
    width *= 10;
    width += (c - '0');
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
    height *= 10;
    height += (c - '0');
    c = myFile.read();
  }
  c = myFile.read();


  byte next8 = myFile.read();
  
              /*Serial.print("|");
              Serial.print(next8);
              Serial.print("| ");*/
  int nextPos = 1;
  bool curr = next8 & 128;
  
              if(curr)
                Serial.print("1 ");
              else
                Serial.print("0 ");
  next8 = next8<<1;
  bool next = next8 & 128;
  //Serial.println(next8);

  
  int j = 1;
  for(int i = 0; i < width; i++)
  {
    LaserWork(next);
    for(; j < height; j++)
    {
      if(mySerial.available() > 0) 
      {
        LaserWork(false);
        myFile.close();
        return;
      }
      
      if(Serial.read() == 'e')
      {
        LaserWork(false);
        myFile.close();
        return;
      }

      
      if(next != curr)
      {
        LaserWork(next);
      } 
      
      Mottor2Step(true,timeToWork);
      curr = next;
      
              if(curr)
                Serial.print("1 ");
              else
                Serial.print("0 ");
                
      if(nextPos == 7)
      {
        next8 = myFile.read();
        nextPos = 0;
        
              //Serial.print("|| ");
      }
      else
      {
        nextPos++;
      }
      next = next8 & 128;
            /*if(nextPos == 0)
            {
              Serial.print("|");
              Serial.print(next8);
              Serial.print("| ");
            }*/
      next8 = next8<<1;
      
      if(digitalRead(Button1))
      {
        break;
      }
    }
    LaserWork(false);
    ResetMotor2();
    Mottor1Step(false);
    j = 0;
    Serial.println();
    if(digitalRead(Button2))
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
     delay(timeToWork);
  }
  else
  {
    analogWrite(Laser, 0);
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
    Mottor1Move(true,1);
  }
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
    Mottor2Move(false,1);
  }
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
/*void ResetMotors()
{
  ResetMotor1();
  int i = 0;
  while(!digitalRead(Button2))
  {
    Mottor1Move(false);
    i++;
  }
  Serial.println(i);
  ResetMotor2();
  i = 0;
  while(!digitalRead(Button1))
  {
    Mottor2Move(true);
    i++;
  }
  Serial.println(i);
}*/
