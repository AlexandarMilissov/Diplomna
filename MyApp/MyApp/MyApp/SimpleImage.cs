using Nancy.Json;
using Newtonsoft.Json;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyApp
{
    public class SimpleImage
    {
        public Int16 width;
        public Int16 height;
        public List<byte> image; 
        
        public SimpleImage()
        {

        }
        public SimpleImage(FileData file)
        {
            FromFile(file);
        }


        public void FromFile(FileData file)
        {
            string extention = file.FileName.Split('.').Last();
            switch(extention)
            {
                case "bmp":
                    FromBMP(file);
                    break;
                default:
                    throw new Exception("not a valid type");
            }
        }

        private void FromBMP(FileData file)
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

            //
            //  size check?
            //


            List<byte> lb = b.ToList().Skip(offset).ToList();
            int bytesPerRow = (int)Math.Ceiling(width / 8d);
            int bytesToSkipEachRow = bytesPerRow % 4;
            if (bytesToSkipEachRow != 0)
            {
                bytesToSkipEachRow = 4 - bytesToSkipEachRow;
            }

            List<bool> final = new List<bool>();

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

                BitArray bitArray = new BitArray(row.ToArray());
                bitArray.CopyTo(boolAray, 0);

                boolList = boolAray.ToList();
                for (int j = 0; j < row.Count; j++)
                {
                    boolList.AddRange(boolList.Take(8).Reverse().ToList());
                    boolList.RemoveRange(0, 8);
                }
                boolList.Reverse();

                if (width % 8 != 0)
                {
                    boolList.RemoveRange(0, 8 - width % 8);
                }
                final.Reverse();
                final.AddRange(boolList);
                final.Reverse();
            }

            this.width = width;
            this.height = height;
            this.image = EncodeLargeBool(final.ToArray());
        }

        List<byte> EncodeLargeBool(bool[] arr)
        {
            List<byte> result = new List<byte>();

            int size = arr.Length;
            int addition = 8 - size % 8;
            if(addition == 8)
            {
                addition = 0;
            }
            bool[] temp = new bool[8];
            for (int i = 0; i < size;)
            {
                for (int j = 0; j < 8; j++)
                {
                    if(i >= size)
                    {
                        for(int k = 0; k < addition; k++)
                        {
                            temp[j + k] = false;
                        }
                        break;
                    }
                    else
                    {
                        temp[j] = arr[i];
                    }
                    i++;
                }
                result.Add(EncodeBool(temp));
            }

            return result;
        }
        byte EncodeBool(bool[] arr)
        {
            byte val = 0;
            int i = 0;
            foreach (bool b in arr)
            {
                i++;
                val <<= 1;
                if (b) val |= 1;
            }
            while(i<8)
            {
                val <<= 1;
                i++;
            }
            return val;
        }
    }
}