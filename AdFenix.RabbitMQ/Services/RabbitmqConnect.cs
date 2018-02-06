using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using AdFenix.RabbitMQ.Enums;

namespace AdFenix.RabbitMQ.Services
{
    public class RabbitmqConnect : IRabbitmqConnect
    {
        private static IConnection _connection = null;
        private static Object lockConnection = new Object();
        private IOptions<RabbitmqOptions> rabbitmqOptions;

        public RabbitmqConnect(IOptions<RabbitmqOptions> rabbitmqOptions)
        {
            this.rabbitmqOptions = rabbitmqOptions;
        }

        private IConnection CreateConnection()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.HostName = this.rabbitmqOptions.Value.HostName;
            connectionFactory.UserName = this.rabbitmqOptions.Value.UserName;
            connectionFactory.Password = this.rabbitmqOptions.Value.Password;
            connectionFactory.Port = this.rabbitmqOptions.Value.Port;
            connectionFactory.VirtualHost = this.rabbitmqOptions.Value.VirtualHost;

            return connectionFactory.CreateConnection();
        }

        public IConnection GetConnection()
        {
            lock (lockConnection)
            {
                if (_connection == null)
                {
                    _connection = CreateConnection();
                }

                return _connection;
            }
        }

        //We have move to single conncetion per server. So this method is unused now.
        public void CloseConnection()
        {
            lock (lockConnection)
            {
                if (_connection != null)
                {
                    _connection.Close();
                }
            }

        }

        public IModel SetExchange(string exchangeName, string exchangeType)
        {
            var connection = GetConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);

            SetDeadLetterExchangeAndDefaultQueue(channel);

            return channel;
        }

        public IModel SetQueue(string exchangeName, string queueName, string routingKey, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments = null)
        {
            var connection = GetConnection();
            var channel = connection.CreateModel();

            var queueArgs = new Dictionary<string, object>();
            queueArgs.Add("x-dead-letter-exchange", RabbitmqConfig.DeadLetterExchange);
            queueArgs.Add("x-dead-letter-routing-key", routingKey);

            try
            {
                channel.QueueDeclare(queueName, durable, exclusive, autoDelete, queueArgs);
            }
            catch (Exception)
            {
                //durable queque does not allow modification.
                channel = connection.CreateModel();

                //Only delete empty queque for production
                channel.QueueDelete(queueName,false, false);
                channel.QueueDeclare(queueName, durable, exclusive, autoDelete, queueArgs);
            }

            channel.QueueBind(queueName, exchangeName, routingKey, arguments);

            SetDeadLetterQueue(channel,queueName,routingKey,durable,exclusive,autoDelete);

            return channel;
        }

        private void SetDeadLetterExchangeAndDefaultQueue(IModel channel)
        {
            channel.ExchangeDeclare(RabbitmqConfig.DeadLetterExchange, RabbitmqExchangeType.Topic, true, false, null);

            try
            {
                channel.QueueDeclare(RabbitmqConfig.DeadLetterAllQueue, true, false, false, null);
            }
            catch (Exception)
            {
                channel.QueueDelete(RabbitmqConfig.DeadLetterAllQueue, false, false);
                channel.QueueDeclare(RabbitmqConfig.DeadLetterAllQueue, true, false, false, null);
            }
            

            //Note: default dead letter queue will recive all dead letter. 
            //This will use only for monitoring, should be purge message after monitoring each time.
            channel.QueueBind(RabbitmqConfig.DeadLetterAllQueue, RabbitmqConfig.DeadLetterExchange,"#");
        }

        private void SetDeadLetterQueue(IModel channel,string refQueueName,string routingKey,bool durable,bool exclusive,bool autoDelete)
        {
            var dlxQueueName = $"dlx.{refQueueName}";

            try
            {
                channel.QueueDeclare(dlxQueueName, durable, exclusive, autoDelete, null);
            }
            catch (Exception)
            {
                channel.QueueDelete(dlxQueueName, false, false);
                channel.QueueDeclare(dlxQueueName, durable, exclusive, autoDelete, null);
            }
            

            channel.QueueBind(dlxQueueName, RabbitmqConfig.DeadLetterExchange, routingKey);
        }

    }
}
