using CoreLayer.Actors;
using CoreLayer.Enums.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer.FinancialProducts
{
    public class FinancialProduct
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public float Rate { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public CurrencyType Currency { get; set; }
        [Required]
        public Client? Client { get; set; }
        public long ClientId { get; set; }
    }
}
