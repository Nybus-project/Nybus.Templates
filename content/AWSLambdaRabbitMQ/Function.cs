using System.Collections.Generic;
using Amazon.Lambda.Core;
using Kralizek.Lambda;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambdaRabbitMQ
{
    public class Function : EventFunction<string>
    {
        protected override void Configure(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", false);
            builder.AddEnvironmentVariables();
        }

        protected override void ConfigureLogging(ILoggingBuilder logging, IExecutionEnvironment executionEnvironment)
        {
            logging.AddConfiguration(Configuration.GetSection("Logging"));
        }

        protected override void ConfigureServices(IServiceCollection services, IExecutionEnvironment executionEnvironment)
        {
            RegisterHandler<StringEventHandler>(services);

            services.AddNybus(nybus => 
            {
                nybus.UseConfiguration(Configuration);

                nybus.UseRabbitMqBusEngine(rabbitMq => rabbitMq.UseConfiguration());
            });
        }
    }
}
