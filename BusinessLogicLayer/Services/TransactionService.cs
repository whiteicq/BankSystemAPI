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
        private readonly DbContext _context;

        public TransactionService(DbContext context)
        {
            _context = context;
        }
        // пока не понятно как приделать тип транзакции и валюту (на моменте списания/зачисления средств)
        public void TransferMoney(decimal amount, long senderId, long recieverId, TransactionType type = TransactionType.PeerToPeer, CurrencyType currency = CurrencyType.BYN)
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

            bool isOuterTransaction = _context.Database.CurrentTransaction != null;
            using (var _localTransaction = isOuterTransaction ? null : _context.Database.BeginTransaction())
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
                        Type = type,
                        Currency = currency
                    };

                    _context.Set<Transaction>().Add(transaction);
                    _context.SaveChanges();

                    if (!isOuterTransaction) 
                    {
                        _localTransaction?.Commit(); 
                    }
                }
                catch
                {
                    if (!isOuterTransaction)
                    {
                        _localTransaction?.Rollback();
                    }

                    throw;
                }
            }
        }


    }
}
