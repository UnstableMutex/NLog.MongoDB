using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        protected override void CreateCollection()
        {
            base.CreateCollection();
            var coll = GetDatabase().GetCollection(CollectionName);
            IndexKeysBuilder b = new IndexKeysBuilder();
            IndexOptionsBuilder ob = new IndexOptionsBuilder();
            ob = ob.SetTimeToLive(TimeSpan.FromDays(Days));
            b = b.Ascending(fieldName);
            coll.CreateIndex(b, ob);
        }

        public int Days { get; set; }
        protected override BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            var doc = base.GetDocFromLogEventInfo(logEvent);
            doc.Add(fieldName, DateTime.Now);
            return doc;
        }
    }
}
