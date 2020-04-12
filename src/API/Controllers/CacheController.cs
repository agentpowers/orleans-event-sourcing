using System;
using System.Threading.Tasks;
using Grains.Cache;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Concurrency;

namespace API.Controllers
{
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {
        private IClusterClient client;
        
        public CacheController(IClusterClient client)
        {
            this.client = client;
        }

        // GET
        [HttpGet("{key}")]
        public async Task<string> Get([FromRoute]string key)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            return (await grain.Get()).Value;
        }

        // POST
        [HttpPost("{key}")]
        public async Task Set([FromRoute]string key, [FromForm]string value)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            var immutableValue = new Immutable<string>(value);
            await grain.Set(immutableValue, TimeSpan.MinValue);
        }

        [HttpDelete("{key}")]
        public async Task Delete([FromRoute]string key)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            await grain.Clear();
        }

        [HttpHead("{key}")]
        public async Task Refresh([FromRoute]string key)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            await grain.Refresh();
        }

    }
}
