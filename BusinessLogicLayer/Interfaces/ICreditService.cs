using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface ICreditService
    {
        Credit RequestCredit(long userId, long bankId, decimal sumOfLoan, int term, decimal interest);
        void TransferMoneyForLoan(long clientId, long creditId, long bankAccountId);
        void ExecuteLoanMonthlyPayments();
    }
}