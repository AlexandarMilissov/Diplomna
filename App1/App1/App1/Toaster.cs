﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace App1
{
    public class Toaster
    {
        public string DisplayName { get; set; }
        public IPAddress IPAddress{ get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
