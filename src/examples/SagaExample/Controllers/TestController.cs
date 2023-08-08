using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Saga.Grains;

namespace SagaExample.Controllers
{
    public class TestModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public int Value { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IClusterClient _client;

        public TestController(IClusterClient client)
        {
            _client = client;
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody][Required] TestModel model)
        {
            var grain = _client.GetGrain<ITestSaga>(model.Id);
            await grain.Start(new TestSagaState { Value = model.Value });
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var grain = _client.GetGrain<ITestSaga>(id);
            return Ok(await grain.GetState());
        }

        [HttpPost("resume")]
        public async Task<IActionResult> Resume([FromBody][Required] TestModel model)
        {
            var grain = _client.GetGrain<ITestSaga>(model.Id);
            await grain.Resume(new TestSagaState { Value = model.Value });
            return Ok();
        }

        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromBody][Required] TestModel model)
        {
            var grain = _client.GetGrain<ITestSaga>(model.Id);
            await grain.Revert("reverting...");
            return Ok();
        }
    }
}
