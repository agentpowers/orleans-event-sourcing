using System.Threading.Tasks;
using Grains.Account;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using ErrorCode = Grains.Account.ErrorCode;

namespace API.Controllers
{
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private IClusterClient client;
        
        public AccountController(IClusterClient client)
        {
            this.client = client;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.GetBalance();
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }

        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.Deposit(amount);
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }

        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.Withdraw(amount);
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }

        [HttpPost("{id}/transfer/{toId}")]
        public async Task<IActionResult> Transfer(int id, int toId, decimal amount)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.Transfer(toId, amount);
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }
    }
}
