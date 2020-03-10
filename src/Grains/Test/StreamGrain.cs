using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains.Test
{
    public interface IAggregateEventReceiver : IGrainWithStringKey
    {
        Task ReceiveMessage(string message);
    }

    // public class StreamReceiver : Grain, IAggregateEventReceiver
    // {
    //     public override async Task OnActivateAsync()
    //     {
    //         // //First create the grain reference
    //         // var friend = this.GrainFactory.GetGrain<IHello>("test");
    //         var key = this.GetGrainIdentity();
    //         // //Subscribe the instance to receive messages.
    //         // await friend.Subscribe(this.GetPrimaryKeyString());
    //         // call base OnActivateAsync
    //         await base.OnActivateAsync();
    //     }

    //     public Task ReceiveMessage(string message)
    //     {
    //         Console.WriteLine(message);
    //         return Task.CompletedTask;
    //     }
    // }

    public interface IStream: IGrainWithStringKey
    {
        Task Send(string data);
        Task Subscribe(string grainKey);
        Task UnSubscribe(string grainKey);
        Task Ping();
    }

    public class Stream : Grain, IGrainWithStringKey, IStream
    {
        private List<string> _messages = new List<string>();
        private HashSet<string> _keys = new HashSet<string>(new string[] { "test1", "test2" });
        public override async Task OnActivateAsync()
        {
            this.RegisterTimer(SendMessage, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            await base.OnActivateAsync();
        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }

        public Task Send(string data)
        {
            _messages.Add(data);
            return Task.CompletedTask;
        }

        public async Task SendMessage(object args)
        {
            var value = $"{this.GetPrimaryKeyString()}--{DateTime.UtcNow}";
            foreach (var key in _keys)
            {
                var receiver = this.GrainFactory.GetGrain<IAggregateEventReceiver>(key);
                await receiver.ReceiveMessage($"{key}--{value}");
            }
        }

        // Clients call this to subscribe.
        public Task Subscribe(string grainKey)
        {
            _keys.Add(grainKey);
            return Task.CompletedTask;
        }

        //Also clients use this to unsubscribe themselves to no longer receive the messages.
        public Task UnSubscribe(string grainKey)
        {
            _keys.Remove(grainKey);
            return Task.CompletedTask;
        }
    }
}