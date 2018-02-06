using AdFenix.RabbitMQ.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AdFenix.RabbitMQ
{
    public class RabbitmqServiceRegistration
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IRabbitmqConnect, RabbitmqConnect>();
            services.AddTransient<IRabbitmqProducerService, RabbitmqProducerService>();
            services.AddTransient<IRabbitmqConsumerService, RabbitmqConsumerService>();

        }

    }
}
