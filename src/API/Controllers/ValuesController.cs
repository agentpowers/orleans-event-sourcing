using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace API.Controllers
{
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private IClusterClient client;
        
        public ValuesController(IClusterClient client)
        {
            this.client = client;
        }

        // GET api/values
        [HttpGet]
        public async Task<IDictionary<string, string>> Get()
        {
            var collectionGrain = this.client.GetGrain<IValueCollectionGrain>("collection");
            return await collectionGrain.GetValues();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<string> Get(int id)
        {
            var grain = this.client.GetGrain<IValueGrain>(id);
            return await grain.GetValue();
        }

        // POST api/values/5
        [HttpPost("set")]
        public async Task Post([FromQuery]int id, [FromQuery]string value)
        {
            var grain = this.client.GetGrain<IValueGrain>(id);
            await grain.SetValue(value);
        }
    }
}
