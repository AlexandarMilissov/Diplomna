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


void setup() 
{
  
  Serial.begin(9600);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
  }
  Udp.begin(PORT);
  server.begin();
  server.setNoDelay(true);
}

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
    //check to see if its time to send keepalive
    if(millis() - LastTimeSend>keepAliveTimer)
    {
      SendKeepAlive();
    }
    //drop connection if keepalive is not received
    if(millis() - LastTimeReceived > 4*keepAliveTimer)
    {
      Serial.println("why");
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
        Serial.printf("\0");
        break;
      case '8':
        Serial.print(line);
        Serial.printf("\0");
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
  
  int i = 2;
  int width = GetNumberFromString(line,&i);
  int height = GetNumberFromString(line,&i);
  int numberPackets = GetNumberFromString(line,&i);
  int lastPacketSize = GetNumberFromString(line,&i);
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
}
int GetNumberFromString(String line, int *i)
{
  int number = 0;
  while(true)
  {
    if(line[*i]=='*')
    {
      break;
    }
    number *= 10;
    number += (line[*i] - '0');
    *i++;
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
