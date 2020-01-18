using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ViewModel viewModel = new ViewModel();
        Communication communication;
        public MainPage()
        {
            BindingContext = viewModel;
            this.InitializeComponent();
            communication = new Communication(this);
            communication.StartUDPReceive();
        }
        public async void UpdateView()
        {
            ToasterBindedPage t = new ToasterBindedPage(communication);
            await Navigation.PushModalAsync(t, false);
        }
        private void Discover_Button_Clicked(object sender, EventArgs e)
        {
            communication.SendDiscoverMessage();
        }
        private void List_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Toaster t = e.Item as Toaster;
            communication.SendBindRequest(t);
        }
    }
}
