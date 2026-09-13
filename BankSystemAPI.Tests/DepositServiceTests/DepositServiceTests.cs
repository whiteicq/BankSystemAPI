using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankSystemAPI.Tests.DepositServiceTests
{
    public class DepositServiceTests : IDisposable
    {
        protected readonly BankDbContext _context;
        protected readonly Mock<IBankAccountService> _bankAccountServiceMock;
        protected readonly Mock<ITransactionService> _transactionServiceMock;
        protected readonly Mock<ILoggerService> _loggerMock;
        protected readonly DepositService _depositService;

        protected Bank CreateTestBank(long id = 1L, string bic = "PJCBBY2X", string address = "г. Минск, ул. В.Хоружей 31А", string title = "Приорбанк")
        {
            return new Bank
            {
                Id = id,
                BIC = bic,
                Address = address,
                Title = title
            };
        }

        protected BankAccount CreateTestBankAccount(long id = 1L, long? clientId = 1L, long bankId = 1L, decimal moneyBalance = 1000m, BankAccountType type = BankAccountType.Current, BankAccountStatus status = BankAccountStatus.Active)
        {
            return new BankAccount
            {
                Id = id,
                MoneyBalance = moneyBalance,
                ClientId = clientId,
                BankId = bankId,
                BankAccountNumber = "1234567890123456789012345678",
                Type = type,
                Status = status
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
                PhoneNumber = phonenumber,
                BankAccounts = new List<BankAccount>(),
                Credits = new List<Credit>()
            };
        }

        protected Deposit CreateTestDeposit(long id = 1L, long clientId = 1L, long bankId = 1L, decimal depositAmount = 5000m, int depositTerm = 12, decimal depositInterest = 7.5m, DepositStatus status = DepositStatus.Active)
        {
            return new Deposit
            {
                Id = id,
                ClientId = clientId,
                BankId = bankId,
                DepositAmount = depositAmount,
                DepositTerm = depositTerm,
                DepositInterest = depositInterest,
                Status = status
            };
        }

        public DepositServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new BankDbContext(options);
            _loggerMock = new Mock<ILoggerService>();
            _bankAccountServiceMock = new Mock<IBankAccountService>();
            _transactionServiceMock = new Mock<ITransactionService>();
            _depositService = new DepositService(_context, _bankAccountServiceMock.Object, _transactionServiceMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
