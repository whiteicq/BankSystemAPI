using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BusinessLogicLayer.DTO.Credit
{
    public record CreditRequestDto
    {
        public decimal SumOfLoan { get; set; }
        public int Term { get; set; }
        public decimal Interest { get; set; }
    }
}
