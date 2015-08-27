using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.Models
{
    public class MessageViewModel
    {
        public MessageViewModel(string partitionId, string message)
        {
            this.PartitionId = partitionId;
            this.Message = message;
        }

        public string Message { get; private set; }
        public string PartitionId { get; private set; }
    }
}
