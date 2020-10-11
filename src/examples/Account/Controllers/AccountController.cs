using System.Threading.Tasks;
using Account.Grains;
using Account.Models;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using ErrorCode = Account.Grains.ErrorCode;

namespace Account.Controllers
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
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.GetBalance();
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }

        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> Deposit(int id, [FromBody] AccountDepositModel body)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.Deposit(body.Amount);
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }

        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> Withdraw(int id, [FromBody] AccountWithdrawModel body)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.Withdraw(body.Amount);
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }

        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> Transfer(int id, [FromBody] AccountTransferModel body)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var response = await grain.Transfer(body.ToAccountId, body.Amount);
            if (response.ErrorCode != ErrorCode.None)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response.Value);
        }
    }
}
