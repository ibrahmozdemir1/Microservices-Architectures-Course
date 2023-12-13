using MassTransit;
using Shared.Events;
using Shared.Events.PaymentEvents;
using Shared.Settings;

namespace Payment.API.Consumers
{
    public class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
    {
        ISendEndpointProvider _sendEndpointProvider;

        public PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            //Ödeme İşlemleri

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(
                $"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId)
                {
                    
                };

                await sendEndpoint.Send(paymentCompletedEvent);
                //Ödeme başarılı ise burada belirteceğiz.

                Console.WriteLine("Ödeme Başarılı");
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Yetersiz Bakiye...",
                    OrderItems = context.Message.OrderItems,
                };

                Console.WriteLine("Ödeme Başarısız.");


                await sendEndpoint.Send(paymentFailedEvent);
                //Ödemede bir sorun olduysa buradan bildireceğiz.
            }
        }
    }
}
