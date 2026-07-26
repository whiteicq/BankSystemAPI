using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IBankAccountService
    {
        BankAccount OpenBankAccount(long clientId, long bankId, BankAccountType bankAccountType);
        void CloseBankAccount(long bankAccountId);
        string GenerateUniqueBankAccountNumber(int length);
    }
}
