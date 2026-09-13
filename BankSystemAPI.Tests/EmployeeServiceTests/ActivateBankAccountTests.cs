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
    public class ActivateBankAccountTests : EmployeeServiceTests
    {
        [Theory]
        [InlineData(BankAccountStatus.Unactivated)] // Case 1: Activating an unactivated account
        [InlineData(BankAccountStatus.Frozen)]      // Case 2: Activating a frozen account
        public void ActivateBankAccount_ValidStatus_ShouldActivateAndLog(BankAccountStatus initialStatus)
        {
            // Arrange
            BankAccount account = CreateTestBankAccount(id: 10L, status: initialStatus);

            _context.BankAccounts.Add(account);
            _context.SaveChanges();

            string expectedOldStatus = initialStatus.ToString();
            string expectedNewStatus = BankAccountStatus.Active.ToString();

            // Act
            _employeeService.ActivateBankAccount(account.Id);

            // Assert
            // Verify that the status changed to Active (This passes now thanks to the '&&' fix!)
            Assert.Equal(BankAccountStatus.Active, account.Status);

            // Verify that changes are persisted in the InMemory database
            BankAccount accountFromDb = _context.BankAccounts.Find(account.Id);
            Assert.NotNull(accountFromDb);
            Assert.Equal(BankAccountStatus.Active, accountFromDb.Status);

            // Verify that the logger was invoked correctly
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.BANK_ACCOUNT_ACTIVATED,
                nameof(BankAccount),
                account.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Theory]
        [InlineData(BankAccountStatus.Active)] // Case 1: Account is already active
        [InlineData(BankAccountStatus.Closed)] // Case 2: Account is permanently closed
        public void ActivateBankAccount_InvalidStatus_ShouldReturnEarlyWithoutChanges(BankAccountStatus nonActivatableStatus)
        {
            // Arrange
            BankAccount account = CreateTestBankAccount(id: 10L, status: nonActivatableStatus);

            _context.BankAccounts.Add(account);
            _context.SaveChanges();

            // Act
            _employeeService.ActivateBankAccount(account.Id);

            // Assert
            // Verify that the status remains completely unchanged
            Assert.Equal(nonActivatableStatus, account.Status);

            // Verify that the logger was never triggered due to the early return statement
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
        public void ActivateBankAccount_AccountDoesNotExist_ShouldThrowBankAccountNotFoundException()
        {
            // Act & Assert (Database is empty, searching for ID 999 must throw the correct exception)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _employeeService.ActivateBankAccount(bankAccountId: 999L)
            );

            // Verify that execution aborted before hitting the log method
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
