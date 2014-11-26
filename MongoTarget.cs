    class MongoTarget : NLog.Targets.Target
    {
        private readonly string _mongoCS;
        private readonly string _db;
        private readonly string _collection;
        private MongoCollection<BsonDocument> _coll;
        public byte ExceptionRecursionLevel { get; set; }
        public bool Capped { get; set; }
        public int CappedSizeMB { get; set; }
        public MongoTarget()
        {
            Capped = true;
            CappedSizeMB = 200;
            ExceptionRecursionLevel = 2;
            _collection = App.Current.GetType().Namespace + "Log";
            _db = "Logs";
        }
        public MongoTarget(string mongoCS)
            : this()
        {
            _mongoCS = mongoCS;
            _coll = GetCollection();
        }
        long MB(long count)
        {
            return (long)(count * Math.Pow(1024, 2));
        }
        MongoCollection<BsonDocument> GetCollection()
        {
            var db = new MongoClient(_mongoCS).GetServer().GetDatabase(_db);
            if (!db.CollectionExists(_collection))
            {
                var b = new MongoDB.Driver.Builders.CollectionOptionsBuilder();
                if (Capped)
                {
                    b = b.SetCapped(true).SetMaxSize(MB(CappedSizeMB));
                }
                db.CreateCollection(_collection, b);
            }
            return db.GetCollection(_collection);
        }
        public MongoTarget(string mongoCS, string db, string collection)
            : this()
        {
            _mongoCS = mongoCS;
            _db = db;
            _collection = collection;
            _coll = GetCollection();
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
            doc.Add("LocalDate", DateTime.Now);
            doc.Add("UTCDate", DateTime.Now.ToUniversalTime());
            doc.Add("Message", logEvent.FormattedMessage);
            if (logEvent.Exception != null)
            {
                doc.Add("Exception", GetException(logEvent.Exception));
            }
            return doc;
        }
        BsonDocument GetException(Exception ex)
        {
            return GetException(ex, ExceptionRecursionLevel);
        }
        BsonDocument GetException(Exception ex, byte level)
        {
            var doc = new BsonDocument();
            doc.Add("Message", ex.Message);
            doc.Add("StackTrace", ex.StackTrace);
            doc.Add("Source", ex.Source);
            if (ex.InnerException != null && level > 0)
            {
                doc.Add("InnerException", GetException(ex.InnerException, (byte)(level - 1)));
            }
            return doc;
        }
    }
