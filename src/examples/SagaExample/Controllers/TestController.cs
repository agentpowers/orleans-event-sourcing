using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Saga.Grains;

namespace SagaExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IClusterClient client;

        public TestController(IClusterClient client)
        {
            this.client = client;
        }

        [HttpGet("create/{value}")]
        public async Task<IActionResult> Create(int value)
        {
            var id = Guid.NewGuid().ToString();
            var grain = this.client.GetGrain<ITestSaga>(id);
            await grain.Start(value);
            return Ok(id);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var grain = this.client.GetGrain<ITestSaga>(id);
            var (sagaStatus, context) = await grain.GetStatus();
            return Ok(new { SagaStatus = sagaStatus.ToString(), context });
        }

        [HttpGet("resume/{id}/{value}")]
        public async Task<IActionResult> Resume(string id, int value)
        {
            var grain = this.client.GetGrain<ITestSaga>(id);
            await grain.Resume(value);
            return Ok();
        }

        [HttpGet("revert/{id}")]
        public async Task<IActionResult> Revert(string id)
        {
            var grain = this.client.GetGrain<ITestSaga>(id);
            await grain.Revert("reverting...");
            return Ok();
        }
    }
}
