using BusinessLogicLayer.Exceptions.BankAccount;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class BlockBankAccountTests : EmployeeServiceTests
    {
        [Fact]
        public void BlockBankAccount_ValidActiveAccount_ShouldBlockAndLog()
        {
            // Arrange
            // Create an active bank account that we want to block
            BankAccount account = CreateTestBankAccount(id: 10L, status: BankAccountStatus.Active);

            _context.BankAccounts.Add(account);
            _context.SaveChanges();

            string expectedOldStatus = BankAccountStatus.Active.ToString();
            string expectedNewStatus = BankAccountStatus.Blocked.ToString();

            // Act
            _employeeService.BlockBankAccount(account.Id);

            // Assert
            // Verify that the status inside the tracked entity object changed to Blocked
            Assert.Equal(BankAccountStatus.Blocked, account.Status);

            // Verify that the state was physically updated and saved inside the InMemory database
            BankAccount accountFromDb = _context.BankAccounts.Find(account.Id);
            Assert.NotNull(accountFromDb);
            Assert.Equal(BankAccountStatus.Blocked, accountFromDb.Status);

            // Verify that the logger was called exactly once with proper status strings
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.BANK_ACCOUNT_BLOCKED,
                nameof(BankAccount),
                account.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Fact]
        public void BlockBankAccount_AccountAlreadyBlocked_ShouldReturnEarlyWithoutChangesOrLogs()
        {
            // Arrange
            // Create a bank account that is already Blocked to trigger the early return statement
            BankAccount blockedAccount = CreateTestBankAccount(id: 10L, status: BankAccountStatus.Blocked);

            _context.BankAccounts.Add(blockedAccount);
            _context.SaveChanges();

            // Act
            _employeeService.BlockBankAccount(blockedAccount.Id);

            // Assert
            // Verify that the status securely remains Blocked
            Assert.Equal(BankAccountStatus.Blocked, blockedAccount.Status);

            // Verify that the logger was NEVER called due to the early return condition
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }

        [Fact]
        public void BlockBankAccount_AccountDoesNotExist_ShouldThrowBankAccountNotFoundException()
        {
            // Act & Assert (Database is empty, searching for ID 999 must throw the expected exception)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _employeeService.BlockBankAccount(bankAccountId: 999L)
            );

            // Verify that execution was aborted before touching the logging infrastructure
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }
    }
}
