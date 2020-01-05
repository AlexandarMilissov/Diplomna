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

        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void Discover_Button_Clicked(object sender, EventArgs e)
        {
            var msg = Encoding.UTF8.GetBytes("0");
            await client.SendToAsync(msg, IPAddress.Broadcast.ToString(), PORT);
        }
    }
}
