﻿using Plugin.FilePicker;
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
        private void SendFileButtonClicked(object sender, EventArgs e)
        {
            if (imageSelected == null)
            {
                return;
            }
            communication.SendImage(imageSelected, Entry.Text);
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
                imageSelected = si;
            }
            catch(Exception ex) 
            {
                string t = ex.Message;
                return;
            }
        }

        void CommandButtonClicked(object sender, EventArgs e)
        {
            string data = ((Button)sender).BindingContext as string;
            communication.SendCommand(data);
        }
        
        void TimeButtonClicked(object sender, EventArgs e)
        {
            string data = "9*" + time.Text + "*";
            communication.SendCommand(data);
        }
        void PrintButtonClicked(object sender, EventArgs e)
        {
            string fileName = Entry.Text;
            if (fileName == null || fileName.Length == 0)
                fileName = "file.txt";
            fileName = "8*" + fileName;
            communication.SendCommand(fileName);
        }
    }
}