using System;
using System.Data.SqlClient;
using System.Linq;

namespace Daenet.Common.Logging.Sql
{
    public class SqlServerBatchLogTask: SqlBatchLogTask
    {
        private string _connectionString;
        public SqlServerBatchLogTask(ISqlLoggerSettings settings) : base(settings)
        {
            _connectionString = settings.ConnectionString;
        }

        protected override void WriteToDb(Event[] listToWrite)
        {
            // Don't do anything if list is empty.
            if (CurrentList.Count == 0)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    //create object of SqlBulkCopy which help to insert  
                    using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
                    {
                        CustomDataReader customDataReader = new CustomDataReader(listToWrite);
                        objbulk.DestinationTableName = TableName;

                        foreach (var mapping in DbColumnMapping)
                        {
                            objbulk.ColumnMappings.Add(mapping);
                        }

                        con.Open();

                        if (BatchSize <= 1) // use sync method if BatchSize is < 1
                            objbulk.WriteToServer(customDataReader);
                        else // use async methodd
                            objbulk.WriteToServerAsync(customDataReader);
                    }
                }
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