using BusinessLogicLayer.Exceptions.BankAccount;
using BusinessLogicLayer.Exceptions.Client;
using BusinessLogicLayer.Exceptions.Credit;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Transaction;
using Moq;

namespace BankSystemAPI.Tests.CreditServiceTests
{
    public class TransferMoneyForLoanTests : CreditServiceTests
    {
        [Fact]
        public void TransferMoneyForLoan_ValidData_ShouldTransferMoneyAndActivateCredit()
        {
            // Arrange
            Bank bank = CreateTestBank();

            // 1. Bank master account (ClientId must be null)
            BankAccount masterBankAccount = CreateTestBankAccount(id: 1L, clientId: null, bankId: bank.Id);

            // 2. Client's regular account to receive the loan funds
            BankAccount receiverAccount = CreateTestBankAccount(id: 2L, clientId: 1L, bankId: bank.Id);

            Client client = CreateTestClient(id: 1L);
            client.BankAccounts.Add(receiverAccount);

            // 3. Unactivated credit that is ready to be issued (CRITICAL FIX: status must be Unactivated)
            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id, loanAmount: 5000m, loanBalance: 6000m, status: CreditStatus.Unactivated);

            _context.Banks.Add(bank);
            _context.BankAccounts.AddRange(masterBankAccount, receiverAccount);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Setup the mock to return a simulated unique bank account number for the new credit account
            string expectedCreditCardNumber = "9999888877776666555544443333";
            _bankAccountServiceMock
                .Setup(m => m.GenerateUniqueBankAccountNumber(28))
                .Returns(expectedCreditCardNumber);

            // Act
            _service.TransferMoneyForLoan(client.Id, credit.Id, receiverAccount.Id);

            // Assert
            // Verify that the credit status was changed to Active
            Assert.Equal(CreditStatus.Active, credit.Status);

            // Verify that the private method OpenCreditBankAccount successfully created a new Credit BankAccount in DB
            BankAccount creditAccountFromDb = Assert.Single(_context.BankAccounts, ba => ba.Type == BankAccountType.Credit);
            Assert.Equal(expectedCreditCardNumber, creditAccountFromDb.BankAccountNumber);
            Assert.Equal(-6000m, creditAccountFromDb.MoneyBalance); // Debt balance should be negative

            // Verify that the external transaction service was called once with the correct parameters
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                5000m,
                masterBankAccount.Id,
                receiverAccount.Id,
                TransactionType.Credit
                ),
                Times.Once);
        }

        [Fact]
        public void TransferMoneyForLoan_ClientNotExists_ShouldThrowClientNotFoundException()
        {
            // Act & Assert (DB is empty, client with ID 999 does not exist)
            Assert.Throws<ClientNotFoundException>(() =>
                _service.TransferMoneyForLoan(clientId: 999L, creditId: 1L, bankAccountRecieverId: 1L)
            );
        }

        [Fact]
        public void TransferMoneyForLoan_ClientIsUnactive_ShouldThrowInvalidClientStatusException()
        {
            // Arrange
            // Create an unactive client (e.g., Blocked)
            Client inactiveClient = CreateTestClient(id: 1L, status: ClientStatus.Blocked);
            _context.Clients.Add(inactiveClient);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidClientStatusException>(() =>
                _service.TransferMoneyForLoan(inactiveClient.Id, creditId: 1L, bankAccountRecieverId: 1L)
            );
        }

        [Fact]
        public void TransferMoneyForLoan_CreditNotExists_ShouldThrowCreditNotFoundException()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L);
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act & Assert (Client exists, but credit with ID 999 does not exist for this client)
            Assert.Throws<CreditNotFoundException>(() =>
                _service.TransferMoneyForLoan(client.Id, creditId: 999L, bankAccountRecieverId: 1L)
            );
        }

        [Fact]
        public void TransferMoneyForLoan_CreditAlreadyActive_ShouldThrowInvalidClientStatusException()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L);
            // Create a credit that is already active (cannot transfer money twice)
            Credit activeCredit = CreateTestCredit(id: 10L, clientId: client.Id, status: CreditStatus.Active);

            _context.Clients.Add(client);
            _context.Credits.Add(activeCredit);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidCreditStatusException>(() =>
                _service.TransferMoneyForLoan(client.Id, activeCredit.Id, bankAccountRecieverId: 2L)
            );

            // Ensure that the transaction service was never invoked due to the validation failure
            _transactionServiceMock.Verify(t => t.SystemTransferMoney(
                It.IsAny<decimal>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<TransactionType>()
                ),
                Times.Never);
        }

        [Fact]
        public void TransferMoneyForLoan_ReceiverAccountNotExists_ShouldThrowBankAccountNotFoundException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient(id: 1L);
            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id);

            _context.Banks.Add(bank);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act & Assert (Receiver account with ID 999 does not exist in client's account collection)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _service.TransferMoneyForLoan(client.Id, credit.Id, bankAccountRecieverId: 999L)
            );
        }

        [Fact]
        public void TransferMoneyForLoan_ReceiverAccountIsInactive_ShouldThrowInvalidBankAccountStatusException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            // Create an inactive receiver account (e.g., Closed)
            BankAccount inactiveReceiver = CreateTestBankAccount(id: 2L, clientId: 1L, bankId: bank.Id, status: BankAccountStatus.Closed);

            Client client = CreateTestClient(id: 1L);
            client.BankAccounts.Add(inactiveReceiver);

            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(inactiveReceiver);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidBankAccountStatusException>(() =>
                _service.TransferMoneyForLoan(client.Id, credit.Id, inactiveReceiver.Id)
            );
        }

        [Fact]
        public void TransferMoneyForLoan_MasterAccountNotExists_ShouldThrowBankAccountNotFoundException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            BankAccount receiverAccount = CreateTestBankAccount(id: 2L, clientId: 1L, bankId: bank.Id);
            Client client = CreateTestClient(id: 1L);
            client.BankAccounts.Add(receiverAccount);
            Credit credit = CreateTestCredit(id: 10L, clientId: client.Id, bankId: bank.Id);

            _context.Banks.Add(bank);
            _context.BankAccounts.Add(receiverAccount);
            _context.Clients.Add(client);
            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act & Assert (Master bank account with ClientId == null is missing from the database)
            Assert.Throws<BankAccountNotFoundException>(() =>
                _service.TransferMoneyForLoan(client.Id, credit.Id, receiverAccount.Id)
            );
        } 
    }
}
