using MassTransit;
using MassTransit.Transports;
using Order.API.Models.Entities;
using Quartz;
using Shared.Events.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.OutBox.Table.Publisher.Service.Jobs
{
    public class OrderOutBoxPublishJob : IJob
    {
        public IPublishEndpoint _publishEnpoint;

        public OrderOutBoxPublishJob(IPublishEndpoint publishEnpoint)
        {
            _publishEnpoint = publishEnpoint;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderOutboxSingletonDatabase.DataReaderState())
            {
                OrderOutboxSingletonDatabase.DataReaderBusy();

                List<OrderOutbox> orderOutboxes =
                    (await OrderOutboxSingletonDatabase.QueryAsync<OrderOutbox>
                    ($@"SELECT * FROM OrderOutboxes WHERE PROCESSEDDATE IS NULL")).ToList();


                foreach(var orderOutbox in orderOutboxes)
                {
                    if(orderOutbox.Type == nameof(OrderCreatedEvent))
                    {
                        OrderCreatedEvent orderCreatedEvent =
                            JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);

                        if(orderCreatedEvent != null)
                        {
                            await _publishEnpoint.Publish(orderCreatedEvent);
                            OrderOutboxSingletonDatabase.ExecuteAsync(
                                $@"UPDATE ORDEROUTBOXES SET PROCCESSEDDATE = GETDATE() WHERE ID = '{orderOutbox.Id}'")
                        }

                    }
                }


                OrderOutboxSingletonDatabase.DataReaderReady();
            }
        }
    }
}

