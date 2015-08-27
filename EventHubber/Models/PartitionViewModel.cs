using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.Models
{
    public class PartitionViewModel : INotifyPropertyChanged
    {
        public string PartitionId { get; set; }
        private long BeginSequenceNumber { get; set; }
        private long LastSequenceNumber { get; set; }

        long _messageCount = 0;
        public long MessageCount {
            get { return _messageCount; }
            set {
                if (_messageCount == value)
                    return;
                _messageCount = value;
                RaisePropertyChanged("MessageCount");
            } }


        public PartitionViewModel(PartitionRuntimeInformation info)
        {
           
            this.PartitionId = info.PartitionId;
            BeginSequenceNumber = info.BeginSequenceNumber;
            LastSequenceNumber = info.LastEnqueuedSequenceNumber;
            MessageCount = LastSequenceNumber - BeginSequenceNumber;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
