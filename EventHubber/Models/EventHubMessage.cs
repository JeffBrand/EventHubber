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
    }
}
