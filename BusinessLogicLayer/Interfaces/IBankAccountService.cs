using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IBankAccountService
    {
        BankAccount OpenBankAccount(long usertId, long bankId, BankAccountType bankAccountType);
        void CloseBankAccount(long userId, string bankAccountNumber);
        void SystemCloseBankAccount(long bankAccountId);
        string GenerateUniqueBankAccountNumber(int length);
    }
}
