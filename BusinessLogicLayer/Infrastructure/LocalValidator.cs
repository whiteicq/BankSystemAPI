using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.FinancialProduct.Credit;

namespace BusinessLogicLayer.Infrastructure
{
    static class LocalValidator
    {
        public static bool IsActive(Client client)
        {
            return client.Status == ClientStatus.Active;
        }

        public static bool IsActive(BankAccount bankAccount)
        {
            return bankAccount.Status == BankAccountStatus.Active;
        }

        public static bool IsActive(Credit credit)
        {
            return credit.Status == CreditStatus.Active;
        }
    }
}
