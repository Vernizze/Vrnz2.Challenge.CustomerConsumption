using MongoDB.Bson.Serialization.Attributes;
using System;
using Vrnz2.Data.MongoDB.Entities.Base;

namespace Vrnz2.Challenge.CustomerConsumption.Shared.Entities
{
    [BsonIgnoreExtraElements]
    public class CustomerConsumption
        : BaseMongoDbEntity
    {
        public string Cpf { get; set; }
        public string CustomerName { get; set; }
        public DateTime PaymentDate { get; set; }
        public int YearReference { get; set; }
        public int MonthReference { get; set; }
        public decimal Value { get; set; }
    }
}
