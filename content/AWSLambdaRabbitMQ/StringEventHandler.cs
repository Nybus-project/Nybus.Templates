using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Nybus;

namespace AWSLambdaRabbitMQ
{
    public class StringEventHandler : Kralizek.Lambda.IEventHandler<string>
    {
        private readonly ILogger<StringEventHandler> _logger;
        private readonly IBus _bus;

        public StringEventHandler(IBus bus, ILogger<StringEventHandler> logger)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task HandleAsync(string input, ILambdaContext context)
        {
            _logger.LogInformation($"Received: {input}");

            return Task.CompletedTask;
        }
    }
}