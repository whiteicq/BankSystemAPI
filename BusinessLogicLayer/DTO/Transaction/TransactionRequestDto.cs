using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;

namespace BusinessLogicLayer.DTO.Transaction
{
    public class TransactionRequestDto
    {
        public decimal Amount { get; set; }
        public string SenderBankAccountNumber { get; set; } = null!;
        public string RecieverBankAccountNumber { get; set; } = null!;
        public TransactionType TransactionType { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
