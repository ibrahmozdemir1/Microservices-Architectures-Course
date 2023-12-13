using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stock.API.Models.Entities
{
    public class Stock
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [BsonElement(Order = 0)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]
        [BsonElement(Order = 1)]
        public int ProductId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]
        [BsonElement(Order = 2)]
        public int Count { get; set; }
    }
}
