using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MyApp
{
    public class Device
    {
        public string DisplayName { get; set; }
        public IPAddress IPAddress { get; set; }
        public bool BindActive { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
        public Device(string Name, IPAddress ipAddress)
        {
            DisplayName = Name;
            IPAddress = ipAddress;
            BindActive = false;
        }
    }
}
