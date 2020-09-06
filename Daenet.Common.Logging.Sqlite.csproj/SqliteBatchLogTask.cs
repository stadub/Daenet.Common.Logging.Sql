using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Daenet.Common.Logging.Sql;
using SQLite;

namespace Lesofto.Common.Logging.Sqlite
{
    public class SqliteBatchLogTask: SqlBatchLogTask
    {
        private string _connectionString;
        private SQLiteOpenFlags _openFlags;

        public SqliteBatchLogTask(ISqlLoggerSettings settings) : base(settings)
        {
            _connectionString = settings.ConnectionString;
            _openFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.Create;
        }

        private async Task Write(Event[] listToWrite)
        {
            var connection = new SQLiteAsyncConnection(_connectionString, _openFlags);
            
            await connection.CreateTableAsync<Event>();
            var table = connection.Table<Event>();

            await connection.InsertAllAsync(listToWrite);

            
        }

        protected override void WriteToDb(Event[] listToWrite)
        {
            // Don't do anything if list is empty.
            if (CurrentList.Count == 0)
                return;

            try
            {
                
            }
            catch (InvalidOperationException invalidEx)
            {
                if (invalidEx.Message == "The given ColumnMapping does not match up with any column in the source or destination.")
                {

                    var columns = String.Join(",", DbColumnMapping.Select(d => d.DestinationColumn));
                    HandleError(new Exception($"Missing/Invalid table columns. Required columns: {columns}", invalidEx));
                }
                else
                    HandleError(invalidEx);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
    }
}