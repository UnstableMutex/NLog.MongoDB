using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog.Targets;

namespace NLog.MongoDB
{
    [Target("CappedMongoTarget")]
   public class CappedMongoTarget : MongoTargetBase
    {
        public bool IsCapped { get; set; }
        public long MaxSize { get; set; }

        public CappedMongoTarget()
        {
            IsCapped = true;
            MaxSize = MB(200);
        }
        long MB(long count)
        {
            return (long)(count * Math.Pow(1024, 2));
        }
        protected override void CreateCollection()
        {
            var db = GetDatabase();
            CollectionOptionsBuilder b = new CollectionOptionsBuilder();
            b = b.SetCapped(IsCapped).SetMaxSize(MaxSize);
            db.CreateCollection(CollectionName, b);
        }

    }
}
