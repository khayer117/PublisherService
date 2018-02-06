
using AdFenix.Infrastructure.Commands;

namespace AdFenix.RabbitMQ.Services
{
    public interface IRabbitmqProducerService
    {
        /// <summary>
        /// Declare Exchange 
        /// </summary>
        void SetExchange(string exchangeName, string exchangeType);
        
        /// <summary>
        /// Publish command to specific Exchange 
        /// </summary>
        void BasicPublish(string exchangeName,IQueueCommand command,string routingKey="", bool isDoChannelClose = true);

        void CloseChannel();

    }
}
