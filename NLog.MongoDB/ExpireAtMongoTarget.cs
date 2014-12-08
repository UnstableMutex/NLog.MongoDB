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
    [Target("ExpireAtMongoTarget")]
   public class ExpireAtMongoTarget : MongoTargetBase
    {
        protected override void CreateCollection()
        {
            base.CreateCollection();
            var coll = GetDatabase().GetCollection(CollectionName);
            IndexKeysBuilder b = new IndexKeysBuilder();
            IndexOptionsBuilder ob = new IndexOptionsBuilder();
            ob = ob.SetTimeToLive(new TimeSpan(0));
            b = b.Ascending(fieldName);
            coll.CreateIndex(b, ob);

        }

        private const string fieldName = "ExpireAt";
        protected override BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            TimeSpan ts = new TimeSpan(Days,Hours,0,0);
            var doc = base.GetDocFromLogEventInfo(logEvent);
            doc.Add(fieldName, DateTime.Now.Add(ts));
            return doc;
        }
        public int Days { get; set; }
        public int Hours { get; set; }

        public ExpireAtMongoTarget()
        {
            Days = 10;
        }

      
    }
}
