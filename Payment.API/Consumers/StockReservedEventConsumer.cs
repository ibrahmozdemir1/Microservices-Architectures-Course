using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            //Ödeme İşlemleri

            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                };

                _publishEndpoint.Publish(paymentCompletedEvent);
                //Ödeme başarılı ise burada belirteceğiz.

                Console.WriteLine("Ödeme Başarılı");
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    BuyerId = context.Message.BuyerId,
                    Message = "Yetersiz Bakiye...",
                };

                Console.WriteLine("Ödeme Başarısız.");


                _publishEndpoint.Publish(paymentFailedEvent);
                //Ödemede bir sorun olduysa buradan bildireceğiz.
            }


            return Task.CompletedTask;
        }
    }
}
