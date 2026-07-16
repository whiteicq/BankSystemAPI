using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Enums.Transaction;

namespace BusinessLogicLayer.Services
{
    public class DepositService : IDepositService
    {
        private readonly DbContext _context;
        private readonly IBankAccountService _bankAccountService;
        private readonly ITransactionService _transactionService;
        public DepositService(DbContext context, IBankAccountService bankAccountService, ITransactionService transactionService)
        {
            _context = context;
            _bankAccountService = bankAccountService;
            _transactionService = transactionService;
        }

        public void RequestDeposit(long clientId, decimal sumOfDeposit, int term, decimal interest)
        {
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

            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            Deposit deposit = new Deposit
            {
                DepositAmount = sumOfDeposit,
                DepositTerm = term,
                DepositInterest = interest,
                Status = DepositStatus.Unactivated,
                Client = client
            };

            _context.Set<Deposit>().Add(deposit);
            _context.SaveChanges();
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
                Deposit = currentDeposit
            };

            _context.Set<BankAccount>().Add(depositBankAccount);
            _context.SaveChanges();

            return depositBankAccount;
        }

        // не доделано
        public void TransferMoneyForDeposit(long clientId, long depositId, long bankAccountSenderId)
        {
            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            Deposit currentDeposit = _context.Set<Deposit>().Find(depositId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(currentDeposit))
            {
                throw new InvalidOperationException();
            }

            BankAccount masterBankAccount = GetMasterBankAccount(currentDeposit);
            BankAccount bankAccountSender = _context.Set<BankAccount>().Find(bankAccountSenderId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(bankAccountSender))
            {
                throw new InvalidOperationException();
            }

            if (bankAccountSender.MoneyBalance < 0 || bankAccountSender.MoneyBalance < currentDeposit.DepositAmount)
            {
                throw new InvalidOperationException();
            }

            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    BankAccount depositBankAccount = OpenDepositBankAccount(clientId, depositId);
                    _transactionService.TransferMoney(currentDeposit.DepositAmount, bankAccountSenderId, 
                        depositBankAccount.Id, 
                        TransactionType.Deposit);

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
                        
                        _transactionService.TransferMoney(monthlyAccrual, masterBankAccount.Id, deposit.BankAccount.Id);
                        if (DateTime.Today.Year == deposit.OpenedAt.Year)
                        {
                            deposit.Status = DepositStatus.Closed;
                            BankAccount bankAccount = _context.Set<BankAccount>().First(ba => ba.ClientId == deposit.Client.Id && ba.Status == BankAccountStatus.Active && ba.Type == BankAccountType.Current);
                            // если срок вклада закончился, перевод средств клиенту 
                            _transactionService.TransferMoney(deposit.BankAccount.MoneyBalance, deposit.BankAccount.Id, bankAccount.Id);
                            deposit.BankAccount.Status = BankAccountStatus.Closed;
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