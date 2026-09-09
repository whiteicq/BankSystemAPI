using BusinessLogicLayer.Exceptions.Bank;
using BusinessLogicLayer.Exceptions.BankAccount;
using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Exceptions.Deposit;
using BusinessLogicLayer.Exceptions.Transaction;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using DataAccessLayer.Enums.Logs;
using DataAccessLayer.Enums.Transaction;
using Microsoft.EntityFrameworkCore;

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
                throw new BankNotFoundException($"Entity of {nameof(Bank)} with {nameof(Bank.Id)} = {bankId} is not found");
            }

            if (sumOfDeposit <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(sumOfDeposit)} is negative or equal than 0. {nameof(sumOfDeposit)} must be more than 0");
            }

            if (term <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(term)} is negative or equal than 0. {nameof(term)} must be more than 0");
            }

            if (interest <= 5 || interest >= 11)
            {
                throw new ArgumentOutOfRangeException($"{nameof(interest)} have invalid value. Value of {nameof(interest)} must be in range of [5..11]");
            }

            Client client = _context.Set<Client>().FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.UserId)} = {userId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot apply a deposit for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
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
            BankAccount masterBankAccount = _context.Set<BankAccount>().FirstOrDefault(ba => ba.BankId == currentDeposit.BankId && ba.ClientId == null) ?? throw new BankAccountNotFoundException($"Master bank account {nameof(Bank)} with {nameof(Bank.Id)} = {currentDeposit.BankId} is not found");

            return masterBankAccount;
        }

        // открытие депозитного счета после одобрения
        private BankAccount OpenDepositBankAccount(long clientId, long depositId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.Deposits).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.Id)} = {clientId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot open a deposit bank account for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }

            Deposit currentDeposit = client.Deposits.FirstOrDefault(cr => cr.Id == depositId) ?? throw new DepositNotFoundException($"Entity of {nameof(Deposit)} with {nameof(Deposit.Id)} = {depositId} is not found");
            if (!LocalValidator.IsActive(currentDeposit))
            {
                throw new InvalidDepositStatusException($"Cannot open a deposit bank account for unactive deposit. The value of {nameof(DepositStatus)} must be { DepositStatus.Active}");
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
            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.Id)} = {clientId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot transfer money to a deposit for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }

            Deposit currentDeposit = _context.Set<Deposit>().FirstOrDefault(dp => dp.Id == depositId && dp.Client.Id == clientId) ?? throw new DepositNotFoundException($"Entity of {nameof(Deposit)} with {nameof(Deposit.Id)} = {depositId} is not found");

            // по уже одобренного депозита нельзя перевести деньги дважды!
            if (LocalValidator.IsActive(currentDeposit))
            {
                throw new InvalidDepositStatusException($"Cannot transfer money to deposit twice");
            }

            BankAccount bankAccountSender = client.BankAccounts.FirstOrDefault(ba => ba.Id == bankAccountSenderId && ba.BankId == currentDeposit.BankId) ?? throw new BankAccountNotFoundException("");
            if (!LocalValidator.IsActive(bankAccountSender))
            {
                throw new InvalidBankAccountStatusException($"Cannot transfer money to a deposit from unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            if (bankAccountSender.MoneyBalance < currentDeposit.DepositAmount)
            {
                throw new InsufficientFundsException($"Insufficient funds in the bank account. {nameof(bankAccountSender.MoneyBalance)} must be more or equal than {currentDeposit.DepositAmount}");
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