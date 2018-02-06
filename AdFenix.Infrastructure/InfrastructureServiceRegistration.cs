using AdFenix.Infrastructure.Mediators;
using Microsoft.Extensions.DependencyInjection;

namespace AdFenix.Infrastructure
{
    public class InfrastructureServiceRegistration
    {
        public static void Register(IServiceCollection services)
        {

            services.AddTransient<IActionCommandDispacher, ActionCommandDispacher>();
        }
        
    }
}
