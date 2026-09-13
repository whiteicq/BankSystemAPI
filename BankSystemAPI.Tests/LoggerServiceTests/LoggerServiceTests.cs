using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Logs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.LoggerServiceTests
{
    public class LoggerServiceTests
    {
        protected readonly BankDbContext _context;
        protected readonly LoggerService _loggerService;

        public LoggerServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BankDbContext(options);

            _loggerService = new LoggerService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    public class MakeLogTests : LoggerServiceTests
    {
        [Fact]
        public void MakeLog_WithAllParameters_ShouldSuccessfullySaveLogToDatabase()
        {
            // Arrange
            OperationType operationType = OperationType.BANK_ACCOUNT_CLOSED;
            string targetTable = nameof(BankAccount);
            long targetRowId = 42L;
            string oldStatus = "Active";
            string newStatus = "Closed";

            // Act
            _loggerService.MakeLog(operationType, targetTable, targetRowId, oldStatus, newStatus);

            // Assert
            // Verify that there is exactly ONE record in the Logs database table
            Log logFromDb = Assert.Single(_context.Logs);

            // Verify that all properties inside the database match the passed arguments perfectly
            Assert.Equal(operationType, logFromDb.TypeOperation);
            Assert.Equal(targetTable, logFromDb.TargetTable);
            Assert.Equal(targetRowId, logFromDb.TargetRowId);
            Assert.Equal(oldStatus, logFromDb.OldValue);
            Assert.Equal(newStatus, logFromDb.NewValue);
        }

        [Fact]
        public void MakeLog_WithOptionalParametersAsNull_ShouldSaveLogWithNullValues()
        {
            // Arrange
            OperationType operationType = OperationType.CREDIT_REQUESTED;
            string targetTable = nameof(Credit);
            long targetRowId = 100L;

            // Act - passing only mandatory arguments, leaving optional ones as null
            _loggerService.MakeLog(operationType, targetTable, targetRowId, oldValue: null, newValue: null);

            // Assert
            Log logFromDb = Assert.Single(_context.Logs);

            Assert.Equal(operationType, logFromDb.TypeOperation);
            Assert.Equal(targetTable, logFromDb.TargetTable);
            Assert.Equal(targetRowId, logFromDb.TargetRowId);

            // Verify that optional fields securely remained null in the database
            Assert.Null(logFromDb.OldValue);
            Assert.Null(logFromDb.NewValue);
        }
    }
}
