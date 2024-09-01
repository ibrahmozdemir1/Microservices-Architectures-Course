using EventStore.Client;
using System.Text.Json;

string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";
var settings = EventStoreClientSettings.Create(connectionString);
var client = new EventStoreClient(settings);


OrderPlacedEvent orderPlacedEvent = new()
{
    OrderID = 1,
    TotalAmount = 50,
};


//while (true)
//{
//    EventData eventData = new(
//        eventId: Uuid.NewUuid(),
//        type: orderPlacedEvent.GetType().Name,
//        data: JsonSerializer.SerializeToUtf8Bytes(orderPlacedEvent)
//        );


//    await client.AppendToStreamAsync(
//         streamName: "order-stream",
//         expectedState: StreamState.Any,
//         eventData: new[] { eventData }
//     );
//}

//var events = client.ReadStreamAsync(
//    streamName: "order-stream",
//    direction: Direction.Forwards,
//    revision: StreamPosition.Start
//    );

//var datas = await events.ToListAsync();

//Console.WriteLine("   ");

await client.SubscribeToStreamAsync(
    streamName: "order-stream",
    start: FromStream.Start,
    eventAppeared: async (streamSubscription, resolvedEvent, cancellationToken) =>
    {
      
        
        OrderPlacedEvent @event =  JsonSerializer.Deserialize<OrderPlacedEvent>(resolvedEvent.Event.Data.ToArray());

        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(@event));
    },
    subscriptionDropped: (streamSubscription, subscriptionDroppedReason, exception) 
    => Console.WriteLine("Disconnected")
   
    );

Console.Read();

class OrderPlacedEvent
{
    public int OrderID { get; set; }
    public int TotalAmount { get; set; }
}