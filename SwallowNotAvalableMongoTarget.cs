using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
namespace NLog.MongoDB
{
    [Target("MongoDBCanFail")]
    class SwallowNotAvalableMongoTarget:TargetWithLayout
    {
        private MongoCollection<BsonDocument> _coll;
        public byte ExceptionRecursionLevel { get; set; }
        public bool Capped { get; set; }
        public int CappedSizeMB { get; set; }
        public string AppName { get; set; }
        [RequiredParameter]
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        private string CollectionName { get; set; }
        public SwallowNotAvalableMongoTarget()
        {
            Capped = true;
            CappedSizeMB = 200;
            ExceptionRecursionLevel = 2;
            DatabaseName = "Logs";
            AppName = "Application";
        }
        long MB(long count)
        {
            return (long)(count * Math.Pow(1024, 2));
        }
        protected override void InitializeTarget()
        {
            CheckCollectionExistance();
        }
        private void CheckCollectionExistance()
        {
            try
            {
                if (_coll == null)
                {
                    CollectionName = AppName + "Log";
                    var db = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
                    if (!db.CollectionExists(CollectionName))
                    {
                        var b = new CollectionOptionsBuilder();
                        if (Capped)
                        {
                            b = b.SetCapped(true).SetMaxSize(MB(CappedSizeMB));
                        }
                        db.CreateCollection(CollectionName, b);
                    }
                    _coll = db.GetCollection(CollectionName);
                }
            }
            catch (Exception)
            {
            }
        }
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var docs = logEvents.Select(l => GetDocFromLogEventInfo(l.LogEvent));
            CheckCollectionExistance();
            try
            {
                _coll.InsertBatch(docs);
            }
            catch (Exception e)
            {
            }
        }
        protected override void Write(LogEventInfo logEvent)
        {
            var doc = GetDocFromLogEventInfo(logEvent);
            CheckCollectionExistance();
            try
            {
                _coll.Save(doc);
            }
            catch (Exception e)
            {
            }
        }
        private BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            var doc = new BsonDocument();
            doc.Add("Level", logEvent.Level.ToString());
            doc.Add("UserName", Environment.UserName);
            doc.Add("WKS", Environment.MachineName);
            doc.Add("LocalDate", DateTime.Now);
            doc.Add("UTCDate", DateTime.Now.ToUniversalTime());
            doc.Add("Message", logEvent.FormattedMessage);
            doc.Add("AppName", AppName);
            if (logEvent.Exception != null)
            {
                doc.Add("Exception", GetException(logEvent.Exception));
            }
            return doc;
        }
        BsonDocument GetException<T>(T ex) where T : Exception
        {
            return GetException(ex, ExceptionRecursionLevel);
        }
        BsonDocument GetException<T>(T ex, byte level) where T : Exception
        {
            var doc = new BsonDocument();
            AddStringProperties(ex, doc);
            doc.Add("TargetSite", ex.TargetSite.Name);
            if (ex.TargetSite.DeclaringType != null)
            {
                doc.Add("ClassName", ex.TargetSite.DeclaringType.FullName);
            }
            if (ex.InnerException != null && level > 0)
            {
                doc.Add("InnerException", GetException(ex.InnerException, (byte)(level - 1)));
            }
            return doc;
        }
        private void AddStringProperties(Exception exception, BsonDocument doc)
        {
            var props = exception.GetType().GetProperties().Where(x => x.PropertyType == typeof(string));
            foreach (var pi in props)
            {
                string s = (string)pi.GetValue(exception, null);
                doc.Add(pi.Name, s);
            }
        }
    }
}
