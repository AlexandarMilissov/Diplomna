using Plugin.FilePicker;
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
            communication.currentPage = this;
            InitializeComponent();
        }

        private async void BackButtonClicked(object sender, EventArgs e)
        {
            communication.SendBindDropped();
            await Navigation.PopModalAsync();
        }
        private async void SelextImageButtonClicked(object sender, EventArgs e)
        {
            string[] fileTypes = null;
            var file = await CrossFilePicker.Current.PickFile(fileTypes);

            if (file != null)
            {
                lbl.Text = file.FileName;
            }
        }
    }
}