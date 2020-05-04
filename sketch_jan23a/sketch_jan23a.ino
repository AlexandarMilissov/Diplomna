#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

const char* ssid = "ASUS";
const char* password = "PowerNetwork";
const char* thisDeviceName = "Device1";

unsigned int PORT = 4210;
char incomingPacket[5];
WiFiServer server(PORT);
WiFiClient client;
WiFiUDP Udp;
bool isBinded = false;

unsigned long LastTimeSend = 0;
unsigned long LastTimeReceived = 0;
#define keepAliveTimer 2000

bool isSavingImage = false;
int imageSize = 0;

void setup() 
{
  
  WiFi.begin(ssid, password);
  
  /*
    digitalWrite(1,HIGH);
    delay(1000);
    digitalWrite(1,LOW);
  
  for (int i = 0; i< 5; i++);
  {
    delay(500);
  }
  */
 
  Serial.begin(9600);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
  }
  Udp.begin(PORT);
  server.begin();
  server.setNoDelay(true);
}
int count = 0;
void loop()
{
  CheckForDiscoverMessages();
  
  client = server.available();
  if(client.connected())
  {
    client.setNoDelay(true);
    //start timer for keep alive
    LastTimeReceived = millis();
  }
  
  while (client.connected())
  {
    if(!isSavingImage)
    {
      //check to see if its time to send keepalive
      if(millis() - LastTimeSend>keepAliveTimer)
      {
        SendKeepAlive();
      }
      //drop connection if keepalive is not received
      if(millis() - LastTimeReceived > 4*keepAliveTimer)
      {
        client.stop();
        return;
      }
      //check if there is anything to read
      if (!client.available())
      {
        continue;
      }
      
      String line = client.readStringUntil('\r');
      ProcessMessage(line);
    }
    else
    {
      if(client.available() <= 0)
      {
        continue;
      }
      LastTimeReceived = millis();
      char c;
      while(client.available() > 0 && imageSize > 0)
      {
        while(Serial.availableForWrite() <= 1)
        {
        }
        c = client.read();
        Serial.write(c);
        imageSize--;
      }
      
      if(imageSize <= 0)
      {
        isSavingImage = false;
      }
    }
  }
}
void CheckForDiscoverMessages()
{
  int packetSize = Udp.parsePacket();
  if (packetSize)
  {
    int len = Udp.read(incomingPacket, 1024);
    if (len > 0)
    {
      incomingPacket[len] = 0;
    }
    if(incomingPacket[0]=='0')
    {
        SendDiscoverReply();
    }
  }
}
void ProcessMessage(String line)
{
  switch (line[0])
    {
      case '6':
        Serial.print(line);
        Serial.printf("\r");
        ProcessImage(line);
        break;
      case '8':
        Serial.print(line);
        Serial.printf("\r");
        break;
      case 'a':
        LastTimeReceived = millis();
        break;
      default:
        break;
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
  imageSize = (width * height) / 8;
  if((width * height) % 8 != 0)
  {
      imageSize++;
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
void SendKeepAlive()
{
  LastTimeSend = millis();
  client.println("a");
}
void SendDiscoverReply()
{
  Udp.beginPacket(Udp.remoteIP(), PORT);
  Udp.write("1*");
  Udp.write(thisDeviceName);
  Udp.endPacket();
}
