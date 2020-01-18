using Plugin.FilePicker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            string[] fileTypes =
            {
                "*.bmp"
            };
            var file = await CrossFilePicker.Current.PickFile(fileTypes);
            if(!file.FileName.Split('.').Last().Equals("bmp"))
            {
                file = null;
                return;
            }
            /*
            MemoryStream ms = new MemoryStream();
            Image i = Image.
            i.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            byte[] bytes = ms.ToArray();
            BitArray bitArray = new BitArray(bytes);
            Debug.WriteLine(bitArray);
            */
            byte[] b;
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                b = memoryStream.ToArray();
                memoryStream.Close();
                memoryStream.Dispose();
            }
            byte[] o = b.ToList().Skip(10).Take(4).ToArray();
            int offset = BitConverter.ToInt32(o, 0); ;
            byte[] w = b.ToList().Skip(18).Take(4).ToArray();
            int width = BitConverter.ToInt32(w,0);
            byte[] h = b.ToList().Skip(22).Take(4).ToArray();
            int height = BitConverter.ToInt32(h, 0);
            List<byte> final = b.ToList().Skip(54).ToList();
            
            BitArray bitArray = new BitArray(final.ToArray());

            if (file != null)
            {
                lbl.Text = ToDigitString(bitArray);
            }
        }
        public string ToDigitString(BitArray array)
        {
            var builder = new StringBuilder();
            foreach (var bit in array.Cast<bool>())
                builder.Append(bit ? "1" : "0");
            return builder.ToString();
        }
    }
}