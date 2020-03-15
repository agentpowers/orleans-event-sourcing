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
    public class TestController : ControllerBase
    {
        private IClusterClient client;
        
        public TestController(IClusterClient client)
        {
            this.client = client;
        }

        // GET api/values/5
        [HttpGet()]
        public Task Get()
        {
            // var grain = this.client.GetGrain<Grains.Test.ITestStream>("test");
            // grain.Ping();
            return Task.CompletedTask;
        }
    }
}
