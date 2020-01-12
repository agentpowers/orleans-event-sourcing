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
    public class AccountController : ControllerBase
    {
        private IClusterClient client;
        
        public AccountController(IClusterClient client)
        {
            this.client = client;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<decimal> Get(int id)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            return await grain.GetBalance();
        }

        [HttpPost("{id}/deposit")]
        public async Task<decimal> Deposit(int id, decimal amount)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            return await grain.Deposit(amount);
        }

        [HttpPost("{id}/withdraw")]
        public async Task<decimal> Withdraw(int id, decimal amount)
        {
            var grain = this.client.GetGrain<IAccountGrain>(id);
            var result =  await grain.Withdraw(amount);
            return result.Value;
        }
    }
}
