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

        private decimal CalculateMonthlyPayment(decimal moneyBalance, decimal interest, int year, int month)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;

            decimal sum = moneyBalance * (interest / 100m) * daysInMonth * daysInYear;
             
            return Math.Round(sum, 2, MidpointRounding.ToEven);
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
            if (currentDeposit.Status != DepositStatus.Unactivated)
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
                ClientId = clientId,
                Bank = currentDeposit.Bank,
                BankId = currentDeposit.BankId
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

            BankAccount bankAccountSender = client.BankAccounts.FirstOrDefault(ba => ba.Id == bankAccountSenderId && ba.BankId == currentDeposit.BankId && ba.Type == BankAccountType.Current) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} with {nameof(BankAccount.Id)} = {bankAccountSenderId} is not found");
            if (!LocalValidator.IsActive(bankAccountSender))
            {
                throw new InvalidBankAccountStatusException($"Cannot transfer money to a deposit from unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            if (bankAccountSender.MoneyBalance < currentDeposit.DepositAmount)
            {
                throw new InsufficientFundsException($"Insufficient funds in the bank account. {nameof(bankAccountSender.MoneyBalance)} must be more or equal than {currentDeposit.DepositAmount}");
            }
            
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
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int todayDay = today.Day;

            bool isLastDayOfMonth = today.Day == DateTime.DaysInMonth(today.Year, today.Month);

            List<Deposit> activeDeposits = _context.Set<Deposit>()
                .Include(d => d.Client)
                .Include(d => d.BankAccount)
                .Where(d => d.Status == DepositStatus.Active && 
                (d.OpenedAt.Day == todayDay || (isLastDayOfMonth && d.OpenedAt.Day > todayDay)))
                .ToList();

            
            foreach (Deposit deposit in activeDeposits)
            {
                using (var _transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        decimal monthlyAccrual = CalculateMonthlyPayment(deposit.BankAccount!.MoneyBalance, deposit.DepositInterest, today.Year, today.Month);
                        BankAccount masterBankAccount = GetMasterBankAccount(deposit);
                        
                        _transactionService.SystemTransferMoney(monthlyAccrual, masterBankAccount.Id, deposit.BankAccount.Id, TransactionType.Deposit, deposit.Currency);

                        DateOnly expirationDate = deposit.OpenedAt.AddMonths(deposit.DepositTerm);

                        if (today >= expirationDate)
                        {
                            BankAccount bankAccount = _context.Set<BankAccount>()
                                .FirstOrDefault(ba => 
                                ba.ClientId == deposit.Client.Id &&
                                ba.Status == BankAccountStatus.Active &&
                                ba.Type == BankAccountType.Current &&
                                ba.BankId == deposit.BankId) 
                                ?? throw new BankAccountNotFoundException("");
                            
                            // если срок вклада закончился, перевод средств клиенту 
                            _transactionService.SystemTransferMoney(deposit.BankAccount.MoneyBalance, deposit.BankAccount.Id, bankAccount.Id, TransactionType.Deposit);
                            _bankAccountService.SystemCloseBankAccount(deposit.BankAccount.Id);
                            deposit.Status = DepositStatus.Closed;
                            deposit.ClosedAt = today;
                        }

                        _context.SaveChanges();
                        _transaction.Commit();
                    }
                    catch
                    {
                        _transaction.Rollback();
                    }
                }
            }
        }
    }
}