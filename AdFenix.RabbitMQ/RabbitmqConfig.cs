namespace AdFenix.RabbitMQ
{
    public class RabbitmqConfig
    {

        public const string QueueBasicEvent = "event.addpublisher";
        public const string ExchangeBasicEvent = "event.exchange";

        public const string DeadLetterExchange = "dlx.common";
        public const string DeadLetterAllQueue = "dlx.queue.all";

        public const string RoutingKeyBasicEventAddPublisher = "event.addpublisher";

    }
}
