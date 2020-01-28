#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#define keepAliveTimer 2000

const char* ssid = "ASUS";
const char* password = "PowerNetwork";

WiFiUDP Udp;
IPAddress BindIP (255,255,255,255);
bool isBinded = false;
unsigned int PORTR = 4210;  // local port to listen on
unsigned int PORTS = 4211;  // local port to send to
char incomingPacket[1024];  // buffer for incoming packets

unsigned long LastTimeSend = 0;
unsigned long LastTimeReceived = 0;


void setup()
{
  pinMode(0, INPUT);
  Serial.begin(9600);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
  }

  Udp.begin(PORTR);
}


void loop()
{
  if(isBinded == true)
  {
    if(millis() - LastTimeSend>keepAliveTimer)
    {
      LastTimeSend = millis();
      Udp.beginPacket(BindIP, PORTS);
      Udp.write("a*");
      Udp.endPacket();
    }
  }



  
  if(isBinded!=false && millis() - LastTimeReceived > 4*keepAliveTimer)
  {
    isBinded = false;
  }
  int packetSize = Udp.parsePacket();
  
  if (packetSize)
  {
    int len = Udp.read(incomingPacket, 1024);
    if (len > 0)
    {
      incomingPacket[len] = 0;
    }
    //0 discover
    //1 discover reply
    //2 bind request
    //3 bind reply
    //4 bind drop
    //5 bind drop reply
    //6 image received
    //7 image received acknowledge
    //8 draw command
    //9 draw command reply
    //a keepalive
    
    //Serial.printf("UDP packet contents: %s\n", incomingPacket);
    switch (incomingPacket[0])
    {
      case '0':
        SendDiscoverReply();
        break;
      case '2':
        if (isBinded == false || BindIP == Udp.remoteIP())
        {
          BindIP = Udp.remoteIP();
          isBinded = true;
        }
        BindReply(isBinded);
        break;
      case '4':
        if (BindIP == Udp.remoteIP())
        {
          isBinded = false;
          BindDropReply();
        }
        break;
      case '6':
        Serial.print(incomingPacket);
        Serial.printf("\0");
        break;
      case '8':
        break;
      case 'a':
        if(isBinded == false)
        {
          Serial.printf("Keepalive received when doesnt need\n");
          SendErrorMessage("received keep alive when no bind was present");
          break;
        }
        //Serial.printf("Keepalive received\n");
        LastTimeReceived = millis();
        break;
      default:
        break;
    }
  }
}
void SendErrorMessage(char* s)
{
  Udp.beginPacket(Udp.remoteIP(), PORTS);
  Udp.write(s);
  Udp.endPacket();
}
void BindReply(bool b)
{
  Udp.beginPacket(Udp.remoteIP(), PORTS);
  if (b)
  {
    Udp.write("3*1");
    LastTimeReceived = millis();
  }
  else
  {
    Udp.write("3*0");
  }
  Udp.endPacket();
}
void BindDropReply()
{
  Udp.beginPacket(Udp.remoteIP(), PORTS);
  Udp.write("5*");
  Udp.endPacket();
}
void SendDiscoverReply()
{
  Udp.beginPacket(Udp.remoteIP(), PORTS);
  Udp.write("1*");
  Udp.endPacket();
}
