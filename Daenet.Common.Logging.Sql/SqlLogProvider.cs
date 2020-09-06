using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Daenet.Common.Logging.Sql
{
    [ProviderAlias("SqlProvider")]
    public class SqlLogProvider : ILoggerProvider
    {
        private readonly ISqlBatchLogTask _logger;
        private ISqlLoggerSettings _settings;
        private readonly ConcurrentDictionary<string, SqlLogger> _loggers = new ConcurrentDictionary<string, SqlLogger>();

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlLogProvider(ISqlBatchLogTask logger, ISqlLoggerSettings settings)
        {
            this._settings = settings;
            _logger = logger;
        }

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SqlLogProvider(ISqlBatchLogTask logger, IOptions<SqlLoggerSettings> settings) : this(logger, settings.Value)
        {
        }

        /// <summary>
        /// Create SQL Logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>Returns Logger</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private SqlLogger CreateLoggerImplementation(string categoryName)
        {
            return new SqlLogger(_logger, _settings, categoryName);
        }


        public void Dispose()
        {
        }
    }
}
