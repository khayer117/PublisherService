using System.Collections.Generic;
using AdFenix.Infrastructure.Commands;

namespace AdFenix.RabbitMQ.Services
{
    public interface IRabbitmqConsumerService
    {
        /// <summary>
        /// Declare Queue, then bind with exchage using routing key 
        /// </summary>
        void SetQueue(string exchangeName, string exchangeType, string queueName, string routingKey, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null);
        
        /// <summary>
        /// Attached Reciever for specific queue 
        /// </summary>
        void ReceiveMessages(string quaueName);

        /// <summary>
        /// Attached Reciever for specific queue 
        /// </summary>
        void ReceiveMessages<T>(string queueName) where T : IQueueCommand;
    }
}
