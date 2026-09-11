using BusinessLogicLayer.Exceptions.Bank;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.BankAccountServiceTests
{
    public class OpenBankAccountTests : BankAccountServiceTests
    {
        [Fact]
        public void OpenBankAccount_ValidData_ShouldCreateAndReturnBankAccount()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient();

            _context.Banks.Add(bank);
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            BankAccount result = _service.OpenBankAccount(client.UserId, bank.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(28, result.BankAccountNumber.Length);
            Assert.Equal(client.Id, result.ClientId);
            Assert.Equal(bank.Id, result.BankId);
            Assert.Contains(result, _context.BankAccounts);

            _loggerMock.Verify(m => m.MakeLog(
                OperationType.BANK_ACCOUNT_OPENED,
                nameof(BankAccount),
                result.Id,
                null,
                BankAccountStatus.Unactivated.ToString()),
                Times.Once);
        }

        [Fact]
        public void OpenBankAccount_BankIsNull_ShouldThrowException()
        {
            // Arrange
            Client client = CreateTestClient();
            Bank bank = CreateTestBank();

            _context.Add(client);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<BankNotFoundException>(() => _service.OpenBankAccount(client.UserId, bank.Id));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void OpenBankAccount_ClientIsUnactive_ShouldThrowException()
        {
            // Arrange
            Client client = CreateTestClient(status: ClientStatus.Unactive);
            Bank bank = CreateTestBank();

            _context.Add(client);
            _context.Add(bank);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<InvalidClientStatusException>(() => _service.OpenBankAccount(client.UserId, bank.Id));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }
    }
}
