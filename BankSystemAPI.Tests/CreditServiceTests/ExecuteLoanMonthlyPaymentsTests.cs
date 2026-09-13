using BankSystemAPI.Tests.CreditServiceTests;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Transaction;
using Moq;
using DataAccessLayer.Enums.Common;

namespace BankSystemAPI.Tests.CreditServiceTests
{
    public class ExecuteLoanMonthlyPaymentsTests : CreditServiceTests
    {
        [Fact]
        public void ExecuteLoanMonthlyPayments_SuccessfulPayment_ShouldDeductBalanceAndTransferMoney()
        {
            // Arrange
            Bank bank = CreateTestBank();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // 1. Create a client and an active current account with enough money
            Client client = CreateTestClient(id: 1L);
            BankAccount currentAccount = CreateTestBankAccount(id: 2L, clientId: client.Id, bankId: bank.Id, moneyBalance: 5000m, type: BankAccountType.Current);
            client.BankAccounts.Add(currentAccount);

            // 2. Create the bank's master account
            BankAccount masterAccount = CreateTestBankAccount(id: 3L, clientId: null, bankId: bank.Id);

            // 3. Create a credit account linked to the credit
            long creditAccountId = 4L;
            BankAccount creditAccount = CreateTestBankAccount(id: creditAccountId, clientId: client.Id, bankId: bank.Id, moneyBalance: -6000m, type: BankAccountType.Credit);

            // 4. Create an active credit that matches today's day of month
            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id, loanAmount: 5000m, loanBalance: 6000m, status: CreditStatus.Active);
            credit.OpenedAt = today; // Forces cr.OpenedAt.Day == todayDay to be true
            credit.BankAccountId = creditAccountId;
            client.Credits.Add(credit);

            _context.Banks.Add(bank);
            _context.BankAccounts.AddRange(currentAccount, masterAccount, creditAccount);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Calculate expected payment to verify correct math inside the test assert
            // (Assuming 5000 loan, 12 months, 15% interest results in ~451.29 monthly payment)
            decimal expectedPayment = 451.29m;

            // Act
            _service.ExecuteLoanMonthlyPayments();

            // Assert
            // Verify that the loan balance was decreased
            Assert.Equal(6000m - expectedPayment, credit.LoanBalance);

            // Verify that the credit bank account balance moved closer to zero
            Assert.Equal(-6000m + expectedPayment, creditAccount.MoneyBalance);

            // Verify that the status remains Active because the loan is not fully paid yet
            Assert.Equal(CreditStatus.Active, credit.Status);

            // Verify that the external transaction service transferred the exact monthly payment
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                expectedPayment,
                currentAccount.Id,
                masterAccount.Id,
                TransactionType.Credit,
                credit.Currency
                ),
                Times.Once);
        }

        [Fact]
        public void ExecuteLoanMonthlyPayments_NoMoneyOnCurrentAccount_ShouldSetStatusToExpired()
        {
            // Arrange
            Bank bank = CreateTestBank();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            Client client = CreateTestClient(id: 1L);
            // Client has an account, but the balance is 0 (less than monthly payment)
            BankAccount currentAccount = CreateTestBankAccount(id: 2L, clientId: client.Id, bankId: bank.Id, moneyBalance: 0m, type: BankAccountType.Current);
            client.BankAccounts.Add(currentAccount);

            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id, status: CreditStatus.Active);
            credit.OpenedAt = today;
            client.Credits.Add(credit);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(currentAccount);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act
            _service.ExecuteLoanMonthlyPayments();

            // Assert
            // Verify that the credit status was changed to Expired due to insufficient funds
            Assert.Equal(CreditStatus.Expired, credit.Status);

            // Verify that no money transfer was attempted
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                It.IsAny<decimal>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<TransactionType>(), It.IsAny<CurrencyType>()
                ),
                Times.Never);
        }

        [Fact]
        public void ExecuteLoanMonthlyPayments_LastPayment_ShouldCloseCreditAndCloseBankAccount()
        {
            // Arrange
            Bank bank = CreateTestBank();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            Client client = CreateTestClient(id: 1L);
            BankAccount currentAccount = CreateTestBankAccount(id: 2L, clientId: client.Id, bankId: bank.Id, moneyBalance: 5000m, type: BankAccountType.Current);
            client.BankAccounts.Add(currentAccount);
            BankAccount masterAccount = CreateTestBankAccount(id: 3L, clientId: null, bankId: bank.Id);

            // The remaining debt is very low (e.g., 100), meaning the monthly payment will fully cover it
            long creditAccountId = 4L;
            BankAccount creditAccount = CreateTestBankAccount(id: creditAccountId, clientId: client.Id, bankId: bank.Id, moneyBalance: -100m, type: BankAccountType.Credit);

            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id, loanAmount: 5000m, loanBalance: 100m, status: CreditStatus.Active);
            credit.OpenedAt = today;
            credit.BankAccountId = creditAccountId;
            client.Credits.Add(credit);

            _context.Banks.Add(bank);
            _context.BankAccounts.AddRange(currentAccount, masterAccount, creditAccount);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act
            _service.ExecuteLoanMonthlyPayments();

            // Assert
            // Verify that the credit is completely paid off and closed
            Assert.Equal(CreditStatus.Closed, credit.Status);
            Assert.Equal(today, credit.ClosedAt);
            Assert.Equal(0m, credit.LoanBalance);
            Assert.Equal(0m, creditAccount.MoneyBalance);

            // Verify that the external bank account service was called to officially close the credit account
            _bankAccountServiceMock.Verify(s => s.SystemCloseBankAccount(creditAccountId), Times.Once);
        }

        [Fact]
        public void ExecuteLoanMonthlyPayments_WrongDayOfRecord_ShouldSkipProcessing()
        {
            // Arrange
            Bank bank = CreateTestBank();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // Calculate a day that does NOT match today's day to ensure the credit is filtered out
            // If today is the 15th, we set OpenedAt to the 14th. If today is the 1st, we set it to the 2nd.
            int nonMatchingDay = today.Day == 1 ? 2 : today.Day - 1;
            DateOnly wrongDate = new DateOnly(today.Year, today.Month, nonMatchingDay);

            Client client = CreateTestClient(id: 1L);
            BankAccount currentAccount = CreateTestBankAccount(id: 2L, clientId: client.Id, bankId: bank.Id, moneyBalance: 5000m, type: BankAccountType.Current);
            client.BankAccounts.Add(currentAccount);

            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id, status: CreditStatus.Active);
            credit.OpenedAt = wrongDate; // This should cause the Where clause to skip this credit
            client.Credits.Add(credit);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(currentAccount);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act
            _service.ExecuteLoanMonthlyPayments();

            // Assert
            // Verify that the credit was ignored and its balance remains untouched
            Assert.Equal(CreditStatus.Active, credit.Status);

            // Verify that no money was transferred since the query shouldn't select this credit
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(It.IsAny<decimal>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<TransactionType>(), It.IsAny<CurrencyType>()),
                Times.Never);
        }
    }
}

