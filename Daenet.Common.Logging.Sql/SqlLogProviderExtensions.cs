using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// Extensions for working work ILogger and other interfaces.
    /// </summary>
    public static class SqlLogProviderExtensions
    {

        private static string _sqlLoggerProviderCfgSectionName = (Attribute.GetCustomAttribute(typeof(SqlLogProvider), typeof (ProviderAliasAttribute)) as ProviderAliasAttribute).Alias;



        /// <summary>
        /// Gets settings from configuration.
        /// </summary>
        /// <param name="config">Configuration for SQL Server Logging.</param>
        /// <returns></returns>
        public static ISqlLoggerSettings GetSqlLoggerSettings(this IConfiguration config)
        {
            var settings = new SqlLoggerSettings();
            SetSqlLoggerSettings(settings, config);
            return settings;
        }

        /// <summary>
        /// Set settings from configuration.
        /// </summary>
        /// <param name="config">Configuration for SQL Server Logging.</param>
        /// <returns></returns>
        public static void SetSqlLoggerSettings(this SqlLoggerSettings settings, IConfiguration config)
        {
            if (settings == null)
                settings = new SqlLoggerSettings();

            var sqlServerSection = config.GetSection(_sqlLoggerProviderCfgSectionName);

            settings.ConnectionString = sqlServerSection.GetValue<string>("ConnectionString");

            if (String.IsNullOrEmpty(settings.ConnectionString))
                throw new ArgumentException("SqlProvider:ConnectionString is Null or Empty!", nameof(settings.ConnectionString));

            settings.IncludeExceptionStackTrace = sqlServerSection.GetValue<bool>("IncludeExceptionStackTrace");

            settings.TableName = sqlServerSection.GetValue<string>("TableName");

            if (String.IsNullOrEmpty(settings.TableName))
                throw new ArgumentException("SqlProvider:TableName is Null or Empty!", nameof(settings.TableName));

            settings.IgnoreLoggingErrors = sqlServerSection.GetValue<bool>("IgnoreLoggingErrors");
            settings.ScopeSeparator = sqlServerSection.GetValue<string>("ScopeSeparator");

            settings.BatchSize = sqlServerSection.GetValue<int>("BatchSize");
            settings.InsertTimerInSec = sqlServerSection.GetValue<int>("InsertTimerInSec");

            var columnsMapping = sqlServerSection.GetSection("ScopeColumnMapping");
            if (columnsMapping != null)
            {
                foreach (var item in columnsMapping.GetChildren())
                {
                    settings.ScopeColumnMapping.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                }
            }
        }
    }
}
