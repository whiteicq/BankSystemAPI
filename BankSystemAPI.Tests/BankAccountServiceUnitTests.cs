using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Logs;
using Microsoft.EntityFrameworkCore;
using BusinessLogicLayer.Exceptions.Client;
using Moq;
using SQLitePCL;
using Xunit.Sdk;
using System.Diagnostics;

namespace BankSystemAPI.Tests
{
    public class BankAccountServiceUnitTests : IDisposable
    {
        private readonly BankAccountService _service;
        private readonly BankDbContext _context;
        private readonly Mock<ILoggerService> _loggerMock;

        public BankAccountServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BankDbContext(options);
            _loggerMock = new Mock<ILoggerService>();
            _service = new BankAccountService(_context, _loggerMock.Object);
        }

        private Bank CreateTestBank(long id= 1L, string bic = "PJCBBY2X", string address = "г. Минск, ул. В.Хоружей 31А", string title = "Приорбанк")
        {
            return new Bank
            {
                Id = id,
                BIC = bic,
                Address = address,
                Title = title
            };
        }

        private Client CreateTestClient(long id = 1L, long userId = 1L, ClientStatus status = ClientStatus.Active, string name = "Ivan", string surname = "Ivanov", string phonenumber = "+375292281234")
        {
            return new Client
            {
                Id = id, 
                UserId = userId,
                Status = status,
                Name = name,
                Surname = surname, 
                PhoneNumber = phonenumber
            };
        }

        private BankAccount CreateTestBankAccount(long clientId = 1L, long bankId = 1L, decimal moneyBalance = 1000m, BankAccountType type = BankAccountType.Current, BankAccountStatus status = BankAccountStatus.Active)
        {
            return new BankAccount
            {
                MoneyBalance = moneyBalance,
                ClientId = clientId,
                BankId = bankId,
                BankAccountNumber = _service.GenerateUniqueBankAccountNumber(28),
                Type = type,
                Status = status
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

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
            Assert.Throws<InvalidDataException>(() => _service.OpenBankAccount(client.UserId, bank.Id));

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
            Assert.Throws<InvalidStatus>(() => _service.OpenBankAccount(client.UserId, bank.Id));

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(28)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(100)]
        public void GenerateUniqueBankAccountNumber_ValidData_ShouldGenerateAndReturn(int length)
        {
            // Act
            string result = _service.GenerateUniqueBankAccountNumber(length);

            // Assert
            Assert.Equal(length, result.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(-25)]
        [InlineData(-100)]
        [InlineData(-13)]
        public void GenerateUniqueBankAccountNumber_LengthLessOrEqualThanZero_ShouldGenerateAndReturn(int length)
        {
            // Act
            string result = _service.GenerateUniqueBankAccountNumber(length);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CloseBankAccount_ValidData_ShouldCloseBankAccount()
        {

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
            Assert.Throws<ClientNotFound>(() => _service.CloseBankAccount(1, bankAccount.Id));
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
            Assert.Throws<InvalidOperationException>(() => _service.CloseBankAccount(client.UserId, 1));
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
            Assert.Throws<InvalidOperationException>(() => _service.CloseBankAccount(client.UserId, bankAccount.Id));
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

        }

        [Fact]
        public void CloseBankAccount_MoneyBalanceOutOfRange_ShouldThrowException()
        {

        }
    }
}
