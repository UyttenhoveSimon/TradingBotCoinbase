using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BotTradingCoinbase
{
    public static class Extensions
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static List<PropertyInfo> Properties(object obj)
        {
            return obj.GetType().GetProperties().ToList();
        }

        public static List<string> LoopProperties(object obj)
        {
            var properties = new List<string>();
            foreach (var prop in Properties(obj))
            {
                if (prop.GetIndexParameters().Length == 0)
                {
                    Logger.Trace(prop.Name + " " + prop.GetValue(obj));
                    properties.Add(prop.Name + " " + prop.GetValue(obj));
                }
            }
            Logger.Trace("**********************************************");

            return properties;
        }
    }
}