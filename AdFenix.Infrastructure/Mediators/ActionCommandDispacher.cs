using System;
using System.Threading.Tasks;

namespace AdFenix.Infrastructure.Mediators
{
    public class ActionCommandDispacher:IActionCommandDispacher
    {
        private static readonly Type GenericCommandHandlerType = typeof(IActionCommandHandler<>);
        //private readonly Func<Type, object> resolver;
        private IServiceProvider serviceProvider;
        //private readonly ILogger logger;

        public ActionCommandDispacher(IServiceProvider serviceProvider)
        {
            //this.resolver = resolver;
            //this.logger = logger;
            this.serviceProvider = serviceProvider;
        }
        public async Task Send(object command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var handlerType = GenericCommandHandlerType.MakeGenericType(command.GetType());
            dynamic handler = null;

            try
            {
                //handler = this.resolver(handlerType);
                handler = this.serviceProvider.GetService(handlerType);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //this.logger.Warn($"Could not find \"{command.ToString()}\" handler. Skipped this command.");
                return;
            }

            if (handler == null)
            {
                //this.logger.Warn($"Could not find \"{command.ToString()}\" handler. Skipped this command.");
                return;
            }

            try
            {
                await handler.Handle((dynamic)command);

                var disposable = handler as IDisposable;
                disposable?.Dispose();
            }
            catch (Exception e)
            {
                var s = e.Message;
                //this.logger.Error(e, "CommandBus- Handler {Handler} generates error.", handler.ToString());
                throw;
            }
        }
    }
}
