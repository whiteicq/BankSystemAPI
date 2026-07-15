using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface IDepositService
    {
        void RequestDeposit(long clientId, decimal sumOfDeposit, int term, decimal interest);
        void TransferMoneyForDeposit(long clientId, long depositId, long bankAccountSenderId);
        void ExecuteDepositMonthlyPayments();
    }
}
