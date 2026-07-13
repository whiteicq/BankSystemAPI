using CoreLayer.Actors;
using CoreLayer.FinancialProducts;
using System.ComponentModel.DataAnnotations;

namespace CoreLayer
{
    public class Bank
    {
        public long Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string? Title { get; set; }
        [Required]
        public string? BIC { get; set; }
        [Required]
        [MaxLength(35)]
        public string? Adress { get; set; }
        //public ICollection<User>? Users { get; set; }
        public ICollection<BankAccount>? BankAccounts { get; set; }
        public ICollection<Employee>? Employees { get; set; }
        public ICollection<Credit>? Credits { get; set; }
        public ICollection<Deposit>? Deposits { get; set; }

    }
}
