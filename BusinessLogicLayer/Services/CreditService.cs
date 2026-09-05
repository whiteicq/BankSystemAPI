using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Exceptions.Bank;
using BusinessLogicLayer.Exceptions.Credit;
using BusinessLogicLayer.Infrastructure;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Logs;
using DataAccessLayer.Enums.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Connections.Features;
using BusinessLogicLayer.Exceptions.BankAccount;

namespace BusinessLogicLayer.Services
{
    public class CreditService : ICreditService
    {
        private readonly ITransactionService _transactionService;
        private readonly IBankAccountService _bankAccountService;
        private readonly DbContext _context;
        private readonly ILoggerService _loggerService;

        public CreditService(DbContext context, ITransactionService transactionService, IBankAccountService bankAccountService, ILoggerService loggerService)
        {
            _context = context;
            _transactionService = transactionService;
            _bankAccountService = bankAccountService;
            _loggerService = loggerService;
        }

        // запрос клиента на кредит
        public Credit RequestCredit(long userId, long bankId, decimal sumOfLoan, int term, decimal interest)
        {
            bool bankExists = _context.Set<Bank>().Any(b => b.Id == bankId);
            if (!bankExists)
            {
                throw new BankNotFoundException($"Entity of {nameof(Bank)} with {nameof(Bank.Id)} = {bankId} is not found");
            }   

            if (sumOfLoan <= 0)
            {
                throw new ArgumentOutOfRangeException($"It is not possible to apply for a loan with a negative {nameof(sumOfLoan)}. Value of {nameof(sumOfLoan)} must be positive");
            }

            if (term <= 0)
            {
                throw new ArgumentOutOfRangeException($"It is not possible to apply for a loan for a negative {nameof(term)}. Value of {nameof(term)} must be positive");
            }

            if (interest <= 14 || interest >= 25)
            {
                throw new ArgumentOutOfRangeException($"It is not possible to apply for a loan with a invalid value of {nameof(interest)}. Value of {nameof(interest)} must be in range of [14..25]");
            }

            Client client = _context.Set<Client>().FirstOrDefault(cl => cl.UserId == userId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.UserId)} = {userId} is not found");

            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot apply for a loan for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }

            decimal totalLoanBalance = CalculateMontlyPayment(sumOfLoan, term, interest) * term;

            // тут же формирование неактивированного кредита
            Credit credit = new Credit
            {
                LoanAmount = sumOfLoan,
                LoanBalance = totalLoanBalance,
                LoanTerm = term,
                LoanInterest = interest,
                Client = client,
                BankId = bankId
            };

            _context.Set<Credit>().Add(credit);

            _context.SaveChanges();

            _loggerService.MakeLog(OperationType.CREDIT_REQUESTED, nameof(Credit), credit.Id, newValue:CreditStatus.Unactivated.ToString());

            return credit;
        }

        // открытие кредитного счета после одобрения
        private void OpenCreditBankAccount(long clientId, long creditId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.Credits).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.Id)} = {clientId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot open credit for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }

            Credit currentCredit = client.Credits.FirstOrDefault(cr => cr.Id == creditId) ?? throw new CreditNotFoundException($"Entity of {nameof(Credit)} with {nameof(Credit.Id)} = {creditId} is not found");
            if (!LocalValidator.IsActive(currentCredit))
            {
                throw new InvalidCreditStatusException($"Cannot create credit bank account for unactive credit. The value of {nameof(CreditStatus)} must be {CreditStatus.Active}");
            }

            BankAccount creditBankAccount = new BankAccount
            {
                BankAccountNumber = _bankAccountService.GenerateUniqueBankAccountNumber(28),
                MoneyBalance = -currentCredit.LoanBalance,
                Type = BankAccountType.Credit,
                Status = BankAccountStatus.Active,
                Client = client,
                Credit = currentCredit,
                Bank = currentCredit.Bank
            };

            _context.Set<BankAccount>().Add(creditBankAccount);
            _context.SaveChanges();
        }

        // перевод денег по кредиту в случае одобрения кредита (ВЫЗЫВАТЬ СОТРУДНИКОМ ПРИ ОДОБРЕНИИ!)
        public void TransferMoneyForLoan(long clientId, long creditId, long bankAccountRecieverId)
        {
            Client client = _context.Set<Client>().Include(cl => cl.BankAccounts).FirstOrDefault(cl => cl.Id == clientId) ?? throw new ClientNotFoundException($"Entity of {nameof(Client)} with {nameof(Client.Id)} = {clientId} is not found");
            if (!LocalValidator.IsActive(client))
            {
                throw new InvalidClientStatusException($"Cannot transef money on loan for unactive client. The value of {nameof(ClientStatus)} must be {ClientStatus.Active}");
            }

            Credit currentCredit = _context.Set<Credit>().FirstOrDefault(cr => cr.Id == creditId && cr.ClientId == client.Id) ?? throw new CreditNotFoundException($"Entity of {nameof(Credit)} with {nameof(Credit.Id)} = {creditId} & {nameof(Client.Id)} = {clientId} is not found");

            // по уже одобренному кредиту нельзя перевести деньги дважды!
            if (LocalValidator.IsActive(currentCredit))
            {
                throw new InvalidCreditStatusException("Cannot transfer money on loan twice");
            }

            BankAccount bankAccountReciever = client.BankAccounts.FirstOrDefault(ba => ba.Id == bankAccountRecieverId && ba.BankId == currentCredit.BankId) ?? throw new BankAccountNotFoundException($"Entity of {nameof(BankAccount)} is not found");
            if (!LocalValidator.IsActive(bankAccountReciever))
            {
                throw new InvalidBankAccountStatusException($"Cannot transfer money on unactive bank account. The value of {nameof(BankAccountStatus)} must be {BankAccountStatus.Active}");
            }

            
            BankAccount masterBankAccount = GetMasterBankAccount(currentCredit);

            using (var _transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _transactionService.SystemTransferMoney(currentCredit.LoanAmount, masterBankAccount.Id, bankAccountRecieverId, TransactionType.Credit);
                    OpenCreditBankAccount(clientId, creditId);

                    currentCredit.Status = CreditStatus.Active;
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
                        List<BankAccount> bankAccountsOfClientForWriteOff = _context.Set<BankAccount>().
                            Where(ba => ba.ClientId == client.Id 
                            && ba.Type == BankAccountType.Current 
                            && ba.Status == BankAccountStatus.Active
                            && ba.MoneyBalance >= montlyPayment
                            && ba.BankId == credit.BankId).ToList();

                        if (bankAccountsOfClientForWriteOff is null)
                        {
                            credit.Status = CreditStatus.Expired;
                            _context.SaveChanges();
                            _transaction.Commit();
                            return;
                        }

                        BankAccount currentBankAccount = bankAccountsOfClientForWriteOff.First();
                        BankAccount masterBankAccount = GetMasterBankAccount(credit);

                        _transactionService.SystemTransferMoney(montlyPayment, currentBankAccount.Id, masterBankAccount.Id, TransactionType.Credit, credit.Currency);

                        BankAccount? creditBankAccount = _context.Set<BankAccount>().FirstOrDefault(ba => ba.Id == credit.BankAccountId) ?? throw new KeyNotFoundException();
                        credit.LoanBalance -= montlyPayment;
                        creditBankAccount.MoneyBalance -= montlyPayment;
                        if (creditBankAccount.MoneyBalance >= 0 || credit.LoanBalance <= 0)
                        {
                            credit.Status = CreditStatus.Closed;
                            _bankAccountService.SystemCloseBankAccount(creditBankAccount.Id);
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
