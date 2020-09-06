using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Daenet.Common.Logging.Sql.Test
{
    public class SqlServerLoggerSwitchTests
    {
        private ILogger _logger;

        public SqlServerLoggerSwitchTests()
        {
            //this.getLogger(null, @"SqlServerLoggerSwitchSettings.json");
        }


        [Fact]
        public void TestFullName()
        {
            ILogger logger = this.GetLogger( "SqlLoggerSettings.json");
            using (logger.BeginScope(Guid.NewGuid()))
            {
                logger.LogTrace(123, "Test Trace Message");
            }
        }

        /// <summary>
        /// Initializes the logger
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="settingsFile"></param>
        private ILogger GetLogger(string settingsFile)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(settingsFile);
            var configRoot = builder.Build();

            ILoggerFactory loggerFactory = new LoggerFactory().AddSqlServerLogger(configRoot.GetSqlLoggerSettings());
            ILogger logger = loggerFactory.CreateLogger<SqlServerLoggerSwitchTests>();
            return logger;
        }
    }
}
