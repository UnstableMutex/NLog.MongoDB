﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog.Common;
using NLog.LogReceiverService;
using NLog.Targets;

namespace NLog.MongoDB
{
    [Target("SubscribeMongoTarget")]
    public class SubscribeMongoTarget : MongoTargetBase
    {
        public SubscribeMongoTarget()
        {
            IsCapped = true;
            MaxSize = MB(200);
        }

        private string[] _subscribers;
        const string appnames = "AppNames";
        protected override void InitializeTarget()
        {
            var db = GetDatabase();
            var subscribersCollection = db.GetCollection("Subscribers");
            StringCollection coll = new StringCollection();
            foreach (BsonDocument sub in subscribersCollection.FindAll())
            {

                if (sub.Contains(appnames))
                {
                    var arr = sub[appnames].AsBsonArray;
                    if (arr.Contains(AppName))
                    {
                        coll.Add(sub["_id"].AsString);
                    }
                }
                else
                {
                    coll.Add(sub["_id"].AsString);
                }
            }
            if (coll.Count > 0)
            {
                _subscribers = coll.ToArray();
            }
            if (_subscribers != null)
            {
                base.InitializeTarget();
            }
        }

        public bool IsCapped { get; set; }
        public long MaxSize { get; set; }

        private long MB(long count)
        {
            return (long)(count * Math.Pow(1024, 2));
        }

        protected override void CreateCollection()
        {
            MongoDatabase db = GetDatabase();
            CollectionOptionsBuilder b = CollectionOptions.SetCapped(true).SetMaxSize(MaxSize);
            db.CreateCollection(CollectionName, b);
        }

        protected override BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            if (_subscribers == null)
            {
                return null;
            }
            var doc = base.GetDocFromLogEventInfo(logEvent);
            doc.Add("Subscribers", new BsonArray(_subscribers));
            return doc;
        }
        protected override void Write(LogEventInfo logEvent)
        {
            BsonDocument doc = GetDocFromLogEventInfo(logEvent);
            if (doc == null)
                return;
            if (doc["Subscribers"].AsBsonArray.Count > 0)
            {
                _coll.Save(doc);
            }
        }
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            IEnumerable<BsonDocument> docs = logEvents.Select(l => GetDocFromLogEventInfo(l.LogEvent)).Where(x => x != null);
            _coll.InsertBatch(docs);
        }

    }
}
