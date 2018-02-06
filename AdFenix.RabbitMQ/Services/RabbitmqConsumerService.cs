using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using AdFenix.Infrastructure.Commands;
using AdFenix.Infrastructure.Mediators;

namespace AdFenix.RabbitMQ.Services
{
    public class RabbitmqConsumerService : IRabbitmqConsumerService
    {
        private IRabbitmqConnect rabbitmqConnect;
        private IActionCommandDispacher actionCommandDispacher;
        //private ILogger logger;
        private const string commandTypeName = "commandType";
        //private ISlackSimpleClient slackSimpleClient;

        public RabbitmqConsumerService(IRabbitmqConnect rabbitmqConnect,IActionCommandDispacher actionCommandDispacher)
        {
            this.rabbitmqConnect = rabbitmqConnect;
            this.actionCommandDispacher = actionCommandDispacher;
            //this.logger = logger;
            //this.slackSimpleClient = slackSimpleClient;
        }
        public void SetQueue(string exchangeName, string exchangeType, string queueName, string routingKey, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            try
            {
                using (this.rabbitmqConnect.SetExchange(exchangeName, exchangeType))
                {
                    if (!string.IsNullOrEmpty(queueName))
                    {
                        using (this.rabbitmqConnect.SetQueue(exchangeName, queueName, routingKey,
                            durable, exclusive, autoDelete, arguments)){}
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "Rabbitmq Set Queue failed.";
                //this.logger.Error(ex,errorMsg);
                //this.slackSimpleClient.PostException(SlackSetting.ERROR_EXCEPTION_WEB_HOOK,ex,errorMsg);
                Console.WriteLine(errorMsg + ":" +ex.Message);
                throw;
            }
        }

        public void ReceiveMessages(string quaueName)
        {
            // Bad, but added this try catch considering starcounter.
            try
            {
                var connection = this.rabbitmqConnect.GetConnection();
                var channel = connection.CreateModel();
                channel.BasicQos(0, 1, false);
                var eventingBasicConsumer = new EventingBasicConsumer(channel);

                eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
                {
                    var isSucceedAck = true;
                    var basicProperties = basicDeliveryEventArgs.BasicProperties;

                    if (basicProperties.Headers != null && basicProperties.Headers.ContainsKey(commandTypeName))
                    {
                        try
                        {
                            var commandTypeBytes = basicProperties.Headers[commandTypeName] as byte[];
                            var commandType = Encoding.UTF8.GetString(commandTypeBytes);
                            Type t = Type.GetType(commandType);
                            isSucceedAck = DispatchCommand(quaueName, basicDeliveryEventArgs, channel, Type.GetType(commandType));
                        }
                        catch (Exception ex)
                        {
                            //Todo: Need to acknowledge to System admin.(slack, email)
                            isSucceedAck = false;
                            //this.logger.Error(ex, "Error in Basic Consumer");
                            Console.WriteLine(ex);

                        }
                    }
                    else
                    {
                        //this.logger.Info($"Skipping message. Invalid {commandTypeName} type in property header.");
                    }

                    HandleAck(basicDeliveryEventArgs, channel, isSucceedAck);
                };

                channel.BasicConsume(quaueName, false, eventingBasicConsumer);
                //this.logger.Info($"Waiting for messages from {quaueName}");
            }
            catch (Exception ex)
            {
                //var errorMsg = "Rabbitmq ReceiveMessages failed.";
                Console.WriteLine(ex);
                //this.logger.Error(ex, errorMsg);
                //this.slackSimpleClient.PostException(SlackSetting.ERROR_EXCEPTION_WEB_HOOK, ex, errorMsg);
            }
        }

        /// <summary>
        /// Listen to a queue for fixed format messages
        /// </summary>
        /// <typeparam name="T">The command type to deserialize the message onto</typeparam>
        /// <param name="queueName">Queue name to listen to</param>
        public void ReceiveMessages<T>(string queueName) where T : IQueueCommand
        {
            // Bad, but added this try catch considering starcounter.
            try
            {
                var connection = this.rabbitmqConnect.GetConnection();
                var channel = connection.CreateModel();
                channel.BasicQos(0, 1, false);
                var eventingBasicConsumer = new EventingBasicConsumer(channel);

                eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
                {
                    bool isSuccessAck = DispatchCommand(queueName, basicDeliveryEventArgs, channel, typeof(T));
                    HandleAck(basicDeliveryEventArgs, channel, isSuccessAck);
                };

                channel.BasicConsume(queueName, false, eventingBasicConsumer);
                //this.logger.Info($"Waiting for messages from {queueName}");
            }
            catch (Exception ex)
            {
                var errorMsg = String.Format("Rabbitmq ReceiveMessages<{0}> failed.", typeof(T).ToString());
                //this.logger.Error(ex, errorMsg);
                //this.slackSimpleClient.PostException(SlackSetting.ERROR_EXCEPTION_WEB_HOOK, ex, errorMsg);
                Console.WriteLine(ex);
            }
        }

        private bool DispatchCommand(string queueName, BasicDeliverEventArgs basicDeliveryEventArgs, IModel channel, Type commandType)
        {
            var isSucceedAck = true;
            var message = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);

            try
            {
                var recieveCommand = JsonConvert.DeserializeObject(message, commandType);
                //this.logger.Info($"Message received from {queueName}: {JsonConvert.SerializeObject(recieveCommand)}");
                this.actionCommandDispacher.Send(recieveCommand).Wait();
            }
            catch (Exception ex)
            {
                isSucceedAck = false;
                //this.logger.Error(ex, "Error in Basic Consumer");
                Console.WriteLine(ex);
            }

            return isSucceedAck;
        }

        private void HandleAck(BasicDeliverEventArgs basicDeliveryEventArgs, IModel channel, bool isSucceedAck)
        {
            try
            {
                if (isSucceedAck)
                {
                    channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                }
                else
                {
                    //Dead Letter Exchanges(https://www.rabbitmq.com/dlx.html) already configured.
                    channel.BasicNack(basicDeliveryEventArgs.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                //var errMsg = "Error in HandleAck";
                //this.logger.Error(ex, errMsg);
                //this.slackSimpleClient.PostException(SlackSetting.ERROR_EXCEPTION_WEB_HOOK, ex, errMsg);
                Console.WriteLine(ex);
            }
        }
    }
}