using BusinessLogicLayer.Exceptions.Deposit;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class RejectDepositTests : EmployeeServiceTests
    {
        [Fact]
        public void RejectDeposit_ValidUnactivatedDeposit_ShouldRejectAndLog()
        {
            // Arrange
            // Create an unactivated deposit (it is neither Closed nor Rejected)
            Deposit deposit = CreateTestDeposit(id: 10L, status: DepositStatus.Unactivated);

            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Act
            _employeeService.RejectDeposit(deposit.Id);

            // Assert
            // Verify that the status inside the tracked entity object changed to Rejected
            Assert.Equal(DepositStatus.Rejected, deposit.Status);

            // Verify that changes were successfully saved to the InMemory database
            Deposit depositFromDb = _context.Deposits.Find(deposit.Id);
            Assert.NotNull(depositFromDb);
            Assert.Equal(DepositStatus.Rejected, depositFromDb.Status);

            // Verify that the logger was triggered exactly once after SaveChanges successfully executed
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.DEPOSIT_REJECTED,
                nameof(Deposit),
                deposit.Id,
                DepositStatus.Active.ToString(),
                DepositStatus.Rejected.ToString()
                ),
                Times.Once);
        }

        [Theory]
        [InlineData(DepositStatus.Rejected)] // Case 1: Deposit is already rejected
        [InlineData(DepositStatus.Closed)]   // Case 2: Deposit is already closed
        public void RejectDeposit_DepositIsAlreadyClosedOrRejected_ShouldThrowDepositNotFoundException(DepositStatus invalidStatus)
        {
            // Arrange
            // Create a deposit with a status that should be filtered out by the FirstOrDefault query
            Deposit deposit = CreateTestDeposit(id: 10L, status: invalidStatus);

            _context.Deposits.Add(deposit);
            _context.SaveChanges();

            // Act & Assert
            // The service should fail to find the deposit and throw DepositNotFoundException
            Assert.Throws<DepositNotFoundException>(() =>
                _employeeService.RejectDeposit(deposit.Id)
            );

            // Verify that the status inside the database remains completely unchanged
            Assert.Equal(invalidStatus, deposit.Status);

            // Verify that the logger was never invoked since execution failed early
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }

        [Fact]
        public void RejectDeposit_DepositDoesNotExist_ShouldThrowDepositNotFoundException()
        {
            // Act & Assert (Database is empty, searching for non-existent ID 999 must throw exception)
            Assert.Throws<DepositNotFoundException>(() =>
                _employeeService.RejectDeposit(depositId: 999L)
            );

            // Verify that execution aborted before hitting the log method
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }
    }
}
