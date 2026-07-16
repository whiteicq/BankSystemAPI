using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using Microsoft.EntityFrameworkCore;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Transaction;

namespace BusinessLogicLayer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly DbContext _context;

        public EmployeeService(DbContext context)
        {
            _context = context;
        }

        public void ActivateClient(long clientId)
        {
            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");
            client.Status = ClientStatus.Active;

            _context.SaveChanges();
        }

        public void ActiveBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            bankAccount.Status = BankAccountStatus.Active;

            _context.SaveChanges();
        }

        public void ActiveCredit(long creditId)
        {
            Credit credit = _context.Set<Credit>().Find(creditId) ?? throw new KeyNotFoundException();
            credit.Status = CreditStatus.Active;

            _context.SaveChanges();
        }

        public void ActiveDeposit(long depositId)
        {
            Deposit deposit = _context.Set<Deposit>().Find(depositId) ?? throw new KeyNotFoundException();
            deposit.Status = DepositStatus.Active;

            _context.SaveChanges();
        }

        public void BlockBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            bankAccount.Status = BankAccountStatus.Blocked;

            _context.SaveChanges();
        }

        public void BlockClient(long clientId)
        {
            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");
            client.Status = ClientStatus.Blocked;

            _context.SaveChanges();
        }

        public void CancelTransaction(long transactionId)
        {
            Transaction transaction = _context.Set<Transaction>()
                .Include(tr => tr.Sender)
                .Include(tr => tr.Receiver)
                .FirstOrDefault(tr => tr.Id == transactionId 
                && tr.Status == TransactionStatus.Confirmed 
                && tr.Type == TransactionType.PeerToPeer) ?? throw new KeyNotFoundException();
           
            BankAccount sender = transaction.Sender;
            BankAccount receiver = transaction.Receiver;
            decimal transactionAmount = transaction.TransactionAmount;

            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    receiver.MoneyBalance -= transactionAmount;
                    sender.MoneyBalance += transactionAmount;
                    transaction.Status = TransactionStatus.Canceled;

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

        public void CloseBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            bankAccount.Status = BankAccountStatus.Closed;

            _context.SaveChanges();
        }

        public void FreezeBankAccount(long bankAccountId)
        {
            BankAccount bankAccount = _context.Set<BankAccount>().Find(bankAccountId) ?? throw new KeyNotFoundException();
            bankAccount.Status = BankAccountStatus.Frozen;

            _context.SaveChanges();
        }

        public void FreezeTransaction(long transactionId)
        {
            throw new NotImplementedException();
        }

        public void RejectCredit(long creditId)
        {
            Credit credit = _context.Set<Credit>().Find(creditId) ?? throw new KeyNotFoundException();
            credit.Status = CreditStatus.Rejected;

            _context.SaveChanges();
        }

        public void RejectDeposit(long depositId)
        {
            Deposit deposit = _context.Set<Deposit>().Find(depositId) ?? throw new KeyNotFoundException();
            deposit.Status = DepositStatus.Rejected;

            _context.SaveChanges();
        }
    }
}
