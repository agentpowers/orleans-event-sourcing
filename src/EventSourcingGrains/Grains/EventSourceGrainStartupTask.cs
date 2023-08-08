using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Persistance;
using Orleans.Runtime;

namespace EventSourcingGrains.Grains
{
    public class CallGrainStartupTask : IStartupTask
    {
        private readonly IEventSourceGrainSettingsMap _eventSourceGrainSettings;
        private readonly IRepository _repository;
        public CallGrainStartupTask(IEventSourceGrainSettingsMap eventSourceGrainSettings, IRepository repository)
        {
            _eventSourceGrainSettings = eventSourceGrainSettings;
            _repository = repository;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            // init aggregate tables
            foreach (var aggregateName in _eventSourceGrainSettings.Keys)
            {
                await _repository.CreateEventsAndSnapshotsTables(aggregateName);
            }
        }
    }
}
