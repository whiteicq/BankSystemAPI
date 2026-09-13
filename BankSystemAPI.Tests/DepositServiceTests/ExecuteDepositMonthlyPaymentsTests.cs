using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using DataAccessLayer.Enums.Transaction;
using Moq;

namespace BankSystemAPI.Tests.DepositServiceTests
{
    public class ExecuteDepositMonthlyPaymentsTests : DepositServiceTests
    {
        [Fact]
        public void ExecuteDepositMonthlyPayments_RegularMonth_ShouldAccrueInterestOnly()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // 1. Bank master account (source of interest payments)
            BankAccount masterAccount = CreateTestBankAccount(id: 10L, clientId: null, bankId: bank.Id);

            // 2. Client's deposit account
            BankAccount depositAccount = CreateTestBankAccount(id: 20L, clientId: client.Id, bankId: bank.Id, moneyBalance: 5000m, type: BankAccountType.Deposit);

            // 3. Active deposit (opened recently, so expirationDate is far in the future)
            Deposit deposit = CreateTestDeposit(id: 100L, clientId: client.Id, bankId: bank.Id, depositAmount: 5000m, depositTerm: 12, depositInterest: 12m, status: DepositStatus.Active);
            deposit.OpenedAt = today.AddMonths(-1); // Opened 1 month ago, day matches today
            deposit.BankAccount = depositAccount;

            _context.Banks.Add(bank);
            _context.BankAccounts.AddRange(masterAccount, depositAccount);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // CRITICAL FIX: Dynamically calculate the exact interest for the current month to match your formula
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int daysInYear = DateTime.IsLeapYear(today.Year) ? 366 : 365;
            decimal expectedInterest = Math.Round(5000m * (12m / 100m) * daysInMonth / daysInYear, 2, MidpointRounding.ToEven);

            // Act
            _depositService.ExecuteDepositMonthlyPayments();

            // Assert
            // Verify that the deposit status remains Active since it hasn't expired yet
            Assert.Equal(DepositStatus.Active, deposit.Status);

            // Verify that the transaction service transferred the monthly interest from master to deposit account
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                expectedInterest, // Matches exactly what your service computes (49.32m for September)
                masterAccount.Id,
                depositAccount.Id,
                TransactionType.Deposit,
                It.IsAny<CurrencyType>()
                ),
                Times.Once);

            // Verify that the final payout logic was NOT triggered (deposit account shouldn't be closed)
            _bankAccountServiceMock.Verify(s => s.SystemCloseBankAccount(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public void ExecuteDepositMonthlyPayments_ExpirationMonth_ShouldAccrueInterestAndReturnFundsToClient()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            BankAccount masterAccount = CreateTestBankAccount(id: 10L, clientId: null, bankId: bank.Id);

            // Deposit account currently holds 5000m
            BankAccount depositAccount = CreateTestBankAccount(id: 20L, clientId: client.Id, bankId: bank.Id, moneyBalance: 5000m, type: BankAccountType.Deposit);

            // Client's active current account where expired deposit money will be sent
            BankAccount clientCurrentAccount = CreateTestBankAccount(id: 30L, clientId: client.Id, bankId: bank.Id, moneyBalance: 0m, type: BankAccountType.Current);
            client.BankAccounts.Add(clientCurrentAccount);

            // Deposit is set up so that it expires exactly TODAY (Opened N months ago based on term)
            int term = 6;
            Deposit deposit = CreateTestDeposit(id: 100L, clientId: client.Id, bankId: bank.Id, depositAmount: 5000m, depositTerm: term, depositInterest: 12m, status: DepositStatus.Active);
            deposit.OpenedAt = today.AddMonths(-term); // expirationDate will be exactly today
            deposit.BankAccount = depositAccount;

            _context.Banks.Add(bank);
            _context.BankAccounts.AddRange(masterAccount, depositAccount, clientCurrentAccount);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int daysInYear = DateTime.IsLeapYear(today.Year) ? 366 : 365;
            decimal expectedInterest = Math.Round(5000m * (12m / 100m) * daysInMonth / daysInYear, 2, MidpointRounding.ToEven);

            // Act
            _depositService.ExecuteDepositMonthlyPayments();

            // Assert
            // 1. Verify deposit properties updated correctly
            Assert.Equal(DepositStatus.Closed, deposit.Status);
            Assert.Equal(today, deposit.ClosedAt);

            // 2. Verify regular monthly interest accrual still happened
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                expectedInterest,
                masterAccount.Id,
                depositAccount.Id,
                TransactionType.Deposit,
                It.IsAny<CurrencyType>()
                ),
                Times.Once);

            // 3. Verify total deposit balance was transferred from deposit account to client's current account
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                depositAccount.MoneyBalance, // Total remaining amount on deposit card
                depositAccount.Id,
                clientCurrentAccount.Id,
                TransactionType.Deposit,
                It.IsAny<CurrencyType>()
                ),
                Times.Once);

            // 4. Verify that the bank account service was called to close the deposit bank account
            _bankAccountServiceMock.Verify(s => s.SystemCloseBankAccount(depositAccount.Id), Times.Once);
        }

        [Fact]
        public void ExecuteDepositMonthlyPayments_ExpiredButClientHasNoCurrentAccount_ShouldRollbackAndNotClose()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L); // Client has NO active current accounts in DB
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            BankAccount masterAccount = CreateTestBankAccount(id: 10L, clientId: null, bankId: bank.Id);
            BankAccount depositAccount = CreateTestBankAccount(id: 20L, clientId: client.Id, bankId: bank.Id, moneyBalance: 5000m, type: BankAccountType.Deposit);

            int term = 12;
            Deposit deposit = CreateTestDeposit(id: 100L, clientId: client.Id, bankId: bank.Id, depositTerm: term, status: DepositStatus.Active);
            deposit.OpenedAt = today.AddMonths(-term); // Forces expiration flow
            deposit.BankAccount = depositAccount;

            _context.Banks.Add(bank);
            _context.BankAccounts.AddRange(masterAccount, depositAccount);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Act
            _depositService.ExecuteDepositMonthlyPayments();

            // Assert
            // Due to "throw new BankAccountNotFoundException", the catch block should trigger Rollback.
            // The deposit status must safely remain Active (changes rolled back)
            var depositFromDb = _context.Deposits.Find(deposit.Id);
            Assert.Equal(DepositStatus.Active, depositFromDb!.Status);

            // The bank account service must never be called to close the account since the operation aborted
            _bankAccountServiceMock.Verify(s => s.SystemCloseBankAccount(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public void ExecuteDepositMonthlyPayments_WrongDayOfRecord_ShouldSkipProcessing()
        {
            // Arrange
            Bank bank = CreateTestBank();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // Pick a non-matching day of the month to ensure filtering works correctly
            int nonMatchingDay = today.Day == 1 ? 2 : today.Day - 1;
            DateOnly wrongDate = new DateOnly(today.Year, today.Month, nonMatchingDay);

            Client client = CreateTestClient(id: 1L);
            BankAccount depositAccount = CreateTestBankAccount(id: 20L, clientId: client.Id, bankId: bank.Id, type: BankAccountType.Deposit);

            Deposit deposit = CreateTestDeposit(id: 100L, clientId: client.Id, bankId: bank.Id, status: DepositStatus.Active);
            deposit.OpenedAt = wrongDate; // This day mismatch should filter out the record
            deposit.BankAccount = depositAccount;

            _context.Banks.Add(bank); _context.BankAccounts.Add(depositAccount); _context.Clients.Add(client); _context.Deposits.Add(deposit); _context.SaveChanges();
            
            // Act
            _depositService.ExecuteDepositMonthlyPayments();
            
            // Assert
            Assert.Equal(DepositStatus.Active, deposit.Status);
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(It.IsAny<decimal>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<TransactionType>(), It.IsAny<CurrencyType>()),Times.Never);
        }
    }
}
