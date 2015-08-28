using EventHubber.Model;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace EventHubber.Services
{
    public interface IEventHubService
    {
        bool IsReading { get; }
        bool IsOpen{ get; }
        long MessageCount { get; }

        IObservable<PartitionRuntimeInformation> PartitionFound { get; }
        IObservable<EventHubMessage> MessageReceived{ get; }
        Task OpenEventHubAsync(string eventHubConnectionString, string hubName);
        Task ReadAllAsync(DateTime startTime);
        Task ReadAllAsync(TimeSpan offset);
        Task ReadAllAsync(long offset);
        Task ReadAsync(string partitionId, TimeSpan offset);
        Task ReadAsync(string partitionId, DateTime startTime);
        Task ReadAsync(string partitionId, long offset);
        void Stop();
    }
}