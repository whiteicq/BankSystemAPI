
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankSystemAPI.Tests.TransactionServiceTests
{
    public class TransactionServiceTests
    {
        protected readonly TransactionService _service;
        protected readonly BankDbContext _context;
        protected readonly Mock<ILoggerService> _loggerMock;

        public TransactionServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new BankDbContext(options);
            _loggerMock = new Mock<ILoggerService>();
            _service = new TransactionService(_context, _loggerMock.Object);
        }

        protected Client CreateTestClient(long id = 1L, long userId = 1L, ClientStatus status = ClientStatus.Active)
        {
            return new Client
            {
                Id = id,
                UserId = userId,
                Status = status,
                Name = "Ivan",
                Surname = "Ivanov",
                PhoneNumber = "+375292281234",
                BankAccounts = new List<BankAccount>()
            };
        }

        protected BankAccount CreateTestBankAccount(long id = 1L, long? clientId = 1L, decimal balance = 1000m, string number = "1111", BankAccountType type = BankAccountType.Current, BankAccountStatus status = BankAccountStatus.Active)
        {
            return new BankAccount
            {
                Id = id,
                ClientId = clientId,
                MoneyBalance = balance,
                BankAccountNumber = number,
                Type = type,
                Status = status,
                BankId = 1L
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
