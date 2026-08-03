using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Logs;
using Microsoft.EntityFrameworkCore;


namespace BusinessLogicLayer.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly DbContext _context;

        public LoggerService(DbContext context)
        {
            _context = context;
        }

        public void MakeLog(OperationType operationType, string targetTable, long targetRowId, string? oldValue = null, string? newValue = null)
        {
            Log newLog = new Log
            {
                TypeOperation = operationType,
                TargetTable = targetTable,
                TargetRowId = targetRowId,
                OldValue = oldValue,
                NewValue = newValue
            };

            _context.Set<Log>().Add(newLog);
            _context.SaveChanges();
        }
    }
}
