using GalaSoft.MvvmLight;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.ViewModel
{
    public class PartitionViewModel : ViewModelBase
    {
        public string PartitionId { get; set; }
        private long BeginSequenceNumber { get; set; }
        private long LastSequenceNumber { get; set; }

        public string Offset { get; set; }

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
            Offset = info.LastEnqueuedOffset;

            MessageCount = LastSequenceNumber - BeginSequenceNumber;
            if (MessageCount != 0)
            {
                MessageCount++;
            }
        }

       

        
    }
}
