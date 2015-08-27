using EventHubber.Model;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace EventHubber.Services
{
    public interface IEventHubService
    {
        IObservable<PartitionRuntimeInformation> PartitionFound { get; }
        IObservable<EventHubMessage> MessageReceived{ get; }
        Task OpenEventHubAsync(string eventHubConnectionString, string hubName);
        Task ReadAllAsync();
        void StopReading();
    }
}