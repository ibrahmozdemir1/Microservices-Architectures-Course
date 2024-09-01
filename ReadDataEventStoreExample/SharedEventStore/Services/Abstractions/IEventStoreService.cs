using EventStore.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventStore.Services.Abstractions
{
    public interface IEventStoreService
    {
        Task AppendToStreamAsync(string streamName, IEnumerable<EventData> eventData);
        EventData GenerateEventData(object @event);
        Task SubscribeToStreamAsync(string streamName, Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> eventAppeared);
    }
}
