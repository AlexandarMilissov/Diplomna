using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace MyApp
{
    public class Communication
    {
        const int PORT = 4210;
        const int keepAliveTimer = 2000;

        UdpSocketClient udpClient = new UdpSocketClient();
        UdpSocketReceiver udpReceiver = new UdpSocketReceiver();
        TcpSocketClient tcpClient;

        Thread tcpReceive;
        bool isTcpThreadActive = false;
        Timer timer;
        Stopwatch keepAliveWatch = new Stopwatch();
        Device bindedDevice;

        MainPage mainPage;
        public ContentPage currentPage;

        public Communication(MainPage m)
        {
            mainPage = m;
            currentPage = m;
            StartUDPReceive();
        }

        public async void StartUDPReceive()
        {
            await udpReceiver.StartListeningAsync(PORT);
            udpReceiver.MessageReceived += (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);

                List<string> info = data.Split('*').ToList();
                if(info[0] == "1")
                {
                    ReceivedDiscoverReply(IPAddress.Parse(args.RemoteAddress), info.Skip(1).ToList());
                }
            };
        }
        private void ReceivedDiscoverReply(IPAddress DeviceIP, List<string> UDPcontent)
        {
            Device d = new Device(UDPcontent[0], DeviceIP);
            mainPage.viewModel.AddDevice(d);
        }
        private void TcpReceive(object data)
        {
            string s = "";
            while (isTcpThreadActive)
            {
                try
                {
                    if (!isTcpThreadActive)
                        return;
                    int nextByte = tcpClient.ReadStream.ReadByte();
                    switch (nextByte)
                    {
                        case -1:
                            break;
                        case (int)'a':
                            KeepAliveReceived();
                            break;
                        default:
                            s += nextByte;
                            break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }
        private void KeepAliveReceived()
        {
            keepAliveWatch.Restart();
        }
        public void BindDropped()
        {
            if (bindedDevice != null)
            {
                bindedDevice.BindActive = false;
            }
            keepAliveWatch.Reset();
            bindedDevice = null;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            isTcpThreadActive = false;
            tcpClient = null;
            tcpReceive = null;
            timer = null;
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                currentPage.Navigation.PopModalAsync();
            });
        }
        async void SendKeepAlive(Object stateInfo)
        {
            if (bindedDevice != null && bindedDevice.BindActive == false)
            {
                bindedDevice = null;
                await tcpClient.DisconnectAsync();
                return;
            }
            else if (keepAliveWatch.ElapsedMilliseconds > 4 * keepAliveTimer)
            {
                if (bindedDevice != null)
                {
                    bindedDevice.BindActive = false;
                }
                await tcpClient.DisconnectAsync();
                isTcpThreadActive = false;
                BindDropped();
                return;
            }
            byte[] msg = Encoding.UTF8.GetBytes("a*\r");
            tcpClient.WriteStream.Write(msg, 0, msg.Length);
            await tcpClient.WriteStream.FlushAsync();
        }
        public async void SendDiscoverMessage()
        {
            byte[] msg = Encoding.UTF8.GetBytes("0*");
            await udpClient.SendToAsync(msg, IPAddress.Broadcast.ToString(), PORT);
        }
        public async void StartTCP(Device d)
        {
            tcpClient = new TcpSocketClient();
            try
            {
                await tcpClient.ConnectAsync(d.IPAddress.ToString(), 4210);
            }
            catch
            {
                return;
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                mainPage.UpdateView();
            });
            keepAliveWatch.Start();
            timer = new Timer(SendKeepAlive, null, 0, keepAliveTimer);

            bindedDevice = d;
            bindedDevice.BindActive = true;

            isTcpThreadActive = true;
            tcpReceive = new Thread(TcpReceive);
            tcpReceive.Start();
        }
        public async void SendCommand(string command)
        {
            byte[] msg = Encoding.UTF8.GetBytes("8*" + command + "\r");
            tcpClient.WriteStream.Write(msg, 0, msg.Length);
            await tcpClient.WriteStream.FlushAsync();
        }
        public async void SendImage(SimpleImage image, string name)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);

            if (name == null || name.Length == 0)
            {
                name ="file.txt";
            }
            string start =  "6*" + 
							image.width.ToString() + 
							"*" + 
							image.height.ToString() + 
							"*" + 
                            name + 
							"\r";

            byte[] msg = Encoding.UTF8.GetBytes(start);
            tcpClient.WriteStream.Write(msg, 0, msg.Length);
            await tcpClient.WriteStream.FlushAsync();
            Thread.Sleep(50);

            /*List<byte> message;
            for (int i = 0; i < numberPackets; i++)
            {
                message = new List<byte>();
                message.Add((byte)'6');
                message.Add((byte)'*');
                message.AddRange(image.image.Skip(i * maxImageSize).Take(maxImageSize));
                keepAliveWatch.Reset();
                byte[] msgPart = message.ToArray();
                tcpClient.WriteStream.Write(msgPart, 0, msgPart.Length);
                await tcpClient.WriteStream.FlushAsync();
                //Thread.Sleep(50);
            }*/
            byte[] message = image.image.ToArray();
            tcpClient.WriteStream.Write(message, 0, message.Length);
            await tcpClient.WriteStream.FlushAsync();
            for(int i = 0; i < image.image.Count / 5000; i++)
            {
                keepAliveWatch.Reset();
                Thread.Sleep(500);
            }

            timer.Change(0, keepAliveTimer);
        }
    }
}
