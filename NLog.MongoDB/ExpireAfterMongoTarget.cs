using System;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Targets;
namespace NLog.MongoDB
{
    [Target("ExpireAfterMongoTarget")]
    public class ExpireAfterMongoTarget : MongoTargetBase
    {
        private const string fieldName = "CreatedAt";
        public ExpireAfterMongoTarget()
        {
            Days = 20;
        }
        public int Days { get; set; }
        protected override void CreateCollection()
        {
            base.CreateCollection();
            var coll = GetDatabase().GetCollection<BsonDocument>(CollectionName);
            var ind = Builders<BsonDocument>.IndexKeys.Descending(fieldName);
            CreateIndexOptions cio = new CreateIndexOptions();
            cio.ExpireAfter = TimeSpan.FromDays(Days);
            coll.Indexes.CreateOneAsync(ind, cio);
        }
        protected override BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            BsonDocument doc = base.GetDocFromLogEventInfo(logEvent);
            doc.Add(fieldName, DateTime.Now);
            return doc;
        }
    }
}