using EventSourcing.Persistance;
using EventSourcing.Stream;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains.Test
{

    public class AccountAggregateReceiver : Grain, IAggregateStreamReceiverWithIntegerKey
    {
        public override async Task OnActivateAsync()
        {
            var key = this.GetGrainIdentity();
            var aggregateStreamGrain = GrainFactory.GetGrain<AggregateStreamGrain>(Account.AccountGrain.AggregateName);
            //Subscribe the instance to receive messages.
            await aggregateStreamGrain.Subscribe(this.GetGrainIdentity());
            // call base OnActivateAsync
            await base.OnActivateAsync();
        }

        public Task Receive(Event @event)
        {
            Console.WriteLine($"{@event}");
            return Task.CompletedTask;
        }
    }
}