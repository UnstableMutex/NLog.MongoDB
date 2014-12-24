using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.MongoDB
{
    [Target("MongoTarget")]
    public class MongoTargetBase : TargetWithLayout
    {
        protected MongoCollection<BsonDocument> _coll;

        public MongoTargetBase()
        {
            ExceptionRecursionLevel = 2;
            DatabaseName = "Logs";
            AppName = "Application";
            CollectionName = AppName + "Log";
        }

        public byte ExceptionRecursionLevel { get; set; }
        public string AppName { get; set; }

        [RequiredParameter]
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
        protected string CollectionName { get; set; }

        protected MongoDatabase GetDatabase()
        {
            MongoDatabase db = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
            return db;
        }

        protected override void InitializeTarget()
        {
            CollectionName = AppName + "Log";
            if (_coll == null)
            {
                MongoDatabase db = GetDatabase();
                if (!db.CollectionExists(CollectionName))
                {
                    CreateCollection();
                }
                _coll = db.GetCollection(CollectionName);
            }
        }

        protected virtual void CreateCollection()
        {
            MongoDatabase db = GetDatabase();
            db.CreateCollection(CollectionName);
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            IEnumerable<BsonDocument> docs = logEvents.Select(l => GetDocFromLogEventInfo(l.LogEvent));
            _coll.InsertBatch(docs);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            BsonDocument doc = GetDocFromLogEventInfo(logEvent);
            _coll.Save(doc);
        }

        protected virtual BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            var doc = new BsonDocument();
            doc.Add("Level", logEvent.Level.ToString());
            doc.Add("UserName", Environment.UserName);
            doc.Add("WKS", Environment.MachineName);
            DateTime nowformongo = DateTime.Now.Add(DateTimeOffset.Now.Offset);
            doc.Add("LocalDate", nowformongo);
            doc.Add("UTCDate", DateTime.Now.ToUniversalTime());
            doc.Add("Message", logEvent.FormattedMessage);
            doc.Add("AppName", AppName);
            if (logEvent.Exception != null)
            {
                doc.Add("Exception", GetException(logEvent.Exception));
            }
            return doc;
        }

        private BsonDocument GetException<T>(T ex) where T : Exception
        {
            return GetException(ex, ExceptionRecursionLevel);
        }

        private BsonDocument GetException<T>(T ex, byte level) where T : Exception
        {
            var doc = new BsonDocument();
            AddStringProperties(ex, doc);
            AddIntProperties(ex, doc);
            doc.Add("exType", ex.GetType().FullName);
            if (ex.TargetSite != null)
            {
                doc.Add("TargetSite", ex.TargetSite.Name);
                if (ex.TargetSite.DeclaringType != null)
                {
                    doc.Add("ClassName", ex.TargetSite.DeclaringType.FullName);
                }
            }
            if (ex.InnerException != null && level > 0)
            {
                doc.Add("InnerException", GetException(ex.InnerException, (byte)(level - 1)));
            }
            return doc;
        }

        private void AddIntProperties(Exception exception, BsonDocument doc)
        {
            IEnumerable<PropertyInfo> props =
                exception.GetType().GetProperties().Where(x => x.PropertyType == typeof(int));
            foreach (PropertyInfo pi in props)
            {
                var s = (int)pi.GetValue(exception, null);
                doc.Add(pi.Name, s);
            }
        }

        private void AddStringProperties(Exception exception, BsonDocument doc)
        {
            IEnumerable<PropertyInfo> props =
                exception.GetType().GetProperties().Where(x => x.PropertyType == typeof(string));
            foreach (PropertyInfo pi in props)
            {
                var s = (string)pi.GetValue(exception, null);
                doc.Add(pi.Name, s);
            }
        }
    }
}