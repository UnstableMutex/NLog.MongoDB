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
    [Target("MongoDBExpireAt")]
    public class MongoTargetExpireAt : Target
    {

        private MongoCollection<BsonDocument> _coll;
        public byte ExceptionRecursionLevel { get; set; }

        public int RecordExistsDays { get; set; }
        public int RecordExistsHours { get; set; }
        public int RecordExistsMinutes { get; set; }
        public int RecordExistsSeconds { get; set; }

        public string AppName { get; set; }
        [RequiredParameter]
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        private string CollectionName { get; set; }
        private TimeSpan _timeSpan;
        public MongoTargetExpireAt()
        {

            RecordExistsSeconds = 40;

            _timeSpan= new TimeSpan(RecordExistsDays, RecordExistsHours,RecordExistsMinutes,RecordExistsSeconds);
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
            if (_coll == null)
            {
              IndexKeysBuilder b=new IndexKeysBuilder();
                IndexOptionsBuilder ob=new IndexOptionsBuilder();
                ob = ob.SetTimeToLive(new TimeSpan(0));
                b = b.Ascending("ExpireAt");
                CollectionName = AppName + "Log";
                var db = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
                if (!db.CollectionExists(CollectionName))
                {
                    db.CreateCollection(CollectionName );
                    _coll = db.GetCollection(CollectionName);
                    _coll.CreateIndex(b, ob);
                }
                _coll = db.GetCollection(CollectionName);
            }
        }
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var docs = logEvents.Select(l => GetDocFromLogEventInfo(l.LogEvent));

            _coll.InsertBatch(docs);
        }
        protected override void Write(LogEventInfo logEvent)
        {
            var doc = GetDocFromLogEventInfo(logEvent);
            _coll.Save(doc);
        }
        private BsonDocument GetDocFromLogEventInfo(LogEventInfo logEvent)
        {
            var doc = new BsonDocument();
            doc.Add("Level", logEvent.Level.ToString());
            doc.Add("UserName", Environment.UserName);
            doc.Add("WKS", Environment.MachineName);
            DateTime nowformongo = DateTime.Now.Add(DateTime.Now - DateTime.Now.ToUniversalTime());
            doc.Add("LocalDate", nowformongo);
            doc.Add("UTCDate", DateTime.Now.ToUniversalTime());
            doc.Add("Message", logEvent.FormattedMessage);
            doc.Add("AppName", AppName);
            doc.Add("ExpireAt", DateTime.Now.Add(_timeSpan));
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
            doc.Add("exType", ex.GetType().FullName);
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
                string s = (string)pi.GetValue(exception);
                doc.Add(pi.Name, s);
            }
        }
    }
}
