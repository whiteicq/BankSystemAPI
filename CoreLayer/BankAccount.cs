using CoreLayer.Enums.BankAccount;
using CoreLayer.Enums.Common;
using CoreLayer.Actors;
using System.ComponentModel.DataAnnotations;

namespace CoreLayer
{
    public class BankAccount
    {
        public long Id { get; set; }
        public decimal Balance { get; set; }
        [Required]
        public string? BankAccountNumber { get; set; }
        public BankAccountType TypeAccount { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public BankAccountStatus Status { get; set; }
        public CurrencyType Currency { get; set; }
        [Required]
        public Client? Client { get; set; }
        public long ClientId { get; set; }
        [Required]
        public Bank? Bank { get; set; }
        public long BankId { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
