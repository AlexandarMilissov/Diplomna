
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
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
        SimpleImage imageSelected;
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
        private async void SelectImageButtonClicked(object sender, EventArgs e)
        {
            string[] fileTypes =
            {
                "*.bmp"
            };
            FileData file = await CrossFilePicker.Current.PickFile(fileTypes);
            if (file==null || (file!=null && !file.FileName.Split('.').Last().Equals("bmp")))
            {
                return;
            }
            imageSelected = FileToSimpleImage(file);
            /*
            if (file != null)
            {
                lbl.Text = ToDigitString(bitArray);
            }
            */
        }

        private void SendImageButtonClicked(object sender, EventArgs e)
        {
            if(imageSelected == null)
            {
                return;
            }
            communication.SendImage(imageSelected);
        }
        
        SimpleImage FileToSimpleImage(FileData file)
        {
            byte[] b;
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                b = memoryStream.ToArray();
                memoryStream.Close();
                memoryStream.Dispose();
            }

            Int16 offset = BitConverter.ToInt16(b.ToList().Skip(10).Take(4).ToArray(), 0);
            Int16 width = BitConverter.ToInt16(b.ToList().Skip(18).Take(4).ToArray(), 0);
            Int16 height = BitConverter.ToInt16(b.ToList().Skip(22).Take(4).ToArray(), 0);

            if (width* height > 128 * 8)
            {
                return null;
            }



                List<byte> lb = b.ToList().Skip(offset).ToList();
            int bytesPerRow = (int)Math.Ceiling(width / 8d);
            int bytesToSkipEachRow = bytesPerRow % 4;
            if (bytesToSkipEachRow != 0)
            {
                bytesToSkipEachRow = 4 - bytesToSkipEachRow;
            }

            List<bool> final = new List<bool>();
            BitArray bitArray;

            List<byte> row;
            bool[] boolAray;
            List<bool> boolList;
            for (int i = 0; i < height; i++)
            {
                row = lb.Take(bytesPerRow).ToList();
                lb.RemoveRange(0, bytesPerRow + bytesToSkipEachRow);

                boolAray = new bool[width];
                if (width % 8 != 0)
                {
                    boolAray = new bool[(8 - width % 8) + width];
                }

                bitArray = new BitArray(row.ToArray());
                bitArray.CopyTo(boolAray, 0);

                if (width % 8 != 0)
                {
                    boolList = boolAray.ToList();
                    boolList.RemoveRange(0, 8 - width % 8);
                }
                else
                {
                    boolList = boolAray.ToList();
                }
                final.AddRange(boolList);
            }
            bitArray = new BitArray(final.ToArray());
            bitArray.Not();
            SimpleImage simpleImage = new SimpleImage(width,height,bitArray);
            return simpleImage;
        }
    }
}