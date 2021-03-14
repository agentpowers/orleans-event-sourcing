using System;
using System.Threading.Tasks;
using EventSourcing.Persistance;
using Xunit;

namespace EventSourcing.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var postgresRepo = new PostgresRepository("host=localhost;database=EventSourcing;username=orleans;password=5544338;Enlist=false;Maximum Pool Size=90;");
            var events = new AggregateEventBase[]
            { 
                new AggregateEventBase{ AggregateId = 3, AggregateVersion = 6744, EventVersion = 0, ParentEventId = null, RootEventId = null, Type = "TransferDebited", Data = "",  Created = DateTime.UtcNow },
                new AggregateEventBase{ AggregateId = 3, AggregateVersion = 6745, EventVersion = 0, ParentEventId = null, RootEventId = null, Type = "TransferDebited", Data = "",  Created = DateTime.UtcNow },
            };
            //await postgresRepo.SaveEvent("account", events[0]);
            await postgresRepo.SaveEvents("account", events);
        }
    }
}
