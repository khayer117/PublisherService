using AdFenix.Entities;
using AdFenix.Infrastructure;
using AdFenix.Infrastructure.Commands;
using AdFenix.Infrastructure.Helpers;
using AdFenix.Infrastructure.Mediators;
using AdFenix.RabbitMQ;
using AdFenix.RabbitMQ.Enums;
using AdFenix.RabbitMQ.Services;
using AdFenix.Consumer.Service.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdFenix.Consumer.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            RegisterCustomServices(services);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                app.UseExceptionHandler();
            }

            app.UseStatusCodePages();

            app.UseMvc();

            RunRabbitConsumer(serviceProvider);
        }
        public void RegisterCustomServices(IServiceCollection services)
        {
            services.Configure<RabbitmqOptions>(Configuration.GetSection("Rabbitmq"));

            InfrastructureServiceRegistration.Register(services);
            RabbitmqServiceRegistration.Register(services);

            var connectionString = Configuration["ConnectionStrings:AdfenixConnectionString"];
            DataContextRegistration.Register(services, connectionString);

            services
                .AddTransient<IActionCommandHandler<AddPublicationOwnerCommand>, AddPublicationOwnerCommandHandler>();

        }

        public void RunRabbitConsumer(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Starting Rabbit Reciver..");
            Thread.Sleep(15000);

            try
            {
                var rabbitmqConsumerService = serviceProvider.GetService<IRabbitmqConsumerService>();
                Retry.Do(() =>
                {
                    rabbitmqConsumerService.SetQueue(RabbitmqConfig.ExchangeBasicEvent,
                        RabbitmqExchangeType.Direct,
                        RabbitmqConfig.QueueBasicEvent,
                        RabbitmqConfig.RoutingKeyBasicEventAddPublisher);

                    Task.Run(() => rabbitmqConsumerService.ReceiveMessages(RabbitmqConfig.QueueBasicEvent));
                }, TimeSpan.FromSeconds(10), 200);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Rabbit Reciver fails to start.");
                Console.WriteLine(ex);
            }


            Console.WriteLine($"Listening {RabbitmqConfig.QueueBasicEvent}");
        }
    }
}
