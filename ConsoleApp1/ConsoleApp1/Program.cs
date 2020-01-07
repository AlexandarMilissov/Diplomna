using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

namespace ConsoleApp1
{
    class Program
    {
        const int PORT = 4210;
        const int keepAliveTimer = 2000;
        UdpSocketClient client = new UdpSocketClient();
        public struct UDPState
        {
            public UdpClient u;
            public IPEndPoint e;
        }

        static HashSet<IPAddress> knownToasters = new HashSet<IPAddress> ();
        static IPEndPoint Bind = new IPEndPoint(IPAddress.Loopback,PORT);
        static Timer timer;
        static Stopwatch keepAliveWatch = new Stopwatch();
        static UDPState s;

        static void Main(string[] args)
        {
            s.e = new IPEndPoint(IPAddress.Any, PORT);
            s.u = new UdpClient(s.e);
            ReceiveMessages();
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = ((UDPState)(ar.AsyncState)).u;
            IPEndPoint e = ((UDPState)(ar.AsyncState)).e;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);


            List<string> info = receiveString.Split("*").ToList();
            switch(info[0])
            {
                case "0":
                    Console.WriteLine($"received discover message from {e.ToString()}");
                    byte[] send_buffer = Encoding.ASCII.GetBytes("1*");
                    s.u.Send(send_buffer, send_buffer.Length, IPAddress.Broadcast.ToString(), PORT);
                    break;
                case "1":
                    ReceivedDiscoverReply(e, info.Skip(1).ToList());
                    break;
                case "3":
                    ReceivedBindReply(e, info.Skip(1).ToList(),u);
                    break;
                case "5":
                    ReceivedBindDroppedAcknowledge();
                    break;
                case "7":
                    break;
                case "9":
                    break;
                case "a":
                    KeepAliveReceived();
                    break;
                default:
                    //invalid message
                    ReceivedInvalidUDP(info);
                    break;
            }
            u.BeginReceive(new AsyncCallback(ReceiveCallback), ar.AsyncState);
        }

        public static void ReceiveMessages()
        {
            Console.WriteLine("listening for messages");
            s.u.BeginReceive(new AsyncCallback(ReceiveCallback), s);

            while (true)
            {
                InTheMeanTime();
            }
        }
        static void InTheMeanTime()
        {
            string read = Console.ReadLine();
            if (read == "d")
            {
                
                SendDiscoverMessage();
            }
            else if(read == "b")
            {
                SendBindRequest();
            }
            else if(read == "r")
            {
                SendBindDropped();
            }
            Thread.Sleep(50);
        }
        static void SendDiscoverMessage()
        {
            byte[] send_buffer = Encoding.ASCII.GetBytes("0*");
            s.u.Send(send_buffer, send_buffer.Length, new IPEndPoint(IPAddress.Broadcast, PORT));
        }

        static void SendBindRequest()
        {
            string bindIp = Console.ReadLine();
            byte[] send_buffer = Encoding.ASCII.GetBytes("2*");
            s.u.Send(send_buffer, send_buffer.Length, new IPEndPoint(IPAddress.Parse(bindIp), PORT));
        }
        static void SendBindDropped()
        {
            if(Bind.Address != IPAddress.Loopback)
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes("4*");
                s.u.Send(send_buffer, send_buffer.Length, Bind);
            }
            else
            {
                Console.WriteLine("No current bind");
            }
        }
        static void ReceivedBindDroppedAcknowledge()
        {
            BindDropped();
            Console.WriteLine("Bind Dropped");
        }
        static void BindDropped()
        {
            Bind.Address = IPAddress.Loopback;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
        }
        static void ReceivedBindReply(IPEndPoint tosterIP, List<string> UDPcontent,UdpClient u)
        {
            if(UDPcontent[0]=="1")
            {
                Bind = tosterIP;
                Console.WriteLine("Binded to " + Bind.Address);
                timer = new Timer(SendKeepAlive,null,0,keepAliveTimer);
                keepAliveWatch.Start();
            }
            else
            {
                Console.WriteLine("Access denied");
            }
        }
        static void ReceivedDiscoverReply(IPEndPoint tosterIP,List<string> UDPcontent)
        {
            knownToasters.Add(tosterIP.Address);
            Console.WriteLine(string.Join(" ", UDPcontent));
            foreach (var v in knownToasters)
            {
                Console.WriteLine(v.ToString()) ;
            }
        }
        static void SendKeepAlive(Object stateInfo)
        {
            if (keepAliveWatch.ElapsedMilliseconds > 4 * keepAliveTimer)
            {
                Console.WriteLine("Bind dropped, timed out");
                BindDropped();
                return;
            }
            Console.WriteLine("keepalive sent");
            byte[] send_buffer = Encoding.ASCII.GetBytes("a*");
            UdpClient u = new UdpClient();
            u.Send(send_buffer, send_buffer.Length, Bind);
        }
        static void KeepAliveReceived()
        {
            keepAliveWatch.Restart();
            Console.WriteLine("keepalive received");
        }
        static void ReceivedInvalidUDP(List<string> UDPcontent)
        {
            Console.WriteLine(string.Join("invalid messsage : ", UDPcontent));
        }
    }
}