using EventHubber.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.Models
{
    public class EventHubViewModel : INotifyPropertyChanged
    {
        EventHubService _service;

        public event PropertyChangedEventHandler PropertyChanged;

        public string EventHubConnectionString { get; set; }

        public int MyProperty { get; set; }

        public EventHubViewModel()
        {
            
        }
        
        void RaisePropertyChanged(string propName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
