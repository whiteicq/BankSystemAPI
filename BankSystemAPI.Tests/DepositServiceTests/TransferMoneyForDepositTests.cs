using BusinessLogicLayer.Exceptions.BankAccount;
using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Exceptions.Deposit;
using BusinessLogicLayer.Exceptions.Transaction;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using DataAccessLayer.Enums.Transaction;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.DepositServiceTests
{
    public class TransferMoneyForDepositTests : DepositServiceTests
    {
        [Fact]
        public void TransferMoneyForDeposit_ValidData_ShouldTransferMoneyAndActivateDeposit()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);

            // Sender account must be Current, Active, and have enough money (e.g., 10000 for a 5000 deposit)
            BankAccount senderAccount = CreateTestBankAccount(id: 10L, clientId: client.Id, bankId: bank.Id, moneyBalance: 10000m, type: BankAccountType.Current);
            client.BankAccounts.Add(senderAccount);

            // Deposit must be Unactivated so that LocalValidator.IsActive returns false
            Deposit deposit = CreateTestDeposit(id: 50L, clientId: client.Id, bankId: bank.Id, depositAmount: 5000m, status: DepositStatus.Unactivated);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(senderAccount);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Setup the mock for internal account number generation inside OpenDepositBankAccount
            string expectedDepositAccountNumber = "DEPOSIT-ACCOUNT-NUMBER-777";
            _bankAccountServiceMock
                .Setup(m => m.GenerateUniqueBankAccountNumber(28))
                .Returns(expectedDepositAccountNumber);

            // Act
            _depositService.TransferMoneyForDeposit(client.Id, deposit.Id, senderAccount.Id);

            // Assert
            // Verify that the deposit status was updated to Active
            Assert.Equal(DepositStatus.Active, deposit.Status);

            // Verify that the transaction service was invoked to transfer funds from sender to the new deposit account
            // Note: Since OpenDepositBankAccount generates a new account, we find its ID dynamically from DB
            BankAccount depositAccountFromDb = Assert.Single(_context.BankAccounts, ba => ba.Type == BankAccountType.Deposit);
            Assert.Equal(expectedDepositAccountNumber, depositAccountFromDb.BankAccountNumber);

            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                5000m,
                senderAccount.Id,
                depositAccountFromDb.Id,
                TransactionType.Deposit
                ),
                Times.Once);
        }

        [Fact]
        public void TransferMoneyForDeposit_ClientNotExists_ShouldThrowClientNotFoundException()
        {
            // Act & Assert (DB is empty, so client with ID 999 triggers the exception)
            Assert.Throws<ClientNotFoundException>(() =>
                _depositService.TransferMoneyForDeposit(clientId: 999L, depositId: 1L, bankAccountSenderId: 1L)
            );
        }

        [Fact]
        public void TransferMoneyForDeposit_ClientIsInactive_ShouldThrowInvalidClientStatusException()
        {
            // Arrange
            // Create an inactive client (e.g., Blocked)
            Client inactiveClient = CreateTestClient(id: 1L, status: ClientStatus.Blocked);
            _context.Clients.Add(inactiveClient);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidClientStatusException>(() =>
                _depositService.TransferMoneyForDeposit(inactiveClient.Id, depositId: 1L, bankAccountSenderId: 1L)
            );
        }

        [Fact]
        public void TransferMoneyForDeposit_DepositNotExists_ShouldThrowDepositNotFoundException()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L);
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act & Assert (Client exists, but deposit with ID 999 does not exist for this client)
            Assert.Throws<DepositNotFoundException>(() =>
                _depositService.TransferMoneyForDeposit(client.Id, depositId: 999L, bankAccountSenderId: 1L)
            );
        }

        [Fact]
        public void TransferMoneyForDeposit_DepositAlreadyActive_ShouldThrowInvalidDepositStatusException()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L);
            // Create a deposit that is already active (cannot activate it twice)
            Deposit activeDeposit = CreateTestDeposit(id: 50L, clientId: client.Id, status: DepositStatus.Active);

            _context.Clients.Add(client);
            _context.Deposits.Add(activeDeposit);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidDepositStatusException>(() =>
                _depositService.TransferMoneyForDeposit(client.Id, activeDeposit.Id, bankAccountSenderId: 1L)
            );

            // Ensure transaction service was never called due to early validation failure
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                It.IsAny<decimal>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<TransactionType>()
                ),
                Times.Never);
        }

        [Fact]
        public void TransferMoneyForDeposit_SenderAccountNotExists_ShouldThrowBankAccountNotFoundException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);
            Deposit deposit = CreateTestDeposit(id: 50L, clientId: client.Id, bankId: bank.Id, status: DepositStatus.Unactivated);

            _context.Banks.Add(bank);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Act & Assert (Sender account with ID 999 does not exist in client's account collection)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _depositService.TransferMoneyForDeposit(client.Id, deposit.Id, bankAccountSenderId: 999L)
            );
        }

        [Fact]
        public void TransferMoneyForDeposit_SenderAccountIsInactive_ShouldThrowInvalidBankAccountStatusException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);

            // Create an inactive sender account (e.g., Closed)
            BankAccount inactiveSender = CreateTestBankAccount(id: 10L, clientId: client.Id, bankId: bank.Id, status: BankAccountStatus.Closed);
            client.BankAccounts.Add(inactiveSender);

            Deposit deposit = CreateTestDeposit(id: 50L, clientId: client.Id, bankId: bank.Id, status: DepositStatus.Unactivated);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(inactiveSender);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidBankAccountStatusException>(() =>
                _depositService.TransferMoneyForDeposit(client.Id, deposit.Id, inactiveSender.Id)
            );
        }

        [Fact]
        public void TransferMoneyForDeposit_InsufficientFunds_ShouldThrowInsufficientFundsException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);

            // Sender account has only 1000m, but deposit amount requires 5000m
            BankAccount poorSender = CreateTestBankAccount(id: 10L, clientId: client.Id, bankId: bank.Id, moneyBalance: 1000m, type: BankAccountType.Current);
            client.BankAccounts.Add(poorSender);

            Deposit deposit = CreateTestDeposit(id: 50L, clientId: client.Id, bankId: bank.Id, depositAmount: 5000m, status: DepositStatus.Unactivated);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(poorSender);
            _context.Clients.Add(client);
            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InsufficientFundsException>(() =>
                _depositService.TransferMoneyForDeposit(client.Id, deposit.Id, poorSender.Id)
            );

            // Ensure transaction service was never called due to insufficient funds
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                It.IsAny<decimal>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<TransactionType>()
                ),
                Times.Never);
        }

    }
}
