using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Enums.Transaction;
using DataAccessLayer.Enums.Logs;

namespace BusinessLogicLayer.Services
{
    public class DepositService : IDepositService
    {
        private readonly DbContext _context;
        private readonly IBankAccountService _bankAccountService;
        private readonly ITransactionService _transactionService;
        private readonly ILoggerService _loggerService;
        public DepositService(DbContext context, IBankAccountService bankAccountService, ITransactionService transactionService, ILoggerService loggerService)
        {
            _context = context;
            _bankAccountService = bankAccountService;
            _transactionService = transactionService;
            _loggerService = loggerService;
        }

        public Deposit RequestDeposit(long userId, long bankId, decimal sumOfDeposit, int term, decimal interest)
        {
            bool bankExists = _context.Set<Bank>().Any(b => b.Id == bankId);
            if (!bankExists)
            {
                throw new InvalidDataException();
            }

            if (sumOfDeposit <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (term <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (interest <= 5 || interest >= 11)
            {
                throw new ArgumentOutOfRangeException();
            }

            Client client = _context.Set<Client>().FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            Deposit deposit = new Deposit
            {
                DepositAmount = sumOfDeposit,
                DepositTerm = term,
                DepositInterest = interest,
                Client = client,
                BankId = bankId
            };

            _context.Set<Deposit>().Add(deposit);
            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.DEPOSIT_REQUESTED, nameof(Deposit), deposit.Id, newValue: deposit.Status.ToString());

            return deposit;
        }

        private decimal CalculateMonthlyPayment(decimal moneyBalance, decimal interest)
        {
            decimal sum = moneyBalance * (interest / 12m / 100m);
             
            return sum;
        }

        private BankAccount GetMasterBankAccount(Deposit currentDeposit)
        {
            BankAccount masterBankAccount = _context.Set<BankAccount>().FirstOrDefault(ba => ba.BankId == currentDeposit.BankId && ba.ClientId == null) ?? throw new KeyNotFoundException();

            return masterBankAccount;
        }

        // открытие депозитного счета после одобрения
        private BankAccount OpenDepositBankAccount(long clientId, long depositId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.Deposits).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            Deposit currentDeposit = client.Deposits.FirstOrDefault(cr => cr.Id == depositId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(currentDeposit))
            {
                throw new InvalidOperationException();
            }

            BankAccount depositBankAccount = new BankAccount
            {
                BankAccountNumber = _bankAccountService.GenerateUniqueBankAccountNumber(28),
                Type = BankAccountType.Deposit,
                Status = BankAccountStatus.Active,
                Client = client,
                Deposit = currentDeposit,
                Bank = currentDeposit.Bank
            };

            _context.Set<BankAccount>().Add(depositBankAccount);
            _context.SaveChanges();

            return depositBankAccount;
        }

        public void TransferMoneyForDeposit(long clientId, long depositId, long bankAccountSenderId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            Deposit currentDeposit = _context.Set<Deposit>().FirstOrDefault(dp => dp.Id == depositId && dp.Client.Id == clientId) ?? throw new KeyNotFoundException();

            // по уже одобренного депозита нельзя перевести деньги дважды!
            if (LocalValidator.IsActive(currentDeposit))
            {
                throw new InvalidOperationException();
            }

            BankAccount bankAccountSender = client.BankAccounts.FirstOrDefault(ba => ba.Id == bankAccountSenderId && ba.BankId == currentDeposit.BankId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(bankAccountSender))
            {
                throw new InvalidOperationException();
            }

            if (bankAccountSender.MoneyBalance < 0 || bankAccountSender.MoneyBalance < currentDeposit.DepositAmount)
            {
                throw new InvalidOperationException();
            }

            BankAccount masterBankAccount = GetMasterBankAccount(currentDeposit);
            
            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    BankAccount depositBankAccount = OpenDepositBankAccount(clientId, depositId);
                    _transactionService.SystemTransferMoney(currentDeposit.DepositAmount, bankAccountSenderId, 
                        depositBankAccount.Id, 
                        TransactionType.Deposit);

                    currentDeposit.Status = DepositStatus.Active;
                    _context.SaveChanges();
                    _transaction.Commit();
                }
                catch
                {
                    _transaction.Rollback();
                    throw;
                }
            }
        }

        public void ExecuteDepositMonthlyPayments()
        {
            int todayDay = DateTime.Today.Day;

            List<Deposit> activeDeposits = _context.Set<Deposit>().Include(d => d.Client).Include(d => d.BankAccount)
                .Where(d => d.OpenedAt.Day == todayDay && d.Status == DepositStatus.Active)
                .ToList();

            
            foreach (Deposit deposit in activeDeposits)
            {
                using (var _transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        decimal monthlyAccrual = CalculateMonthlyPayment(deposit.BankAccount!.MoneyBalance, deposit.DepositInterest);
                        BankAccount masterBankAccount = GetMasterBankAccount(deposit);
                        
                        _transactionService.SystemTransferMoney(monthlyAccrual, masterBankAccount.Id, deposit.BankAccount.Id);
                        if (DateTime.Today.Year == deposit.OpenedAt.Year)
                        {
                            BankAccount bankAccount = _context.Set<BankAccount>().First(ba => ba.ClientId == deposit.Client.Id && ba.Status == BankAccountStatus.Active && ba.Type == BankAccountType.Current && ba.BankId == deposit.BankId);
                            
                            // если срок вклада закончился, перевод средств клиенту 
                            _transactionService.SystemTransferMoney(deposit.BankAccount.MoneyBalance, deposit.BankAccount.Id, bankAccount.Id);
                            _bankAccountService.SystemCloseBankAccount(deposit.BankAccount.Id);
                            deposit.Status = DepositStatus.Closed;
                        }

                        _context.SaveChanges();
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}