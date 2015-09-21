using System;
using MongoDB.Bson;
using MongoDB.Driver;
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
            IMongoCollection<BsonDocument> coll = GetDatabase().GetCollection<BsonDocument>(CollectionName);
            var ob = new CreateIndexOptions();
            ob.ExpireAfter = TimeSpan.FromTicks(0);
            var b = Builders<BsonDocument>.IndexKeys.Ascending(fieldName);
            coll.Indexes.CreateOneAsync(b, ob);
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