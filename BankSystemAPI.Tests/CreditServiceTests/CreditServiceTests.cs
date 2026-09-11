using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankSystemAPI.Tests.CreditServiceTests
{
    public class CreditServiceTests : IDisposable
    {
        protected readonly CreditService _service;
        protected readonly BankDbContext _context;
        protected readonly Mock<ILoggerService> _loggerMock;
        protected readonly Mock<ITransactionService> _transactionServiceMock;
        protected readonly Mock<IBankAccountService> _bankAccountServiceMock;

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

        protected Credit CreateTestCredit(long id = 1L, long clientId = 1L, long bankId = 1L, decimal loanAmount = 5000m, decimal loanBalance = 6000m, CreditStatus status = CreditStatus.Unactivated)
        {
            return new Credit
            {
                Id = id,
                ClientId = clientId,
                BankId = bankId,
                LoanAmount = loanAmount,
                LoanBalance = loanBalance,
                LoanTerm = 12,
                LoanInterest = 15m,
                Status = status
            };
        }

        public CreditServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new BankDbContext(options);
            _loggerMock = new Mock<ILoggerService>();
            _transactionServiceMock = new Mock<ITransactionService>();
            _bankAccountServiceMock = new Mock<IBankAccountService>();
            _service = new CreditService(_context, _transactionServiceMock.Object, _bankAccountServiceMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

    }
}
