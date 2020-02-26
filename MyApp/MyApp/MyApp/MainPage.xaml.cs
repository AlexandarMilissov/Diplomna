 using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MyApp
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public ViewModel viewModel = new ViewModel();
        Communication communication;
        public MainPage()
        {
            BindingContext = viewModel;
            this.InitializeComponent();
            communication = new Communication(this);
        }
        public async void UpdateView()
        {
            DeviceBindedPage deviceBindedPage = new DeviceBindedPage(communication);
            await Navigation.PushModalAsync(deviceBindedPage, false);
        }
        private void Discover_Button_Clicked(object sender, EventArgs e)
        {
            communication.SendDiscoverMessage();
        }
        private void List_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Device device = e.Item as Device;
            communication.StartTCP(device);
        }
    }
}

