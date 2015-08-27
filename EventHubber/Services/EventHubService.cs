using EventHubber.Models;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubber.Services
{
    public class EventHubService
    {
        MessagingFactory _factory;
        EventHubClient _client;

        bool _receiving = false;
        List<Task> _runningTasks = new List<Task>();
        CancellationTokenSource _cancellation;

        public ObservableCollection<PartitionViewModel> Partitions { get; set; }
        public ObservableCollection<MessageViewModel>Messages { get; set; }

        public EventHubService(string eventHubConnectionString)
        {
            var builder = new ServiceBusConnectionStringBuilder(eventHubConnectionString) { TransportType = TransportType.Amqp };
            var s = builder.ToString();
            _factory = MessagingFactory.CreateFromConnectionString(builder.ToString());
            Partitions = new ObservableCollection<PartitionViewModel>();
            Messages = new ObservableCollection<MessageViewModel>();
            
        }

        public async Task OpenEventHubAsync(string hubName)
        {
            if (string.IsNullOrWhiteSpace(hubName))
                throw new ArgumentException("invalid hubname");

            _client = _factory.CreateEventHubClient(hubName);var runtimeInfo = await _client.GetRuntimeInformationAsync();

            Partitions.Clear();
            foreach (var p in runtimeInfo.PartitionIds)
            {
                
                var partition = await _client.GetPartitionRuntimeInformationAsync(p);
                Partitions.Add(new PartitionViewModel(partition) ); 
            }

        }

        public async Task StartReadingAsync(string partitionId, DateTime startTime)
        {
           
            Messages.Clear();
            if (string.IsNullOrWhiteSpace(partitionId))
                throw new ArgumentException("Invalid partition id");

            var partition = (from p in Partitions where p.PartitionId == partitionId select p).FirstOrDefault();
            if (partition == null)
                throw new ArgumentOutOfRangeException("Partition id does not exist");

            if (startTime == null)
                throw new ArgumentNullException("startTime cannot be null");

            _receiving = true;
            if (_cancellation == null)
                _cancellation = new CancellationTokenSource();

            var task = Task.Factory.StartNew(async () => {
                    Debug.WriteLine(string.Format("partition {0} started", partitionId));
                    var receiver = await _client.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, startTime);

                    string offset;
                    while (!_cancellation.IsCancellationRequested)
                    {
                        var message = await receiver.ReceiveAsync();
                        if (message != null)
                        {
                            offset = message.Offset;
                            var body = Encoding.UTF8.GetString(message.GetBytes());
                        App.Current.Dispatcher.BeginInvoke(new Action<EventHubService>((sender) => {
                            partition.MessageCount++;
                            Messages.Insert(0,new MessageViewModel(partitionId, body));
                            Debug.WriteLine(body);
                        }),
                             this
                        );
                        
                        }
                    }
                Debug.WriteLine(string.Format("partition {0} finished", partitionId));
            }, _cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            _runningTasks.Add(task);
        }

        public void StopReading()
        {
            _cancellation.Cancel();
            Task.WaitAll(_runningTasks.ToArray());
            Debug.WriteLine("Finished");
            _runningTasks.Clear();
            
        }
    }
}
