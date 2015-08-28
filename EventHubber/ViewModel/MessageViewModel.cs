using EventHubber.Model;
using EventHubber.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.ViewModel
{
    public class MessageViewModel : ViewModelBase
    {
        public EventHubViewModel EventHub { get; set; }

        public MessageViewModel(EventHubMessage msg) 
        {
            this.PartitionId = msg.PartitionId;
            this.Message = msg.MessageBody;
            this.Publisher = msg.Publisher;
            this.EnqueueTimeStamp = msg.EnqueueTimeStamp;
            this.Offset = msg.Offset;
            this.SequenceNumber = msg.SequenceNumber;
        }

       

        public string Message { get; private set; }
        public string PartitionId { get; private set; }
        public string Publisher { get; private set; }
        public DateTime EnqueueTimeStamp { get; private set; }
        public string Body { get; private set; }
        public long SequenceNumber { get; private set; }
        public string Offset { get; private set; }
    }
}
