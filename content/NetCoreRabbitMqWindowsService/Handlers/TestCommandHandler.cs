using System.Threading.Tasks;
using Nybus;

namespace NetCoreRabbitMqWindowsService.Handlers
{
    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<TestCommand> incomingCommand)
        {
            return Task.CompletedTask;
        }
    }
}