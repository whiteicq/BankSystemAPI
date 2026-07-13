 using DataAccessLayer.Enums.FinancialProduct.Credit;
using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Enums.BankAccount;
using BusinessLogicLayer.Infrastructure;

namespace BusinessLogicLayer.Services
{
    public class CreditService : ICreditService
    {
        private ITransactionService _transactionService;
        private IBankAccountService _bankAccountService;
        private DbContext _context;
        
        public CreditService(DbContext context, ITransactionService transactionService, IBankAccountService bankAccountService)
        {
            _context = context;
            _transactionService = transactionService;
            _bankAccountService = bankAccountService;
        }

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

            if (interest >= 14 || interest <= 25)
            {
                throw new ArgumentOutOfRangeException();
            }

            Client client = _context.Set<Client>().Find(clientId) ?? throw new ClientNotFound("");

            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidOperationException();
            }

            decimal totalLoanBalance = CalculateMontlyPayment(sumOfLoan, term, interest) * term;

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

        // НЕ ЗАКОНЧЕН!!!!!!!!!!
        public void LoanPayment(long creditId)
        {
            Credit currentCredit = _context.Set<Credit>()
                .Include(cr => cr.Client)
                .FirstOrDefault(cr => cr.Id == creditId) ?? throw new KeyNotFoundException();
            Client creditOwner = currentCredit.Client;
            // creditOwner.
            if (!LocalValidator.IsActive(creditOwner))
            {
                throw new InvalidStatus();
            }

            if (!LocalValidator.IsActive(currentCredit))
            {
                throw new InvalidOperationException();
            }

            decimal montlyPayment = CalculateMontlyPayment(currentCredit.LoanAmount, currentCredit.LoanTerm, currentCredit.LoanInterest);
        }

        // мб Строитель подойдет, разбить пошагово заполнения полей при создании Счёта
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
                Client = client
            };

            _context.Set<BankAccount>().Add(creditBankAccount);
        }

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

            Credit sourceCredit = _context.Set<Credit>().Find(creditId) ?? throw new KeyNotFoundException();
            if (!LocalValidator.IsActive(sourceCredit))
            {
                throw new InvalidOperationException();
            }

            bankAccountReciever.MoneyBalance += sourceCredit.LoanAmount;
        }

        private decimal CalculateMontlyPayment(decimal loanAmount, int loanTerm, decimal loanInterest)
        {
            loanInterest /= 100m / 12m;
             
            double powBase = (double)(1m + loanInterest);
            decimal powResult = (decimal)Math.Pow(powBase, loanTerm);

            decimal montlyPayment = loanAmount * loanInterest * powResult / (powResult - 1);

            return Math.Round(montlyPayment, 2, MidpointRounding.ToEven);
        }
    }
}
