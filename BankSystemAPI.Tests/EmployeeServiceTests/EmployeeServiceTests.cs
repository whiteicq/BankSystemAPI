using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using DataAccessLayer.Enums.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class EmployeeServiceTests : IDisposable
    {
        protected readonly BankDbContext _context;
        protected readonly Mock<IBankAccountService> _bankAccountServiceMock;
        protected readonly Mock<ICreditService> _creditServiceMock;
        protected readonly Mock<IDepositService> _depositServiceMock;
        protected readonly Mock<ILoggerService> _loggerMock;
        protected readonly EmployeeService _employeeService;

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

        protected Transaction CreateTestTransaction(long id = 1L, BankAccount sender = null, BankAccount receiver = null, decimal amount = 500m, TransactionStatus status = TransactionStatus.Confirmed)
        {
            return new Transaction
            {
                Id = id,
                Sender = sender,
                Receiver = receiver,
                TransactionAmount = amount,
                Status = status,
                Type = TransactionType.PeerToPeer
            };
        }

        public EmployeeServiceTests()
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new BankDbContext(options);

            _bankAccountServiceMock = new Mock<IBankAccountService>();
            _creditServiceMock = new Mock<ICreditService>();
            _depositServiceMock = new Mock<IDepositService>();
            _loggerMock = new Mock<ILoggerService>();

            _employeeService = new EmployeeService(_context, _bankAccountServiceMock.Object, _creditServiceMock.Object, _depositServiceMock.Object, _loggerMock.Object);
                
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
