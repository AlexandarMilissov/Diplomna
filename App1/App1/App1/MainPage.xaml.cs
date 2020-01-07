﻿using Sockets.Plugin;
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
        public MainPage()
        {

            this.InitializeComponent();
            StartUDPReceive();
        }

        async void StartUDPReceive()
        {
            await udpReceiver.StartListeningAsync(PORT);
            udpReceiver.MessageReceived += (sender, args) =>
            {
                string from = String.Format("{0}:{1}", args.RemoteAddress, args.RemotePort);
                string data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);




               

                List<string> info = data.Split('*').ToList();
                switch (info[0])
                {
                    case "0":
                        break;
                    default:
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            label.Text = data;
                        });

                        break;
                };
            };
            await udpReceiver.StartListeningAsync(PORT);
        }


        private async void Discover_Button_Clicked(object sender, EventArgs e)
        {
            var msg = Encoding.UTF8.GetBytes("0*");
            await client.SendToAsync(msg, IPAddress.Broadcast.ToString(), PORT);
        }
    }
}
