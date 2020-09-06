using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Daenet.Common.Logging.Sql.Test
{
    //TODO: Please provide tests for combination of namespaces. Use differnet json setting files.
    /*
     Microsoft.Extensions.Logging.EventHub.Test.EventHubLoggerTests
     EventHubLoggerTests
     EventHub.Test.EventHubLoggerTests
     Default
     */
     // TODO: Scope Tests required. See EventHubLogger
    public class SqlServerLoggerTests
    {
#pragma warning disable SA1308 // Variable names must not be prefixed
        private ILogger _logger;
#pragma warning restore SA1308 // Variable names must not be prefixed

        /// <summary>
        /// Initializes default logger.
        /// </summary>
        public SqlServerLoggerTests()
        {
            InitializeSqlServerLogger();
        }


        /// <summary>
        /// TODO: Describe what does this test does.
        /// </summary>
        [Fact]
        public void SqlLoggingTest()
        {
            this._logger.LogTrace("Test Trace Messages");
            this._logger.LogDebug("Test Debug Messages");
            this._logger.LogInformation("Test Information Messages");
            this._logger.LogWarning("Test Warning Messages");
            this._logger.LogError(new EventId(123, "Error123"), new Exception("new exception", new ArgumentNullException()), "Test Error Message");
            this._logger.LogCritical(new EventId(123, "Critical123"), new Exception("new exception", new ArgumentNullException()), "Test Critical Message");
        }


        /// <summary>
        /// Testing scope of the logger
        /// </summary>
        [Fact]
        public void SqlLoggingScopeTest()
        {
            using (this._logger.BeginScope($"Scope1"))
            {
                this.SqlLoggingTest();

                using (this._logger.BeginScope($"Scope2"))
                {
                    this.SqlLoggingTest();

                    using (this._logger.BeginScope($"Scope3"))
                    {
                        this.SqlLoggingTest();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the logger
        /// </summary>
        /// <param name="filter">Filter used for logging.</param>
        private void InitializeSqlServerLogger()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(@"SqlLoggerSettings.json");
            var configRoot = builder.Build();
            ILoggerFactory loggerFactory = new LoggerFactory().AddSqlServerLogger(configRoot.GetSqlLoggerSettings());
            this._logger = loggerFactory.CreateLogger<SqlServerLoggerTests>();
        }
    }
}
