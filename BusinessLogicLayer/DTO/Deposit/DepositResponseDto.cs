using DataAccessLayer.Enums.FinancialProduct.Deposit;

namespace BusinessLogicLayer.DTO.Deposit
{
    public class DepositResponseDto
    {
        public decimal DepositAmount { get; set; }

        public int DepositTerm { get; set; }

        public decimal DepositInterest { get; set; }

        public DepositStatus Status { get; set; }

        public long ClientId { get; set; }

        public DateOnly OpenedAt { get; set; }

        public long BankId { get; set; }
    }
}
