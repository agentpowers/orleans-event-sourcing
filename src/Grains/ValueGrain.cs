
using GrainInterfaces;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Grains
{
    public class ValueGrain : Grain, IValueGrain
    {
        private string value = "none";

        public Task<string> GetValue()
        {
            return Task.FromResult(this.value);
        }

        public async Task SetValue(string value)
        {
            this.value = value;
            var collectionGrain = GrainFactory.GetGrain<IValueCollectionGrain>("collection");
            await collectionGrain.Add(this.GetPrimaryKey().ToString(), this.value);
        }
    }
}