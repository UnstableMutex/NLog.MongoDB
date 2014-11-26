  class MongoTarget : NLog.Targets.Target
    {
        private readonly string _mongoCS;
        private readonly string _db;
        private readonly string _collection;
        private MongoCollection<BsonDocument> _coll;
        private const byte exlevel = 2;
        public MongoTarget(string mongoCS)
        {
            _mongoCS = mongoCS;
            _collection = App.Current.GetType().Namespace+"Log";
            _db = "Logs";
            _coll = new MongoClient(_mongoCS).GetServer().GetDatabase(_db).GetCollection(_collection);
        }
        public MongoTarget(string mongoCS, string db, string collection)
        {
            _mongoCS = mongoCS;
            _db = db;
            _collection = collection;
            _coll = new MongoClient(_mongoCS).GetServer().GetDatabase(_db).GetCollection(_collection);
        }
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
        }
        protected override void CloseTarget()
        {
            base.CloseTarget();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            base.FlushAsync(asyncContinuation);
        }
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            base.Write(logEvents);
        }
        protected override void Write(LogEventInfo logEvent)
        {
            var doc = new BsonDocument();
            doc.Add("Level", logEvent.Level.ToString());
            doc.Add("Message", logEvent.FormattedMessage);
            if (logEvent.Exception != null)
            {
                doc.Add("Exception", GetException(logEvent.Exception));
            }
            _coll.Save(doc);
        }
        BsonDocument GetException(Exception ex, byte level = exlevel)
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
