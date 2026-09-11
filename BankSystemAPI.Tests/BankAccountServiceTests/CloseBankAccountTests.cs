using BusinessLogicLayer.Exceptions.BankAccount;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Logs;
using Moq;

namespace BankSystemAPI.Tests.BankAccountServiceTests
{
    public class CloseBankAccountTests : BankAccountServiceTests
    {
        [Fact]
        public void CloseBankAccount_ValidData_ShouldCloseBankAccount()
        {
            // Arrange
            BankAccount bankAccount = CreateTestBankAccount(moneyBalance: 0m);
            Client client = CreateTestClient();
            client.BankAccounts.Add(bankAccount);

            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            _service.CloseBankAccount(client.UserId, bankAccount.Id);

            // Assert
            Assert.Equal(BankAccountStatus.Closed, bankAccount.Status);

            _loggerMock.Verify(m => m.MakeLog(
                OperationType.BANK_ACCOUNT_CLOSED,
                nameof(BankAccount),
                bankAccount.Id, 
                BankAccountStatus.Active.ToString(), 
                bankAccount.Status.ToString()
                ),
                Times.Once);
        }

        [Fact]
        public void CloseBankAccount_ClientIsNotExists_ShouldThrowException()
        {
            // Arrange
            BankAccount bankAccount = CreateTestBankAccount();

            _context.BankAccounts.Add(bankAccount);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<ClientNotFoundException>(() => _service.CloseBankAccount(1, bankAccount.Id));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CloseBankAccount_BankAccountIsNotExists_ShouldThrowException()
        {
            // Arrange
            Client client = CreateTestClient();
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<BankAccountNotFoundException>(() => _service.CloseBankAccount(client.UserId, 1));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CloseBankAccount_BankAccountIsNotActive_ShouldThrowException()
        {
            Client client = CreateTestClient();
            BankAccount bankAccount = CreateTestBankAccount(status: BankAccountStatus.Unactivated);
            client.BankAccounts.Add(bankAccount);
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<InvalidBankAccountStatusException>(() => _service.CloseBankAccount(client.UserId, bankAccount.Id));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CloseBankAccount_ClientIsNotActive_ShouldThrowException()
        {
            // Arrange
            Client client = CreateTestClient(status: ClientStatus.Unactive);
            BankAccount bankAccount = CreateTestBankAccount();
            client.BankAccounts.Add(bankAccount);

            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<InvalidClientStatusException>(() => _service.CloseBankAccount(client.UserId, bankAccount.Id));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1500)]
        [InlineData(24145)]
        [InlineData(-100)]
        [InlineData(-999)]
        [InlineData(-1)]
        public void CloseBankAccount_MoneyBalanceNotZero_ShouldThrowException(decimal balance)
        {
            // Arrange
            Client client = CreateTestClient();
            BankAccount bankAccount = CreateTestBankAccount(moneyBalance: balance);
            client.BankAccounts.Add(bankAccount);

            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.CloseBankAccount(client.UserId, bankAccount.Id));

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
