using System;
using System.Threading.Tasks;
using Caching.Grains;
using Caching.Models;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Concurrency;

namespace Caching.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IClusterClient client;

        public CacheController(IClusterClient client)
        {
            this.client = client;
        }

        // GET
        [HttpGet("{key}")]
        public async Task<string> Get([FromRoute] string key)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            return (await grain.Get()).Value;
        }

        // POST
        [HttpPost("{key}")]
        public async Task Set([FromRoute] string key, [FromBody] CacheModel body)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            var immutableValue = new Immutable<string>(body.Value);
            await grain.Set(immutableValue, TimeSpan.MinValue);
        }

        [HttpDelete("{key}")]
        public async Task Delete([FromRoute] string key)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            await grain.Clear();
        }

        [HttpHead("{key}")]
        public async Task Refresh([FromRoute] string key)
        {
            var grain = this.client.GetGrain<ICacheGrain<string>>(key);
            await grain.Refresh();
        }

    }
}
