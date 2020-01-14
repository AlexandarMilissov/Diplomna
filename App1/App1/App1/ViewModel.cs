using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace App1
{
    public class ViewModel : INotifyPropertyChanged
    {
        string name = string.Empty;
        public string Name
        {
            get => name;
            set
            {
                if(name == value)
                {
                    return;
                }
                else
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(dfuck));
                }
            }
        }
        public string dfuck => $"ok? {Name}";
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
        }
    }
}
