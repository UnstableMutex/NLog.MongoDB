using System;
using MongoDB.Driver;
using NLog.MongoDB;
namespace NLog.MongoDB.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //MongoClient cl=new MongoClient("mongodb://localhost");
            //cl.GetServer().GetDatabase("Logs").Drop();
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("debug message");
            logger.WriteAppStarted();
            logger.WriteAppStarted(new Version(1,2,3,4));
        }
    }
}
