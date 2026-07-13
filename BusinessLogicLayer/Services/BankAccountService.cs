using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Common;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogicLayer.Services
{
    public class BankAccountService : IBankAccountService
    {
        private DbContext _context;

        public BankAccountService(DbContext context)
        {
            _context = context;
        }

        public void CloseBankAccount(long bankAccountId)
        {
            BankAccount bankAccountToClose = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();

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
        }
        
        public void OpenBankAccount(long clientId, long bankId, BankAccountType bankAccountType = BankAccountType.Current)
        {
            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound($"{nameof(client)} is null");

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