using System;
using MongoDB.Driver;
using NLog.MongoDB;
namespace NLog.MongoDB.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoClient cl = new MongoClient("mongodb://localhost");
            //cl.GetDatabase("Logs")
            cl.DropDatabaseAsync("Logs");
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("debug message");
            logger.Error("err raised");
            Console.WriteLine(Environment.UserName);
            logger.WriteAppStarted();
            logger.WriteAppStarted(new Version(1,2,3,4));
        }
    }
}
