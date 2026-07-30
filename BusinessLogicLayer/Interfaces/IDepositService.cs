using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IDepositService
    {
        Deposit RequestDeposit(long userId, decimal sumOfDeposit, int term, decimal interest);
        void TransferMoneyForDeposit(long clientId, long depositId, long bankAccountSenderId);
        void ExecuteDepositMonthlyPayments();
    }
}
