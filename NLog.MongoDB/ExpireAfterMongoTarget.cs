using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
            MongoCollection<BsonDocument> coll = GetDatabase().GetCollection(CollectionName);
            var b = new IndexKeysBuilder();
            var ob = new IndexOptionsBuilder();
            ob = ob.SetTimeToLive(TimeSpan.FromDays(Days));
            b = b.Ascending(fieldName);
            coll.CreateIndex(b, ob);
        }

        protected override BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            BsonDocument doc = base.GetDocFromLogEventInfo(logEvent);
            doc.Add(fieldName, DateTime.Now);
            return doc;
        }
    }
}