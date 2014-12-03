using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLog.MongoDB
{
    public static class Ext
    {
        public static void WriteAppStarted(this Logger logger, Version appVersion = null)
        {
            if (appVersion == null)
            {
                logger.Info("App started.");
            }
            else
            {
                logger.Info("App started. Version {0}", appVersion);

            }
        }
        public static void WriteAppExit(this Logger logger, Version appVersion = null)
        {
            if (appVersion == null)
            {
                logger.Info("App exit.");
            }
            else
            {
                logger.Info("App exit. Version {0}", appVersion);

            }
        }
    }
}
