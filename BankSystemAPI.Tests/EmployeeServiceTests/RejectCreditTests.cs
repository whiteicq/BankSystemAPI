using BusinessLogicLayer.Exceptions.Credit;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class RejectCreditTests : EmployeeServiceTests
    {
        [Fact]
        public void RejectCredit_ValidUnactivatedCredit_ShouldRejectAndLog()
        {
            // Arrange
            // Create an unactivated credit (it is neither Closed nor Rejected)
            Credit credit = CreateTestCredit(id: 10L, status: CreditStatus.Unactivated);

            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act
            _employeeService.RejectCredit(credit.Id);

            // Assert
            // Verify that the status inside the tracked entity object changed to Rejected
            Assert.Equal(CreditStatus.Rejected, credit.Status);

            // Verify that changes were successfully saved to the InMemory database (now happens BEFORE logging)
            Credit creditFromDb = _context.Credits.Find(credit.Id);
            Assert.NotNull(creditFromDb);
            Assert.Equal(CreditStatus.Rejected, creditFromDb.Status);

            // Verify that the logger was triggered exactly once after SaveChanges successfully executed
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.CREDIT_REJECTED,
                nameof(Credit),
                credit.Id,
                CreditStatus.Active.ToString(),
                CreditStatus.Rejected.ToString()
                ),
                Times.Once);
        }

        [Theory]
        [InlineData(CreditStatus.Rejected)] // Case 1: Credit is already rejected
        [InlineData(CreditStatus.Closed)]   // Case 2: Credit is already closed
        public void RejectCredit_CreditIsAlreadyClosedOrRejected_ShouldThrowCreditNotFoundException(CreditStatus invalidStatus)
        {
            // Arrange
            // Create a credit with a status that should be filtered out by the FirstOrDefault query
            Credit credit = CreateTestCredit(id: 10L, status: invalidStatus);

            _context.Credits.Add(credit);
            _context.SaveChanges();

            // Act & Assert
            // The service should fail to find the credit and throw CreditNotFoundException
            Assert.Throws<CreditNotFoundException>(() =>
                _employeeService.RejectCredit(credit.Id)
            );

            // Verify that the status inside the database remains completely unchanged
            Assert.Equal(invalidStatus, credit.Status);

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
        public void RejectCredit_CreditDoesNotExist_ShouldThrowCreditNotFoundException()
        {
            // Act & Assert (Database is empty, searching for non-existent ID 999 must throw exception)
            Assert.Throws<CreditNotFoundException>(() =>
                _employeeService.RejectCredit(creditId: 999L)
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
