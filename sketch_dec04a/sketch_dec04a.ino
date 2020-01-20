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
  Serial.println();

  Serial.printf("Connecting to %s ", ssid);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.println(" connected");

  Udp.begin(PORTR);
  Serial.printf("Now listening at IP %s, UDP port %d\n", WiFi.localIP().toString().c_str(), PORTR);
}


void loop()
{
  if(isBinded == true)
  {
    if(millis() - LastTimeSend>keepAliveTimer)
    {
      LastTimeSend = millis();
      Serial.printf("keepalive send\n");
      Udp.beginPacket(BindIP, PORTS);
      Udp.write("a*");
      Udp.endPacket();
    }
  }



  
  if(isBinded!=false && millis() - LastTimeReceived > 4*keepAliveTimer)
  {
    Serial.println(millis() - LastTimeReceived);
    isBinded = false;
    Serial.printf("\n");
    Serial.printf("Bind Dropped, timed out");
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
    Serial.printf("%c \n",incomingPacket[0]);
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
        Serial.printf("Incoming Image\n ");
        break;
      case '8':
        Serial.printf("Incoming Draw Command: %s\n ", incomingPacket);
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
        Serial.printf("Invalid Message Received\n");
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
    Serial.printf("Bind Accepted\n");
    LastTimeReceived = millis();
  }
  else
  {
    Udp.write("3*0");
    Serial.printf("Bind Denied");
  }
  Udp.endPacket();
}
void BindDropReply()
{
  Serial.printf("Accepted Bind Dropped\n");
  Udp.beginPacket(Udp.remoteIP(), PORTS);
  Udp.write("5*");
  Udp.endPacket();
}
void SendDiscoverReply()
{
  Udp.beginPacket(Udp.remoteIP(), PORTS);
  Serial.printf("to port: %d",PORTS);
  Udp.write("1*");
  Udp.endPacket();
}
