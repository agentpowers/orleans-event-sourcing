using EventSourcing.Persistance;
using EventSourcing.Stream;
using Orleans;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grains.Test
{

    public class AccountAggregateReceiver : Grain, IAggregateStreamReceiver//, IGrainWithStringKey
    {
        public override async Task OnActivateAsync()
        {
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        public Task Receive(AggregateEvent @event)
        {
            Console.WriteLine($"AggregateStreamReceiver key={this.GetPrimaryKeyString()}, event={JsonSerializer.Serialize(@event)}");
            return Task.CompletedTask;
        }
    }
}