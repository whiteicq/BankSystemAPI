using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using Microsoft.EntityFrameworkCore;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Transaction;
using BusinessLogicLayer.Infrastructure;
using DataAccessLayer.Enums.Logs;

namespace BusinessLogicLayer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly DbContext _context;
        private readonly IBankAccountService _bankAccountService;
        private readonly ICreditService _creditService;
        private readonly IDepositService _depositService;
        private readonly ILoggerService _loggerService;

        public EmployeeService(DbContext context, IBankAccountService bankAccountService, ICreditService creditService, IDepositService depositService, ILoggerService loggerService)
        {
            _context = context;
            _bankAccountService = bankAccountService;
            _creditService = creditService;
            _depositService = depositService;
            _loggerService = loggerService;
        }

        public void ActivateClient(long clientId)
        {
            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");
            if (LocalValidator.IsActive(client))
            {
                return;
            }

            string oldStatus = client.Status.ToString();
            client.Status = ClientStatus.Active;

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.CLIENT_ACTIVATED, nameof(Client), clientId, oldStatus, client.Status.ToString());

        }

        public void ActivateBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            if (LocalValidator.IsActive(bankAccount))
            {
                return;
            }

            string oldStatus = bankAccount.Status.ToString();
            bankAccount.Status = BankAccountStatus.Active;

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_ACTIVATED, nameof(BankAccount), bankAccountId, oldStatus, bankAccount.Status.ToString());

        }

        public void ActivateCredit(long clientId, long creditId, long bankAccountRecieverId)
        { 
            _creditService.TransferMoneyForLoan(clientId, creditId, bankAccountRecieverId);

            _loggerService.MakeLog(OperationType.CREDIT_APPROVED, nameof(Credit), creditId);
        }

        public void ActivateDeposit(long clientId, long depositId, long bankAccountSenderId)
        {
            _depositService.TransferMoneyForDeposit(clientId, depositId, bankAccountSenderId);

            _loggerService.MakeLog(OperationType.DEPOSIT_APPROVED, nameof(Deposit), depositId);
        }

        public void BlockBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            if (bankAccount.Status == BankAccountStatus.Blocked)
            {
                return;
            }

            string oldStatus = bankAccount.Status.ToString();
            bankAccount.Status = BankAccountStatus.Blocked;

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_BLOCKED, nameof(BankAccount), bankAccountId, oldStatus, bankAccount.Status.ToString());
        }

        public void BlockClient(long clientId)
        {
            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");
            if (client.Status == ClientStatus.Blocked)
            {
                return;
            }
            string oldStatus = client.Status.ToString();
            client.Status = ClientStatus.Blocked;

            _loggerService.MakeLog(OperationType.CLIENT_BLOCKED, nameof(Client), clientId, oldStatus, client.Status.ToString());

            _context.SaveChanges();
        }

        public void CancelTransaction(long transactionId)
        {
            Transaction transaction = _context.Set<Transaction>()
                .Include(tr => tr.Sender)
                .Include(tr => tr.Receiver)
                .FirstOrDefault(tr => tr.Id == transactionId 
                && tr.Status == TransactionStatus.Confirmed) ?? throw new KeyNotFoundException();
           
            BankAccount sender = transaction.Sender;
            BankAccount receiver = transaction.Receiver;
            decimal transactionAmount = transaction.TransactionAmount;

            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    receiver.MoneyBalance -= transactionAmount;
                    sender.MoneyBalance += transactionAmount;
                    transaction.Status = TransactionStatus.Canceled;

                    _context.SaveChanges();

                    _loggerService.MakeLog(OperationType.TRANSACTION_CANCELED, nameof(Transaction), transactionId, TransactionStatus.Confirmed.ToString(), transaction.Status.ToString());

                    _transaction.Commit();
                }
                catch
                {
                    _transaction.Rollback();
                    throw;
                }
            }
        }

        public void CloseBankAccount(long bankAccountId)
        {
            _bankAccountService.SystemCloseBankAccount(bankAccountId);
        }

        public void FreezeBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(bankAccount))
            {
                return;
            }

            string oldStatus = bankAccount.Status.ToString();
            bankAccount.Status = BankAccountStatus.Frozen;

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_FROZEN, nameof(BankAccount), bankAccountId, oldStatus, bankAccount.Status.ToString());
        }

        public void RejectCredit(long creditId)
        {
            Credit credit = _context.Set<Credit>().FirstOrDefault(cr => cr.Id == creditId && cr.Status == CreditStatus.Active) ?? throw new KeyNotFoundException();
            credit.Status = CreditStatus.Rejected;

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.CREDIT_REJECTED, nameof(Credit), creditId, CreditStatus.Active.ToString(), CreditStatus.Rejected.ToString());
        }

        public void RejectDeposit(long depositId)
        {
            Deposit deposit = _context.Set<Deposit>().FirstOrDefault(dp => dp.Id == depositId && dp.Status == DepositStatus.Active) ?? throw new KeyNotFoundException();
            deposit.Status = DepositStatus.Rejected;

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.DEPOSIT_REJECTED, nameof(Deposit), depositId, DepositStatus.Active.ToString(), DepositStatus.Rejected.ToString());
        } 
    }
}
