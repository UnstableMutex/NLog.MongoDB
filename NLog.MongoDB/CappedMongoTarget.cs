using System;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog.Targets;

namespace NLog.MongoDB
{
    [Target("CappedMongoTarget")]
    public class CappedMongoTarget : MongoTargetBase
    {
        public CappedMongoTarget()
        {
            IsCapped = true;
            MaxSize = MB(200);
        }

        public bool IsCapped { get; set; }
        public long MaxSize { get; set; }

        private long MB(long count)
        {
            return (long) (count*Math.Pow(1024, 2));
        }

        protected override void CreateCollection()
        {
            MongoDatabase db = GetDatabase();
            CollectionOptionsBuilder b = CollectionOptions.SetCapped(true).SetMaxSize(MaxSize);
            db.CreateCollection(CollectionName, b);
        }
    }
}