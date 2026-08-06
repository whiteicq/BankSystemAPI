using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Enums.Logs;
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

        public void CloseBankAccount(long userId, string bankAccountNumber)
        {
            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.UserId == userId) ?? throw new KeyNotFoundException();

            BankAccount bankAccountToClose = client.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == bankAccountNumber) ?? throw new InvalidOperationException();
            
            if (!LocalValidator.IsActive(bankAccountToClose))
            {
                throw new InvalidOperationException("Невозможно закрыть неактивный счет");
            }

            if (bankAccountToClose.MoneyBalance > 0)
            {
                throw new InvalidOperationException("Невозможно закрыть счет со средствами на балансе");
            }

            if (bankAccountToClose.MoneyBalance < 0)
            {
                throw new InvalidOperationException("Невозможно закрыть счет с задолженностью на балансе");
            }

            bankAccountToClose.Status = BankAccountStatus.Closed;
            bankAccountToClose.ClosedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_CLOSED, nameof(BankAccount), bankAccountToClose.Id, BankAccountStatus.Active.ToString(), bankAccountToClose.Status.ToString());
        }
        
        public void SystemCloseBankAccount(long bankAccountId)
        {
            BankAccount bankAccountToClose = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new InvalidOperationException();

            if (!LocalValidator.IsActive(bankAccountToClose))
            {
                throw new InvalidOperationException("Невозможно закрыть неактивный счет");
            }

            if (bankAccountToClose.MoneyBalance > 0)
            {
                throw new InvalidOperationException("Невозможно закрыть счет со средствами на балансе");
            }

            if (bankAccountToClose.MoneyBalance < 0)
            {
                throw new InvalidOperationException("Невозможно закрыть счет с задолженностью на балансе");
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
                throw new InvalidDataException();
            }

            Client client = _context.Set<Client>().FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFound($"{nameof(client)} is null");
            if (LocalValidator.IsActive(client))
            {
                throw new InvalidStatus();
            }

            BankAccount newBankAccount = new BankAccount()
            {
                BankAccountNumber = GenerateUniqueBankAccountNumber(28),
                Type = bankAccountType,
                Client = client,
                BankId = bankId
            };

            _context.Set<BankAccount>().Add(newBankAccount);

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.BANK_ACCOUNT_OPENED, nameof(BankAccount), newBankAccount.Id, newValue: BankAccountStatus.Unactivated.ToString());

            return newBankAccount;
        }

        public string GenerateUniqueBankAccountNumber(int length)
        {
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