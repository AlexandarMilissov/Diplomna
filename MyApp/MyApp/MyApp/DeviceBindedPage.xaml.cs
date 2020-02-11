using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceBindedPage : ContentPage
    {
        Communication communication;
        SimpleImage imageSelected;
        public DeviceBindedPage(Communication c)
        {
            communication = c;
            communication.currentPage = this;
            InitializeComponent();
        }
        private async void BackButtonClicked(object sender, EventArgs e)
        {
            communication.BindDropped();
            await Navigation.PopModalAsync();
        }
        private async void SelectFileButtonClicked(object sender, EventArgs e)
        {
            string s = File.ReadAllText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "temp.txt");
            try
            {
                imageSelected = new SimpleImage();
                imageSelected.FromJSON(s);
            }
            catch{ }
        }

        private void SendFileButtonClicked(object sender, EventArgs e)
        {
            if (imageSelected == null)
            {
                return;
            }
            communication.SendImage(imageSelected);
        }
        private async void TransformFileButtonClicked(object sender, EventArgs e)
        {
            FileData file = await CrossFilePicker.Current.PickFile();
            SimpleImage si;
            if (file == null)
            {
                return;
            }
            try
            {
                si = new SimpleImage(file);
                if(imageSelected == null)
                {
                    imageSelected = si;
                }
            }
            catch 
            {
                return;
            }
            string s = si.ToJSON();
            File.WriteAllText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "temp.txt", s);
        }


        private void CommandButtonClicked1(object sender, EventArgs e)
        {
            char command = '1';
            communication.SendCommand(command);
        }
        private void CommandButtonClicked2(object sender, EventArgs e)
        {
            char command = '2';
            communication.SendCommand(command);
        }
        private void CommandButtonClicked3(object sender, EventArgs e)
        {
            char command = '3';
            communication.SendCommand(command);
        }
        private void CommandButtonClicked4(object sender, EventArgs e)
        {
            char command = '4';
            communication.SendCommand(command);
        }
        private void CommandButtonClicked5(object sender, EventArgs e)
        {
            char command = '5';
            communication.SendCommand(command);
        }
        private void CommandButtonClicked6(object sender, EventArgs e)
        {
            char command = '6';
            communication.SendCommand(command);
        }
        private void CommandButtonClicked7(object sender, EventArgs e)
        {
            char command = '7';
            communication.SendCommand(command);
        }
    }
}