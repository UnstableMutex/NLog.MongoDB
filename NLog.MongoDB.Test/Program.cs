using MongoDB.Driver;

namespace NLog.MongoDB.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoClient cl=new MongoClient("mongodb://localhost");
            cl.GetServer().GetDatabase("Logs").Drop();
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("debug message");
        }
    }
}
