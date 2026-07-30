using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Exceptions.Client;

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
        public Transaction TransferMoney(long userId, decimal amount, string senderBankAccountNumber, string recieverBankAccountNumber, TransactionType type = TransactionType.PeerToPeer, CurrencyType currency = CurrencyType.BYN)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFound("");

            if (!client.BankAccounts.Any(ba => ba.BankAccountNumber == senderBankAccountNumber))
            {
                throw new InvalidOperationException("Операции с чужого счета запрещены");
            }

            BankAccount sender = client.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == senderBankAccountNumber) ?? throw new KeyNotFoundException();

            if (sender.MoneyBalance < amount)
            {
                throw new InvalidOperationException("Недостаточно средств на счету");
            }
            if (!LocalValidator.IsActive(sender))
            {
                throw new InvalidOperationException($"{nameof(sender)} не доступен для использования");
            }

            BankAccount reciever = _context.Set<BankAccount>().FirstOrDefault(ba => ba.BankAccountNumber == recieverBankAccountNumber) ?? throw new KeyNotFoundException();

            if (!LocalValidator.IsActive(reciever))
            {
                throw new InvalidOperationException($"{nameof(reciever)} не доступен для использования");
            }

            Transaction transaction = null!;

            bool isOuterTransaction = _context.Database.CurrentTransaction != null;
            using (var _localTransaction = isOuterTransaction ? null : _context.Database.BeginTransaction())
            {
                try
                {
                    sender.MoneyBalance -= amount;
                    reciever.MoneyBalance += amount;

                    transaction = new Transaction
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

            return transaction;
        }

        public Transaction SystemTransferMoney(decimal amount, long senderBankAccountId, long recieverBankAccountId, TransactionType type = TransactionType.PeerToPeer, CurrencyType currency = CurrencyType.BYN)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            BankAccount sender = _context.Set<BankAccount>().FirstOrDefault(ba => ba.Id == senderBankAccountId) ?? throw new KeyNotFoundException();

            if (sender.MoneyBalance < amount)
            {
                throw new InvalidOperationException("Недостаточно средств на счету");
            }
            if (!LocalValidator.IsActive(sender))
            {
                throw new InvalidOperationException($"{nameof(sender)} не доступен для использования");
            }

            BankAccount reciever = _context.Set<BankAccount>().FirstOrDefault(ba => ba.Id == recieverBankAccountId) ?? throw new KeyNotFoundException();

            if (!LocalValidator.IsActive(reciever))
            {
                throw new InvalidOperationException($"{nameof(reciever)} не доступен для использования");
            }

            Transaction transaction = null!;

            bool isOuterTransaction = _context.Database.CurrentTransaction != null;
            using (var _localTransaction = isOuterTransaction ? null : _context.Database.BeginTransaction())
            {
                try
                {
                    sender.MoneyBalance -= amount;
                    reciever.MoneyBalance += amount;

                    transaction = new Transaction
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

            return transaction;
        }
    }
}
