using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog.Targets;

namespace NLog.MongoDB
{
    [Target("ExpireAtMongoTarget")]
    public class ExpireAtMongoTarget : MongoTargetBase
    {
        private const string fieldName = "ExpireAt";

        public ExpireAtMongoTarget()
        {
            Days = 10;
        }

        public int Days { get; set; }
        public int Hours { get; set; }

        protected override void CreateCollection()
        {
            base.CreateCollection();
            MongoCollection<BsonDocument> coll = GetDatabase().GetCollection(CollectionName);
            var b = new IndexKeysBuilder();
            var ob = new IndexOptionsBuilder();
            ob = ob.SetTimeToLive(new TimeSpan(0));
            b = b.Ascending(fieldName);
            coll.CreateIndex(b, ob);
        }

        protected override BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            var ts = new TimeSpan(Days, Hours, 0, 0);
            BsonDocument doc = base.GetDocFromLogEventInfo(logEvent);
            doc.Add(fieldName, DateTime.Now.Add(ts));
            return doc;
        }
    }
}