using System;
using Microsoft.Extensions.Logging;

namespace Daenet.Common.Logging.Sql
{
    public interface ISqlBatchLogTask
    {
        void Push<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string categoryName);
        void RunInsertTimer();
    }
}