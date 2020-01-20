using Sockets.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace App1
{
    public class Communication
    {
        const int PORTR = 4211;
        const int PORTS = 4210;
        const int keepAliveTimer = 2000;
        UdpSocketClient client = new UdpSocketClient();
        UdpSocketReceiver udpReceiver = new UdpSocketReceiver();
        Timer timer;
        Stopwatch keepAliveWatch = new Stopwatch();
        Toaster bindedToaster;
        MainPage mainPage;

        public ContentPage currentPage;
        public Communication(MainPage m)
        {
            mainPage = m;
            currentPage = m;

            //0 discover 
            //1 discover reply
            //2 bind request
            //3 bind reply
            //4 bind drop
            //5 bind drop reply
            //6 image send
            //7 image send acknowledge
            //8 draw command
            //9 draw command reply
            //a keepalive
        }
        public async void StartUDPReceive()
        {
            await udpReceiver.StartListeningAsync(PORTR);
            udpReceiver.MessageReceived += (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);

                List<string> info = data.Split('*').ToList();

                switch (info[0])
                {
                    case "0":
                        break;
                    case "1":
                        ReceivedDiscoverReply(IPAddress.Parse(args.RemoteAddress), info/*.Skip(1).ToList()*/);
                        break;
                    case "3":
                        ReceivedBindReply(IPAddress.Parse(args.RemoteAddress), info.Skip(1).ToList());
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
        }
        void ReceivedDiscoverReply(IPAddress toasterIP, List<string> UDPcontent)
        {
            Toaster t = new Toaster(UDPcontent[0], toasterIP);

            foreach (var toaster in mainPage.viewModel.KnownToasters)
            {
                if (t.IPAddress.Equals(toaster.IPAddress))
                {
                    return;
                }
            }

            mainPage.viewModel.KnownToasters.Add(t);
        }
        void ReceivedBindReply(IPAddress ipAddress, List<string> info)
        {
            if (info[0] == "1")
            {
                bindedToaster = mainPage.viewModel.KnownToasters.Where(x => x.IPAddress.Equals(ipAddress)).First();
                bindedToaster.BindActive = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    mainPage.UpdateView();
                });
                timer = new Timer(SendKeepAlive, null, 0, keepAliveTimer);
                keepAliveWatch.Start();
            }
            else
            {
            }
        }
        void ReceivedBindDroppedAcknowledge()
        {
            BindDropped(false);
        }
        void KeepAliveReceived()
        {
            keepAliveWatch.Restart();
        }
        void ReceivedInvalidUDP(List<string> info)
        {
        }
        public async void SendDiscoverMessage()
        {
            byte[] msg = Encoding.UTF8.GetBytes("0*");
            await client.SendToAsync(msg, IPAddress.Broadcast.ToString(), PORTS);
        }
        public async void SendBindRequest(Toaster t)
        {
            byte[] msg = Encoding.ASCII.GetBytes("2*");
            await client.SendToAsync(msg, t.IPAddress.ToString(), PORTS);
        }
        public async void SendBindDropped()
        {
            byte[] msg = Encoding.ASCII.GetBytes("4*");
            await client.SendToAsync(msg, bindedToaster.IPAddress.ToString(), PORTS);
            if(bindedToaster != null)
            {
                bindedToaster.BindActive = false;
            }
        }
        public async void SendImage(SimpleImage image)
        {
            string s = "6*" + image.width.ToString() + "*" + image.width.ToString() + "*" + image.ToDigitString();
            byte[] msg = Encoding.ASCII.GetBytes(s);
            await client.SendToAsync(msg, bindedToaster.IPAddress.ToString(), PORTS);
        }
        async void SendKeepAlive(Object stateInfo)
        {
            if (bindedToaster != null && bindedToaster.BindActive == false)
            {
                SendBindDropped();
                return;
            }
            else if (keepAliveWatch.ElapsedMilliseconds > 4 * keepAliveTimer)
            {
                if (bindedToaster != null)
                {
                    bindedToaster.BindActive = false;
                }
                SendBindDropped();
                BindDropped(true);
                return;
            }
            byte[] msg = Encoding.ASCII.GetBytes("a*");
            await client.SendToAsync(msg, bindedToaster.IPAddress.ToString(), PORTS);
        }
        void BindDropped(bool TimedOut)
        {
            bindedToaster.BindActive = false;
            bindedToaster = null;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            if (TimedOut)
            {
                currentPage.Navigation.PopModalAsync();
            }
        }
    }
}
