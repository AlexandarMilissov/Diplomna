using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Threading;

namespace App1
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        const int PORT = 4210;
        const int keepAliveTimer = 2000;
        public struct UDPState
        {
            public UdpClient u;
            public IPEndPoint e;
        }
        static HashSet<IPAddress> knownToasters = new HashSet<IPAddress>();
        static IPEndPoint Bind = new IPEndPoint(IPAddress.Loopback, PORT);
        static Timer timer;
        static Stopwatch keepAliveWatch = new Stopwatch();
        static UDPState s;


        public MainPage()
        {
            this.InitializeComponent();
            s.e = new IPEndPoint(IPAddress.Any, PORT);
            s.u = new UdpClient(s.e);
            this.ReceiveMessages();
        }
        public void ReceiveMessages()
        {
            s.u.BeginReceive(new AsyncCallback(this.ReceiveCallback), s);

            /*while (true)
            {
                this.InTheMeanTime();
            }*/
        }
        void InTheMeanTime()
        {
            Thread.Sleep(1);
        }
        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = ((UDPState)(ar.AsyncState)).u;
            IPEndPoint e = ((UDPState)(ar.AsyncState)).e;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);


            List<string> info = receiveString.Split('*').ToList();
            label.Text = "WORKSSSS!";
            switch (info[0])
            {
                case "0":
                    break;
                case "1":
                    label.Text = "WORKSSSS!";
                    //ReceivedDiscoverReply(e, info.Skip(1).ToList());
                    break;
                case "3":
                    //ReceivedBindReply(e, info.Skip(1).ToList(), u);
                    break;
                case "5":
                    //ReceivedBindDroppedAcknowledge();
                    break;
                case "7":
                    break;
                case "9":
                    break;
                case "a":
                    //KeepAliveReceived();
                    break;
                default:
                    //invalid message
                    //ReceivedInvalidUDP(info);
                    break;
            }
            u.BeginReceive(new AsyncCallback(ReceiveCallback), ar.AsyncState);
        }
        private void Discover_Button_Clicked(object sender, EventArgs e)
        {
            
        }
    }
}
