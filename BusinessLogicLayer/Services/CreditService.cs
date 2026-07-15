 using DataAccessLayer.Enums.FinancialProduct.Credit;
using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Enums.BankAccount;
using BusinessLogicLayer.Infrastructure;
using DataAccessLayer.Enums.Transaction;

namespace BusinessLogicLayer.Services
{
    public class CreditService : ICreditService
    {
        private readonly ITransactionService _transactionService;
        private readonly IBankAccountService _bankAccountService;
        private readonly DbContext _context;
        
        public CreditService(DbContext context, ITransactionService transactionService, IBankAccountService bankAccountService)
        {
            _context = context;
            _transactionService = transactionService;
            _bankAccountService = bankAccountService;
        }

        // запрос клиента на кредит
        public void RequestCredit(long clientId, decimal sumOfLoan, int term, decimal interest)
        {
            if (sumOfLoan <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (term <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (interest <= 14 || interest >= 25)
            {
                throw new ArgumentOutOfRangeException();
            }

            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");

            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            decimal totalLoanBalance = CalculateMontlyPayment(sumOfLoan, term, interest) * term;

            // тут же формирование неактивированного кредита
            Credit credit = new Credit
            {
                LoanAmount = sumOfLoan,
                LoanBalance = totalLoanBalance,
                LoanTerm = term,
                LoanInterest = interest,
                Client = client
            };

            _context.Set<Credit>().Add(credit);

            _context.SaveChanges();
        }

        // открытие кредитного счета после одобрения
        private void OpenCreditBankAccount(long clientId, long creditId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.Credits).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            Credit currentCredit = client.Credits.FirstOrDefault(cr => cr.Id == creditId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(currentCredit))
            {
                throw new InvalidOperationException();
            }

            BankAccount creditBankAccount = new BankAccount
            {
                BankAccountNumber = _bankAccountService.GenerateUniqueBankAccountNumber(28),
                MoneyBalance = -currentCredit.LoanBalance,
                Type = BankAccountType.Credit,
                Status = BankAccountStatus.Active,
                Client = client,
                Credit = currentCredit
            };

            _context.Set<BankAccount>().Add(creditBankAccount);
            _context.SaveChanges();
        }

        // перевод денег по кредиту в случае одобрения кредита
        public void TransferMoneyForLoan(long clientId, long creditId, long bankAccountRecieverId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFound("");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            BankAccount bankAccountReciever = client.BankAccounts.FirstOrDefault(ba => ba.Id == bankAccountRecieverId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(bankAccountReciever))
            {
                throw new InvalidOperationException();
            }

            Credit currentCredit = _context.Set<Credit>().Find(creditId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(currentCredit))
            {
                throw new InvalidOperationException();
            }
            BankAccount masterBankAccount = GetMasterBankAccount(currentCredit);

            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _transactionService.TransferMoney(currentCredit.LoanAmount, masterBankAccount.Id, bankAccountRecieverId);
                    OpenCreditBankAccount(clientId, creditId);

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

        private BankAccount GetMasterBankAccount(Credit currentCredit)
        {
            BankAccount masterBankAccount = _context.Set<BankAccount>().FirstOrDefault(ba => ba.BankId == currentCredit.BankId && ba.ClientId == null) ?? throw new KeyNotFoundException();

            return masterBankAccount;
        }

        private decimal CalculateMontlyPayment(decimal loanAmount, int loanTerm, decimal loanInterest)
        {
            loanInterest /= 100m / 12m;
             
            double powBase = (double)(1m + loanInterest);
            decimal powResult = (decimal)Math.Pow(powBase, loanTerm);

            decimal montlyPayment = loanAmount * loanInterest * powResult / (powResult - 1);

            return Math.Round(montlyPayment, 2, MidpointRounding.ToEven);
        }

        // ежемесячное списание средств по кредиту 
        public void ExecuteLoanMonthlyPayments()
        {
            int todayDay = DateTime.Today.Day;

            List<Credit> activeCredits = _context.Set<Credit>().Include(cr => cr.Client)
                .Where(cr => cr.Status == CreditStatus.Active && cr.OpenedAt.Day == todayDay)
                .ToList();

            foreach (var credit in activeCredits)
            {
                using (var _transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        decimal montlyPayment = CalculateMontlyPayment(credit.LoanAmount, credit.LoanTerm, credit.LoanInterest);

                        Client client = credit.Client;
                        List<BankAccount> bankAccountsOfClientForWriteOff = _context.Set<BankAccount>().Where(ba => ba.ClientId == client.Id 
                        && ba.Type == BankAccountType.Current 
                        && ba.Status == BankAccountStatus.Active
                        && ba.MoneyBalance >= montlyPayment).ToList();

                        if (bankAccountsOfClientForWriteOff is null)
                        {
                            credit.Status = CreditStatus.Expired;
                            _context.SaveChanges();
                            _transaction.Commit();
                            return;
                        }

                        BankAccount currentBankAccount = bankAccountsOfClientForWriteOff.First();
                        BankAccount masterBankAccount = GetMasterBankAccount(credit);

                        _transactionService.TransferMoney(montlyPayment, currentBankAccount.Id, masterBankAccount.Id, TransactionType.Credit);

                        BankAccount? creditBankAccount = _context.Set<BankAccount>().FirstOrDefault(ba => ba.Id == credit.BankId) ?? throw new KeyNotFoundException();
                        credit.LoanBalance -= montlyPayment;
                        creditBankAccount.MoneyBalance -= montlyPayment;
                        if (creditBankAccount.MoneyBalance >= 0 || credit.LoanBalance <= 0)
                        {
                            credit.Status = CreditStatus.Closed;
                            _bankAccountService.CloseBankAccount(creditBankAccount.Id);
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
