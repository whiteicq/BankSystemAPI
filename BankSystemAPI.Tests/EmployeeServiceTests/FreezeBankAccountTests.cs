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
    public class FreezeBankAccountTests : EmployeeServiceTests
    {
        [Fact]
        public void FreezeBankAccount_ValidActiveAccount_ShouldFreezeAndLog()
        {
            // Arrange
            // Create an active bank account that we want to freeze
            // (Assuming BankAccountStatus.Active causes LocalValidator.IsActive to return true)
            BankAccount account = CreateTestBankAccount(id: 10L, status: BankAccountStatus.Active);

            _context.BankAccounts.Add(account);
            _context.SaveChanges();

            string expectedOldStatus = BankAccountStatus.Active.ToString();
            string expectedNewStatus = BankAccountStatus.Frozen.ToString();

            // Act
            _employeeService.FreezeBankAccount(account.Id);

            // Assert
            // Verify that the status inside the tracked entity object changed to Frozen
            Assert.Equal(BankAccountStatus.Frozen, account.Status);

            // Verify that the state was physically updated and saved inside the InMemory database
            BankAccount accountFromDb = _context.BankAccounts.Find(account.Id);
            Assert.NotNull(accountFromDb);
            Assert.Equal(BankAccountStatus.Frozen, accountFromDb.Status);

            // Verify that the logger was invoked exactly once with proper status strings after SaveChanges
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.BANK_ACCOUNT_FROZEN,
                nameof(BankAccount),
                account.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Theory]
        [InlineData(BankAccountStatus.Frozen)] // Case 1: Account is already frozen
        [InlineData(BankAccountStatus.Closed)] // Case 2: Account is permanently closed
        public void FreezeBankAccount_AccountIsAlreadyInactive_ShouldReturnEarlyWithoutChanges(BankAccountStatus inactiveStatus)
        {
            // Arrange
            // Create a bank account with an inactive status so that LocalValidator.IsActive returns false
            BankAccount inactiveAccount = CreateTestBankAccount(id: 10L, status: inactiveStatus);

            _context.BankAccounts.Add(inactiveAccount);
            _context.SaveChanges();

            // Act
            _employeeService.FreezeBankAccount(inactiveAccount.Id);

            // Assert
            // Verify that the status securely remains completely untouched
            Assert.Equal(inactiveStatus, inactiveAccount.Status);

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
        public void FreezeBankAccount_AccountDoesNotExist_ShouldThrowBankAccountNotFoundException()
        {
            // Act & Assert (Database is empty, searching for ID 999 must throw the expected exception)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _employeeService.FreezeBankAccount(bankAccountId: 999L)
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
