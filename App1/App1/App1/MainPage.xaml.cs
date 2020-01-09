using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xamarin.Forms;

namespace App1
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        const int PORT = 4210;
        UdpSocketClient client = new UdpSocketClient();
        UdpSocketReceiver udpReceiver = new UdpSocketReceiver();
        static HashSet<Toaster> knownToasters = new HashSet<Toaster>();
        public MainPage()
        {

            Toaster t = new Toaster { DisplayName = "test" };
            knownToasters.Add(t);

            this.InitializeComponent();
            StartUDPReceive();
            List.ItemsSource = knownToasters;
        }

        async void StartUDPReceive()
        {
            udpReceiver.MessageReceived += (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);

                List<string> info = data.Split('*').ToList();

                Device.BeginInvokeOnMainThread(() =>
                {
                    label.Text = data;
                });
                switch (info[0])
                {
                    case "0":
                        break;
                    case "1":
                        ReceivedDiscoverReply(IPAddress.Parse(args.RemoteAddress), info.Skip(1).ToList());
                        break;
                    case "3":
                        ReceivedBindReply();
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
                        ReceivedInvalidUDP(info);
                        break;
                };
            };
            await udpReceiver.StartListeningAsync(PORT);
        }


        static void KeepAliveReceived()
        {
            throw new NotImplementedException();
        }

        static void ReceivedBindDroppedAcknowledge()
        {
            throw new NotImplementedException();
        }

        static void ReceivedBindReply()
        {
            throw new NotImplementedException();
        }

        static void ReceivedDiscoverReply(IPAddress toasterIP, List<string> UDPcontent)
        {
            Toaster t = new Toaster { IPAddress = toasterIP, DisplayName = UDPcontent[0] };

            knownToasters.Add(t);
            Console.WriteLine(string.Join(" ", UDPcontent));
        }
        static void ReceivedInvalidUDP(List<string> info)
        {
            throw new NotImplementedException();
        }
        private async void Discover_Button_Clicked(object sender, EventArgs e)
        {
            Toaster t = new Toaster { DisplayName = "test" };
            knownToasters.Add(t);



            var msg = Encoding.UTF8.GetBytes("0*");
            await client.SendToAsync(msg, IPAddress.Broadcast.ToString(), PORT);
        }
    }
}
