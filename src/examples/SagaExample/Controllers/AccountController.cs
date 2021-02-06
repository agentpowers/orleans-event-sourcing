using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace SagaExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IClusterClient client;

        public AccountController(IClusterClient client)
        {
            this.client = client;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // var grain = this.client.GetGrain<ISagaGrain>(id);
            // var response = await grain.GetBalance();
            // if (response.ErrorCode != ErrorCode.None)
            // {
            //     return BadRequest(response.ErrorMessage);
            // }
            // return Ok(response.Value);
            await Task.Delay(1);
            return Ok();
        }
    }
}
