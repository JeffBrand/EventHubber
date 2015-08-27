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

        public MessageViewModel(EventHubMessage msg) : this(msg.PartitionId, msg.MessageBody)
        {

        }

        public MessageViewModel(string partitionId, string message)
        {
            this.PartitionId = partitionId;
            this.Message = message;

            
        }

        public string Message { get; private set; }
        public string PartitionId { get; private set; }
    }
}
