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

        public IObservable<PartitionRuntimeInformation> PartitionFound { get { return _foundPartitions; } }
        public IObservable<EventHubMessage> MessageReceived { get { return _messages; } }

        public EventHubService()
        {
           
            
        }

        public async Task OpenEventHubAsync(string eventHubConnectionString, string hubName)
        {
            if (string.IsNullOrWhiteSpace(eventHubConnectionString))
                throw new ArgumentException("invalid event hub connection string");

            if (string.IsNullOrWhiteSpace(hubName))
                throw new ArgumentException("invalid hubname");

            var builder = new ServiceBusConnectionStringBuilder(eventHubConnectionString) { TransportType = TransportType.Amqp };
            var s = builder.ToString();
            _factory = MessagingFactory.CreateFromConnectionString(builder.ToString());
            //Partitions = new ObservableCollection<PartitionViewModel>();
            //Messages = new ObservableCollection<MessageViewModel>();

            _client = _factory.CreateEventHubClient(hubName);var runtimeInfo = await _client.GetRuntimeInformationAsync();

            _partitions.Clear();
            foreach (var p in runtimeInfo.PartitionIds)
            {
                
                var partition = await _client.GetPartitionRuntimeInformationAsync(p);
                _partitions.Add(new Partition(p, _messages));
                _foundPartitions.OnNext(partition);
            }

        }

        public async Task ReadAllAsync()
        {
           
          _receiving = true;
            if (_cancellation == null)
                _cancellation = new CancellationTokenSource();

            

            foreach (var p in _partitions)
            {
                var rcv = await _client.GetDefaultConsumerGroup().CreateReceiverAsync(p.PartitionId, DateTime.MinValue);
                _runningTasks.Add(p.ReadAsync(rcv,_cancellation));
            }

            
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
