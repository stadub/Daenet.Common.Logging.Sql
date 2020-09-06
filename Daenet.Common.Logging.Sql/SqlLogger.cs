using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace Daenet.Common.Logging.Sql
{
    public class SqlLogger : ILogger
    {
        private readonly ISqlBatchLogTask _logger;

        /// <summary>
        /// Set on true if the Logging fails and it is set on IgnoreLoggingErrors.
        /// </summary>
        private bool _isLoggingDisabledOnError = false;

        private readonly bool _ignoreLoggingErrors = false;
        private ISqlLoggerSettings _settings;
        private Func<string, LogLevel, bool> _filter;
        private string _categoryName;

//    public Func<LogLevel, EventId, object, Exception, SqlCommand> SqlCommandFormatter { get; set; }

        #region Public Methods

        public SqlLogger(ISqlBatchLogTask logger, ISqlLoggerSettings settings, string categoryName, Func<string, LogLevel, bool> filter = null)
        {
            _logger = logger;
            try
            {
                _settings = settings;

                if (_settings.ScopeSeparator == null)
                    _settings.ScopeSeparator = "=>";

                _categoryName = categoryName;
                if (filter == null)
                    _filter = ((category, logLevel) => true);
                else
                    _filter = filter;

                _ignoreLoggingErrors = settings.IgnoreLoggingErrors;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }


        /// <summary>
        /// Logs the message to SQL table.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="exceptionFormatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> exceptionFormatter)
        {
            if (!IsEnabled(logLevel))
                return;

            // TODO: Evaluate what do to with the formatted ecxeption.
            // Only needed if exception != NULL? Or use very time?
            //if (exceptionFormatter != null)
            //{
            //    var formattedException = exceptionFormatter(state, exception);
            //}

            _logger.Push(logLevel, eventId, state, exception, _categoryName);
        }


        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return SqlLoggerScope.Push("SqlLogger", state);
        }

        /// <summary>
        /// Checks if the logger is enabled for the specified log level and above 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            if (_isLoggingDisabledOnError)
                return false;

            return _filter(_categoryName, logLevel);
        }

        #endregion

        #region Private Methods

        //TODO: Please test ignore errors if logging fails. See IgnoreLoggingErrors.
        private void HandleError(Exception ex)
        {

            if (_ignoreLoggingErrors)
            {
                if (_isLoggingDisabledOnError == false)
                {
                    //TODO - Should log somewhere else later.
                    Debug.WriteLine($"Logging has failed and it will be disabled. Error: {ex}");
                }

                _isLoggingDisabledOnError = true;
            }
            else
                throw new Exception("Ignore Error is disabled.", ex);
        }
        #endregion
    }

}

