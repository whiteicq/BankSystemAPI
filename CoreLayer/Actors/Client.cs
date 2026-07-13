using System;
using System.Collections.Generic;
using System.Text;
using CoreLayer.Enums.Client;
using CoreLayer.FinancialProducts;
namespace CoreLayer.Actors
{
    public class Client : User
    {
        public ICollection<BankAccount>? BankAccounts { get; set; }
        //public ICollection<FinancialProduct> FinancialProducts { get; set; }
        public ClientStatus Status { get; set; }
        public ICollection<Credit>? Credits { get; set; }
        public ICollection<Deposit>? Deposits { get; set; }
    }
}
