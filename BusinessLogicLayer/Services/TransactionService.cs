 using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Enums.Logs;
using DataAccessLayer.Enums.BankAccount;
using BusinessLogicLayer.Exceptions.Transaction;
using BusinessLogicLayer.Exceptions.BankAccount;
using Microsoft.AspNetCore.Connections.Features;

namespace BusinessLogicLayer.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly DbContext _context;
        private readonly ILoggerService _loggerService;

        public TransactionService(DbContext context, ILoggerService loggerService)
        {
            _context = context;
            _loggerService = loggerService;
        }

        public Transaction TransferMoney(long userId, decimal amount, string senderBankAccountNumber, string recieverBankAccountNumber)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(amount)} is negative. {nameof(amount)} must be more than 0");
            }

            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.UserId)} = {userId} is not found");

            BankAccount sender = client.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == senderBankAccountNumber) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} with {nameof(BankAccount.BankAccountNumber)} = {senderBankAccountNumber} of {nameof(Client)} with {nameof(Client.Id)} = {client.Id} is not found");

            if (sender.MoneyBalance < amount)
            {
                throw new InsufficientFundsException($"Insufficient funds in the bank account. {nameof(sender.MoneyBalance)} must be more or equal than {amount}");
            }
            if (!LocalValidator.IsActive(sender))
            {
                throw new InvalidTransactionStatusException($"Cannot transaction from unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            BankAccount reciever = _context.Set<BankAccount>().FirstOrDefault(ba => ba.BankAccountNumber == recieverBankAccountNumber) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} with {nameof(BankAccount.BankAccountNumber)} = {recieverBankAccountNumber} is not found");

            if (!LocalValidator.IsActive(reciever))
            {
                throw new InvalidTransactionStatusException($"Cannot transaction to unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
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
                        Type = TransactionType.PeerToPeer,
                        Currency = CurrencyType.BYN
                    };

                    _context.Set<Transaction>().Add(transaction);

                    _loggerService.MakeLog(OperationType.TRANSACTION_COMPLETED, nameof(Transaction), transaction.Id, newValue: amount.ToString());

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
                throw new ArgumentOutOfRangeException($"{nameof(amount)} is negative. {nameof(amount)} must be more than 0");
            }

            BankAccount sender = _context.Set<BankAccount>().FirstOrDefault(ba => ba.Id == senderBankAccountId) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} with {nameof(BankAccount.Id)} = {senderBankAccountId} is not found");

            if (sender.MoneyBalance < amount)
            {
                throw new InsufficientFundsException($"Insufficient funds in the bank account. {nameof(sender.MoneyBalance)} must be more or equal than {amount}");
            }
            if (!LocalValidator.IsActive(sender))
            {
                throw new InvalidTransactionStatusException($"Cannot transaction from unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            BankAccount reciever = _context.Set<BankAccount>().FirstOrDefault(ba => ba.Id == recieverBankAccountId) ?? throw new KeyNotFoundException();

            if (!LocalValidator.IsActive(reciever))
            {
                throw new InvalidTransactionStatusException($"Cannot transaction to unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
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

                    _loggerService.MakeLog(OperationType.TRANSACTION_COMPLETED, nameof(Transaction), transaction.Id, newValue: amount.ToString());

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
