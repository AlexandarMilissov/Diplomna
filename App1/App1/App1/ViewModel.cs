using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace App1
{
    public class ViewModel //: INotifyPropertyChanged
    {
        public ObservableCollection<Toaster> KnownToasters { get; set; }
        public ViewModel()
        {
            KnownToasters = new ObservableCollection<Toaster>();
        }
    }
}
