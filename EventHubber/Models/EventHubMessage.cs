using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.Model
{
    public class EventHubMessage
    {
        public string PartitionId { get; set; }
        public string MessageBody { get; set; }
        public DateTime EnqueueTimeStamp { get; set; }

        public string Publisher { get; set; }

        public string Offset { get; set; }
        public long SequenceNumber { get; set; }

        public EventHubMessage()
        {
        } 
        

        public EventHubMessage(string partitionId, EventData message)
        {
            PartitionId = partitionId; 
            MessageBody = Encoding.UTF8.GetString(message.GetBytes());
            EnqueueTimeStamp = message.EnqueuedTimeUtc;
            Publisher = message.SystemProperties[EventDataSystemPropertyNames.Publisher] as string;
            Offset = message.Offset;
            SequenceNumber = message.SequenceNumber;

        }


    }
}
