namespace BusinessLogicLayer.Interfaces
{
    public interface ICreditService
    {
        void RequestCredit(long clientId, decimal sumOfLoan, int term, decimal interest);
        void TransferMoneyForLoan(long clientId, long creditId, long bankAccountId);
        void ExecuteLoanMonthlyPayments();
    }
}