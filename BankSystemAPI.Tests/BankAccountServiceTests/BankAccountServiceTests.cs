using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BankSystemAPI.Tests.BankAccountServiceTests
{
    public class BankAccountServiceTests : IDisposable
    {
        protected readonly BankAccountService _service;
        protected readonly BankDbContext _context;
        protected readonly Mock<ILoggerService> _loggerMock;

        public BankAccountServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BankDbContext(options);
            _loggerMock = new Mock<ILoggerService>();
            _service = new BankAccountService(_context, _loggerMock.Object);
        }

        protected Bank CreateTestBank(long id= 1L, string bic = "PJCBBY2X", string address = "г. Минск, ул. В.Хоружей 31А", string title = "Приорбанк")
        {
            return new Bank
            {
                Id = id,
                BIC = bic,
                Address = address,
                Title = title
            };
        }

        protected Client CreateTestClient(long id = 1L, long userId = 1L, ClientStatus status = ClientStatus.Active, string name = "Ivan", string surname = "Ivanov", string phonenumber = "+375292281234")
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

        protected BankAccount CreateTestBankAccount(long id = 1L, long clientId = 1L, long bankId = 1L, decimal moneyBalance = 1000m, BankAccountType type = BankAccountType.Current, BankAccountStatus status = BankAccountStatus.Active)
        {
            return new BankAccount
            {
                Id = id,
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
    }
}
