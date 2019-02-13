using System;
using Topshelf;
using Nybus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Nybus.Configuration;
using NetCoreRabbitMqWindowsService.Handlers;

namespace NetCoreRabbitMqWindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(configure => 
            {
                configure.UseStartup<NybusStartup>();

                configure.SetDisplayName("Nybus Windows Service with RabbitMq");

                configure.SetServiceName("NetCoreRabbitMqWindowsService");

                configure.SetDescription("A Windows service running Nybus");

                configure.EnableServiceRecovery(rc => rc.RestartService(TimeSpan.FromMinutes(1))
                                                        .RestartService(TimeSpan.FromMinutes(5))
                                                        .RestartService(TimeSpan.FromMinutes(10))
                                                        .SetResetPeriod(1));

                configure.RunAsLocalService();

                configure.StartAutomaticallyDelayed();

                configure.SetStopTimeout(TimeSpan.FromMinutes(5));
            });

            host.Run();
        }
    }

    public class NybusStartup : Startup
    {
        public override IBusHost ConstructService(StartupContext context, IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IBusHost>();
        }

        public override void ConfigureServices(StartupContext context, IServiceCollection services)
        {
            services.AddNybus(nybus =>
            {
                nybus.UseConfiguration(context.Configuration);

                nybus.UseRabbitMqBusEngine(rabbitMq => 
                {
                    rabbitMq.UseConfiguration();

                    rabbitMq.Configure(configuration => configuration.CommandQueueFactory = new StaticQueueFactory("NetCoreRabbitMqWindowsService"));
                });

                /* EVENTS */

                /* This will subscribe the event TestEvent to any IEventHandler<TestEvent> available */
                nybus.SubscribeToEvent<TestEvent>();

                /* This will subscribe the event TestEvent to an instance of TestEventHandler */
                // nybus.SubscribeToEvent<TestEvent, TestEventHandler>();

                /* This will subscribe the event TestEvent to the asynchronous delegate */
                // nybus.SubscribeToEvent<TestEvent>(async (dispatcher, eventContext) => { await DoSomethingAsync(); });

                /* This will subscribe the event TestEvent to the synchronous delegate */
                // nybus.SubscribeToEvent<TestEvent>((dispatcher, eventContext) => { DoSomething(); });


                /* COMMANDS */

                /* This will subscribe the command TestCommand to any ICommandHandler<TestCommand> available */
                nybus.SubscribeToCommand<TestCommand>();

                /* This will subscribe the command TestCommand to an instance of TestCommandHandler */
                // nybus.SubscribeToCommand<TestCommand, TestCommandHandler>();

                /* This will subscribe the command TestCommand to the asynchronous delegate */
                // nybus.SubscribeToCommand<TestCommand>(async (dispatcher, commandContext) => { await DoSomethingAsync(); });

                /* This will subscribe the command TestCommand to the synchronous delegate */
                // nybus.SubscribeToCommand<TestCommand>((dispatcher, commandContext) => { DoSomething(); });
            });

            /* EVENTS */

            /* This will register the event handler TestEventHandler as a handler for TestEvent */
            // services.AddEventHandler<TestEvent, TestEventHandler>();

            /* This will register the event handler TestEventHandler as a handler for all supported events */
            services.AddEventHandler<TestEventHandler>();


            /* COMMANDS */

            /* This will register the command handler TestCommandHandler as a handler for TestCommand */
            // services.AddCommandHandler<TestCommand, TestCommandHandler>();

            /* This will register the command handler TestCommandHandler as a handler for all supported Commands */
            services.AddCommandHandler<TestCommandHandler>();
        }

        public override void ConfigureAppConfiguration(IConfigurationBuilder configuration)
        {
            configuration.SetBasePath(Directory.GetCurrentDirectory());
            
            configuration.AddJsonFile($"appsettings.json", true);
            
            configuration.AddEnvironmentVariables("NYBUS_");
        }

        public override void ConfigureLogging(StartupContext context, ILoggingBuilder logging)
        {
            logging.AddConfiguration(context.Configuration.GetSection("Logging"));
            logging.AddConsole();
        }

        public override bool OnStart(IBusHost host, HostControl control)
        {
            return base.OnStart(host, control);
        }

        public override bool OnStop(IBusHost host, HostControl control)
        {
            return base.OnStop(host, control);
        }
    }

    public class TestEvent : IEvent { }

    public class TestCommand : ICommand { }
}
