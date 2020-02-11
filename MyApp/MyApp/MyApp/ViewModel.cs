using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MyApp
{
    public class ViewModel
    {
        public ObservableCollection<Device> KnownDevices { get; set; }
        public ViewModel()
        {
            KnownDevices = new ObservableCollection<Device>();
        }
        public void AddDevice(Device d)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var device in KnownDevices)
                {
                    if (d.IPAddress.Equals(device.IPAddress))
                    {
                        return;
                    }
                }
                KnownDevices.Add(d);
            });
        }
    }
}
