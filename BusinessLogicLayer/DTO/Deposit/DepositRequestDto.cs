using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO.Deposit
{
    public class DepositRequestDto
    {
        public decimal DepositAmount{ get; set;}
        public int DepositTerm { get; set; } 
        public decimal DepositInterest { get; set; }
    }
}
