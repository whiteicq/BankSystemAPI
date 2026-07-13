using DataAccessLayer.Enums.BankAccount;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface IBankAccountService
    {
        void OpenBankAccount(long clientId, long bankId, BankAccountType bankAccountType);
        void CloseBankAccount(long bankAccountId);
        string GenerateUniqueBankAccountNumber(int length);
    }
}
