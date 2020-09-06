using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Lesofto.Common.Logging.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// Extensions for working work ILogger and other interfaces.
    /// </summary>
    public static class SqliteLogProviderExtensions
    {


        /// <summary>
        /// Adds a sql logger named 'SqlLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddSqliteLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ISqlBatchLogTask, SqliteBatchLogTask>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SqlLogProvider>());
            return builder;
        }

        /// <summary>
        /// Adds a sql logger named 'SqlLogger' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure"></param>
        public static ILoggingBuilder AddSqliteLogger(this ILoggingBuilder builder, Action<SqlLoggerSettings> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddSqliteLogger();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SqlLogProvider>());

            builder.Services.Configure(configure);

            return builder;
        }



        /// <summary>
        /// Adds Sql Logger to LoggerFactory
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory Instance</param>
        /// <param name="settings">Sql Logger Settings</param>
        /// <param name="filter">If specified it will override all defined switches.</param>  
        /// <returns></returns>        
        public static ILoggerFactory AddSqlServerLogger(this ILoggerFactory loggerFactory,
          ISqlLoggerSettings settings)
        {
            var logger = new SqliteBatchLogTask(settings);
            loggerFactory.AddProvider(new SqlLogProvider(logger, settings));

            return loggerFactory;
        }

        /// <summary>
        /// Adds a SQL Logger to the Logger factory.
        /// </summary>
        /// <param name="loggerFactory">The Logger factory instance.</param>
        /// <param name="config">The .NET Core Configuration for the logger section.</param>
        /// <returns></returns>
        public static ILoggerFactory AddSqlServerLogger(this ILoggerFactory loggerFactory, IConfiguration config)
        {
            var settings = config.GetSqlLoggerSettings();
            var logger = new SqliteBatchLogTask(settings);
            loggerFactory.AddProvider(new SqlLogProvider(logger, settings));

            return loggerFactory;
        }




    }
}
