using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    public class SimpleImage
    {
        public Int16 width;
        public Int16 height;
        public BitArray imageInBits;
        public SimpleImage(Int16 Width, Int16 Height, BitArray ImageInBits)
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
