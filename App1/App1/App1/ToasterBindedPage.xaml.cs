using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ToasterBindedPage : ContentPage
    {
        Communication communication;
        public ToasterBindedPage(Communication c)
        {
            communication = c;
            InitializeComponent();
        }

        private async void BackButtonClicked(object sender, EventArgs e)
        {
            communication.SendBindDropped();
            await Navigation.PopAsync();
        }
    }
}