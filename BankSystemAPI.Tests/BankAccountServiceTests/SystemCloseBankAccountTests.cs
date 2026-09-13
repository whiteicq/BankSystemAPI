using BusinessLogicLayer.Exceptions.BankAccount;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.BankAccountServiceTests
{
    public class SystemCloseBankAccountTests : BankAccountServiceTests
    {
        [Fact]
        public void SystemCloseBankAccount_ValidActiveAccountWithZeroBalance_ShouldCloseSuccessfully()
        {
            // Arrange
            // Create an active bank account with zero balance using your base helper
            BankAccount account = CreateTestBankAccount(id: 10L, moneyBalance: 0m, status: BankAccountStatus.Active);

            // Note: Your helper calls GenerateUniqueBankAccountNumber internally, 
            // so the account is already generated. Now we add it to the DB context.
            _context.BankAccounts.Add(account);
            _context.SaveChanges();

            string expectedOldStatus = BankAccountStatus.Active.ToString();
            string expectedNewStatus = BankAccountStatus.Closed.ToString();

            // Act
            _service.SystemCloseBankAccount(account.Id);

            // Assert
            // Verify state modifications on the entity object
            Assert.Equal(BankAccountStatus.Closed, account.Status);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), account.ClosedAt);

            // Verify that modifications were securely persisted inside the database
            BankAccount accountFromDb = _context.BankAccounts.Find(account.Id);
            Assert.NotNull(accountFromDb);
            Assert.Equal(BankAccountStatus.Closed, accountFromDb.Status);

            // Verify that the logging infrastructure recorded the system closure event after SaveChanges
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.BANK_ACCOUNT_CLOSED,
                nameof(BankAccount),
                account.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Fact]
        public void SystemCloseBankAccount_AccountDoesNotExist_ShouldThrowBankAccountNotFoundException()
        {
            // Act & Assert (Database is empty, searching for non-existent ID 999 must throw exception)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _service.SystemCloseBankAccount(bankAccountId: 999L)
            );

            // Verify execution was aborted before touching the logger mock
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
        public void SystemCloseBankAccount_AccountIsInactive_ShouldThrowInvalidBankAccountStatusException()
        {
            // Arrange
            // Create an inactive account (assuming BankAccountStatus.Closed causes LocalValidator.IsActive to return false)
            BankAccount inactiveAccount = CreateTestBankAccount(id: 10L, moneyBalance: 0m, status: BankAccountStatus.Closed);

            _context.BankAccounts.Add(inactiveAccount);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidBankAccountStatusException>(() =>
                _service.SystemCloseBankAccount(inactiveAccount.Id)
            );

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }

        [Theory]
        [InlineData(500)]   // Case 1: Account has positive balance (funds available)
        [InlineData(-150)]  // Case 2: Account has negative balance (debt available)
        public void SystemCloseBankAccount_WithNonZeroBalance_ShouldThrowArgumentOutOfRangeException(decimal invalidBalance)
        {
            // Arrange
            BankAccount account = CreateTestBankAccount(id: 10L, moneyBalance: invalidBalance, status: BankAccountStatus.Active);

            _context.BankAccounts.Add(account);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.SystemCloseBankAccount(account.Id)
            );

            // Verify no log entry was written
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
