using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Account.Grains;
using System.Diagnostics;
using System.Threading;

namespace Account.Controllers
{
    [ApiController]
    [Route("load_testing")]
    public class LoadTestingController : ControllerBase
    {
        private IClusterClient client;
        
        public LoadTestingController(IClusterClient client)
        {
            this.client = client;
        }

        [HttpGet("deposit")]
        public async Task<IActionResult> Deposit(int count = 10000)
        {
            var started = DateTime.UtcNow;
            var sw = Stopwatch.StartNew();
            await Enumerable.Range(1, count).ForEachAsyncSemaphore(Environment.ProcessorCount, body: async entry =>
            {
                var grain = this.client.GetGrain<IAccountGrain>(entry);
                var response = await grain.Deposit(1000);
            });
            sw.Stop();
            return Ok($"{count} Took ticks={sw.ElapsedTicks}, ms={sw.ElapsedMilliseconds}, started={started}");
        }

        [HttpGet("withdraw")]
        public async Task<IActionResult> Withdraw(int count = 10000)
        {
            var sw = Stopwatch.StartNew();
            await Enumerable.Range(1, count).ForEachAsyncSemaphore(Environment.ProcessorCount, body: async entry =>
            {
                var grain = this.client.GetGrain<IAccountGrain>(entry);
                var response = await grain.Withdraw(1000);
            });
            sw.Stop();
            return Ok($"{count} Took ticks={sw.ElapsedTicks}, ms={sw.ElapsedMilliseconds}");
        }
    }

    public static class Ext
    {
        public static async Task ForEachAsyncSemaphore<T>(
            this IEnumerable<T> source,
            int degreeOfParallelism,
            Func<T, Task> body)
        {
            var tasks = new List<Task>();
            using (var throttler = new SemaphoreSlim(degreeOfParallelism))
            {
                foreach (var element in source)
                {
                    await throttler.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await body(element);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}
