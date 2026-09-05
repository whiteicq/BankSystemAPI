using BusinessLogicLayer.Exceptions.BankAccount;
using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Exceptions.Bank;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Logs;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogicLayer.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly DbContext _context;
        private readonly ILoggerService _loggerService;


        public BankAccountService(DbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }

        public void CloseBankAccount(long userId, long bankAccountId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.UserId)} = {userId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot close bank account of unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }
            BankAccount bankAccountToClose = client.BankAccounts.FirstOrDefault(ba => ba.Id == bankAccountId) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} with {nameof(BankAccount.Id)} = {bankAccountId} is not found");
            
            if (!LocalValidator.IsActive(bankAccountToClose))
            {
                throw new InvalidBankAccountStatusException($"Cannot close an unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            if (bankAccountToClose.MoneyBalance > 0)
            {
                throw new InvalidOperationException("It is impossible to close an account with funds in the balance");
            }

            if (bankAccountToClose.MoneyBalance < 0)
            {
                throw new InvalidOperationException("It is impossible to close an account with a debt on the balance");
            }

            bankAccountToClose.Status = BankAccountStatus.Closed;
            bankAccountToClose.ClosedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_CLOSED, nameof(BankAccount), bankAccountToClose.Id, BankAccountStatus.Active.ToString(), bankAccountToClose.Status.ToString());
        }
        
        public void SystemCloseBankAccount(long bankAccountId)
        {
            BankAccount bankAccountToClose = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} with {nameof(BankAccount.Id)} = {bankAccountId} is not found");

            if (!LocalValidator.IsActive(bankAccountToClose))
            {
                throw new InvalidBankAccountStatusException($"Cannot close an unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            if (bankAccountToClose.MoneyBalance > 0)
            {
                throw new ArgumentOutOfRangeException($"It is impossible to close an account with funds in the {nameof(bankAccountToClose.MoneyBalance)}. Value of {nameof(bankAccountToClose.MoneyBalance)} must be equal 0");
            }

            if (bankAccountToClose.MoneyBalance < 0)
            {
                throw new ArgumentOutOfRangeException($"It is impossible to close an account with a debt on the {nameof(bankAccountToClose.MoneyBalance)}. Value of {nameof(bankAccountToClose.MoneyBalance)} must be equal 0");
            }

            bankAccountToClose.Status = BankAccountStatus.Closed;
            bankAccountToClose.ClosedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_CLOSED, nameof(BankAccount), bankAccountToClose.Id, BankAccountStatus.Active.ToString(), bankAccountToClose.Status.ToString());
        }

        public BankAccount OpenBankAccount(long userId, long bankId, BankAccountType bankAccountType = BankAccountType.Current)
        {
            bool bankExists = _context.Set<Bank>().Any(b => b.Id == bankId);
            if (!bankExists)
            {
                throw new BankNotFoundException($"Entity of {nameof(Bank)} with {nameof(Bank.Id)} = {bankId} is not found");
            }

            Client client = _context.Set<Client>().FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.UserId)} = {userId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot open a bank account for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }

            BankAccount newBankAccount = new BankAccount()
            {
                BankAccountNumber = GenerateUniqueBankAccountNumber(28),
                Type = bankAccountType,
                Client = client,
                BankId = bankId,
                OpenedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.Set<BankAccount>().Add(newBankAccount);

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_OPENED, nameof(BankAccount), newBankAccount.Id, newValue: BankAccountStatus.Unactivated.ToString());

            return newBankAccount;
        }

        public string GenerateUniqueBankAccountNumber(int length)
        {
            if (length < 0)
            {
                return string.Empty;
            }

            string uniquebankAccountNumber = string.Empty;
            bool isDublicate;

            do
            {
                uniquebankAccountNumber = GenerateSpecifiedLengthString(length);
                isDublicate = _context.Set<BankAccount>().Any(ba => ba.BankAccountNumber == uniquebankAccountNumber);
            }
            while (isDublicate);

            return uniquebankAccountNumber;
        }

        private string GenerateSpecifiedLengthString(int length)
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int digit = RandomNumberGenerator.GetInt32(0, 10);
                result.Append(digit);
            }

            return result.ToString();
        }
    }
}