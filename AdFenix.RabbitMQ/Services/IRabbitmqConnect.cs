using System.Collections.Generic;
using RabbitMQ.Client;
namespace AdFenix.RabbitMQ.Services
{
    public interface IRabbitmqConnect
    {
        IConnection GetConnection();
        void CloseConnection();
        IModel SetExchange(string exchangeName, string exchangeType);
        IModel SetQueue(string exchangeName, string queueName, string routingKey, bool durable=true, bool exclusive=false, bool autoDelete=false, IDictionary<string, object> arguments = null);
    }
}
