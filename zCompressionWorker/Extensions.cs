using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace zCompressionWorker
{
    public static class Extensions
    {
        public static void LogProperties(this ILogger logger, object objToLog)
        {
            var objType = objToLog.GetType();

            var properties = new List<PropertyInfo>(objType.GetProperties());

            foreach (PropertyInfo prop in properties)
            {
                var propValue = prop.GetValue(objToLog, null);
                logger.LogInformation(String.Format("{0} = '{1}'", prop.Name, propValue));
            }
        }
    }
}
