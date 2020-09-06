using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.Common.Logging.Sql
{

    public abstract class SqlBatchLogTask : ISqlBatchLogTask
    {
        private ISqlLoggerSettings _settings;
        private Task _insertTimerTask;


        public SqlBatchLogTask(ISqlLoggerSettings settings)
        {
            _settings = settings;
            BatchSize = Convert.ToInt32(_settings.BatchSize);

            InitColumnMapping();

            TableName = _settings.TableName;
        }



        // settings
        public int BatchSize { get; }

        public string TableName { get; }

        //

        public List<SqlBulkCopyColumnMapping> DbColumnMapping { get; private set; }
        public BlockingCollection<Event> CurrentList { get; protected set; } = new BlockingCollection<Event>();


        private void InitColumnMapping()
        {
            DbColumnMapping = new List<SqlBulkCopyColumnMapping>();

            var mapping = Event.PropertiesMapping;
            foreach (var map in mapping)
            {
                DbColumnMapping.Add(new SqlBulkCopyColumnMapping(map.Value, map.Key));

            }

            int actualColumn = mapping.Count;

            for (var index = 0; index < _settings.ScopeColumnMapping.Count; index++, actualColumn++)
            {
                var dynamicColumnMapping = _settings.ScopeColumnMapping[index];
                DbColumnMapping.Add(new SqlBulkCopyColumnMapping(actualColumn.ToString(), dynamicColumnMapping.Value));
            }
        }



        public void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string categoryName)
        {
            object[] scopeValues = SqlLoggerScope.Current != null ? 
                SqlLoggerScope.Current.GetScopeInformation(_settings) : 
                new string[_settings.ScopeColumnMapping.Count()];


            var @event = new Event
            {
                EventId = eventId.Id,
                Type = Enum.GetName(typeof(LogLevel), logLevel),
                Message = state.ToString(),
                Timestamp = DateTime.UtcNow,
                CategoryName = categoryName,
                Exception = exception?.ToString()
            };

            @event.DynamicProperties.AddRange(scopeValues);

           
            CurrentList.Add(@event);


            if (CurrentList.Count >= BatchSize)
            {
                var listToWrite = CurrentList.ToArray();
                CurrentList = new BlockingCollection<Event>();
                WriteToDb(listToWrite);

            }
                
        }


        protected abstract void WriteToDb(Event[] listToWrite);

        protected void HandleError(Exception ex)
        {
            if (_settings.IgnoreLoggingErrors && BatchSize <= 1)
            {
                Debug.WriteLine($"Logging has failed. {ex}");
            }
            else
            {
                Debug.WriteLine($"Ignore Error is disabled and an Exception occured: {ex}");
                throw new Exception("Ignore Error is disabled and an Exception occured.", ex);
            }
        }

        public void RunInsertTimer()
        {
            if (_settings.InsertTimerInSec == 0 || _settings.BatchSize == 0)
                return;

            _insertTimerTask = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(_settings.InsertTimerInSec));

                    var listToWrite = CurrentList.ToArray();
                    CurrentList = new BlockingCollection<Event>();
                    WriteToDb(listToWrite); // Just assign it.
                }
            });

            _insertTimerTask.Start();
        }
    }

}
