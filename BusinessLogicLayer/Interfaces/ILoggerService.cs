using DataAccessLayer.Enums.Logs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface ILoggerService
    {
        void MakeLog(OperationType operationType, string targetTable, long targetRowId, string? oldValue = null, string? newValue = null);
    }
}
