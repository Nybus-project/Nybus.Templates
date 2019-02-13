using System.Threading.Tasks;
using Nybus;

namespace NetCoreRabbitMqWindowsService.Handlers
{
    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<TestEvent> incomingEvent)
        {
            return Task.CompletedTask;
        }
    }
}