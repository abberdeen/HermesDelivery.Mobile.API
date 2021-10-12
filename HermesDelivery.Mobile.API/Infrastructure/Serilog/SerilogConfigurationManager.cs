using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog;
using Serilog.Core;

namespace CourierAPI.Infrastructure.Serilog
{
    /// <summary>
    /// 
    /// </summary>
    public class SerilogConfigurationManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ILogger CreateLogger()
        {
            var dataDirectoryPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            return  new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dataDirectoryPath + "/Logs/Log-{Date}.txt", retainedFileCountLimit: null)
                .CreateLogger();
        }

        public static ILogger CreateWebSocketLogger()
        {
            var dataDirectoryPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            return new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dataDirectoryPath + "/Logs/Log-WebSocket-{Date}.txt", retainedFileCountLimit: null)
                .CreateLogger();
        }
    }
}