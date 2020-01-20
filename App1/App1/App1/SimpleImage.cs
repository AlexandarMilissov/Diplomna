using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    public class SimpleImage
    {
        public int width;
        public int height;
        public BitArray imageInBits;
        public SimpleImage(int Width, int Height, BitArray ImageInBits)
        {
            width = Width;
            height = Height;
            imageInBits = ImageInBits;
        }
        public string ToDigitString()
        {
            var builder = new StringBuilder();
            foreach (var bit in imageInBits.Cast<bool>())
                builder.Append(bit ? "1" : "0");
            return builder.ToString();
        }
    }
}
