
using GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains
{
    public class ValueCollectionGrain : Grain, IValueCollectionGrain
    {
        private readonly IDictionary<string, string> map = new Dictionary<string, string>();
        public Task Add(string key, string value)
        {
            if (map.ContainsKey(key))
            {
                map[key] = value;
            }
            else 
            {
                map.Add(key, value);
            }
            return Task.CompletedTask;
        }

        public Task<IDictionary<string, string>> GetValues()
        {
            return Task.FromResult(map);
        }
    }
}