using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using BusinessLogicLayer.Infrastructure;

namespace BusinessLogicLayer.Services
{
    public class TransactionService : ITransactionService
    {
        private DbContext _context;

        public TransactionService(DbContext context)
        {
            _context = context;
        }
        // пока не понятно как приделать тип транзакции и валюту (на моменте списания/зачисления средств)
        public void TransferMoney(decimal amount, long senderId, long recieverId, TransactionType type, CurrencyType currency)
        {
            BankAccount sender = _context.Set<BankAccount>().Find(senderId) ?? throw new KeyNotFoundException();
            BankAccount reciever = _context.Set<BankAccount>().Find(recieverId) ?? throw new KeyNotFoundException();

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (!LocalValidator.IsActive(sender))
            {
                throw new InvalidOperationException($"{nameof(sender)} не доступен для использования");
            }

            if (!LocalValidator.IsActive(reciever))
            {
                throw new InvalidOperationException($"{nameof(reciever)} не доступен для использования");
            }

            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    sender.MoneyBalance -= amount;
                    reciever.MoneyBalance += amount;

                    Transaction transaction = new Transaction
                    {
                        TransactionAmount = amount,
                        Sender = sender,
                        Receiver = reciever,
                        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                        Type = type,
                        Currency = currency
                    };

                    _context.Set<Transaction>().Add(transaction);
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
