using EventHubber.Model;
using EventHubber.ViewModel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubber.Services
{
    public class EventHubService : IEventHubService
    {
        MessagingFactory _factory;
        EventHubClient _client;

        bool _receiving = false;
        List<Task> _runningTasks = new List<Task>();
        List<Partition> _partitions = new List<Partition>();
        CancellationTokenSource _cancellation;

        //public ObservableCollection<PartitionViewModel> Partitions { get; set; }
        //public ObservableCollection<MessageViewModel>Messages { get; set; }

        Subject<PartitionRuntimeInformation> _foundPartitions = new Subject<PartitionRuntimeInformation>();
        Subject<EventHubMessage> _messages = new Subject<EventHubMessage>();


        public bool IsReading { get; private set; }
        public IObservable<PartitionRuntimeInformation> PartitionFound { get { return _foundPartitions; } }
        public IObservable<EventHubMessage> MessageReceived { get { return _messages; } }

        public bool IsOpen { get; private set; }

        public EventHubService()
        {
           
            
        }

        public  Task OpenEventHubAsync(string eventHubConnectionString, string hubName)
        {
            this.IsOpen = true;
            return Task.Factory.StartNew(async () => {
                if (string.IsNullOrWhiteSpace(eventHubConnectionString))
                    throw new ArgumentException("invalid event hub connection string");

                if (string.IsNullOrWhiteSpace(hubName))
                    throw new ArgumentException("invalid hubname");

                var builder = new ServiceBusConnectionStringBuilder(eventHubConnectionString) { TransportType = TransportType.Amqp };
                var s = builder.ToString();
                _factory = MessagingFactory.CreateFromConnectionString(builder.ToString());
       
                _client = _factory.CreateEventHubClient(hubName);
                
                var runtimeInfo = await _client.GetRuntimeInformationAsync();

                _partitions.Clear();
                foreach (var p in runtimeInfo.PartitionIds)
                {
                
                    var partition = await _client.GetPartitionRuntimeInformationAsync(p);
                    _partitions.Add(new Partition(p, _messages));
                    _foundPartitions.OnNext(partition);
                }


            });

        }

        public async Task ReadAllAsync(DateTime startTime)
        {

            PrepareToRead();


            try {
                foreach (var p in _partitions)
                {
                    var rcv = await _client.GetDefaultConsumerGroup().CreateReceiverAsync(p.PartitionId, startTime.ToUniversalTime());
                    _runningTasks.Add(p.ReadAsync(rcv, _cancellation));
                }
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine("tasks cancelled");
            }

            
        }

        public async Task ReadAllAsync(TimeSpan offset)
        {
            ReadAllAsync(DateTime.Now.Subtract(offset));
        }

        public async Task ReadAllAsync(long offset)
        {
            PrepareToRead();

            foreach (var p in _partitions)
            {
                var rcv = await _client.GetDefaultConsumerGroup().CreateReceiverAsync(p.PartitionId, offset.ToString());
                _runningTasks.Add(p.ReadAsync(rcv, _cancellation));
            }
        }

        public async Task ReadAsync(string partitionId, DateTime startTime)
        {
            if (string.IsNullOrWhiteSpace(partitionId))
                throw new ArgumentNullException("partition id cannot be null or empty");

            var partition = (from p in _partitions where p.PartitionId == partitionId select p).FirstOrDefault();
            if (partition == null)
                throw new InvalidOperationException("partition not found");

            PrepareToRead();
            var rcv = await _client.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, startTime.ToUniversalTime());
            _runningTasks.Add(partition.ReadAsync(rcv, _cancellation));

        }

        public async Task ReadAsync(string partitionId, TimeSpan offset)
        {
            ReadAsync(partitionId, DateTime.Now.Subtract(offset));
        }

        public async Task ReadAsync(string partitionId, long offset)
        {
            if (!string.IsNullOrWhiteSpace(partitionId))
                throw new ArgumentNullException("partition id cannot be null or empty");

            var partition = (from p in _partitions where p.PartitionId == partitionId select p).FirstOrDefault();
            if (partition == null)
                throw new InvalidOperationException("partition not found");

            PrepareToRead();
            var rcv = await _client.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, offset.ToString());
            _runningTasks.Add(partition.ReadAsync(rcv, _cancellation));
        }

        private void PrepareToRead()
        {
            _receiving = true;
            if (_cancellation == null)
                _cancellation = new CancellationTokenSource();

            _runningTasks.Clear();
            IsReading = true;
            
        }

        public void Stop()
        {
            if (_cancellation != null)
            {
                _cancellation.Cancel();
                Task.WaitAll(_runningTasks.ToArray());
                _cancellation = null;
            }
            IsReading = false;
            Debug.WriteLine("Finished");
            
            
        }

        
    }
}
