using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer.Actors
{
    public class Employee : User
    {
        [Required]
        public string? PersonellNumber { get; set; }
        [Required]
        public Bank? Bank { get; set; }
        public long BankId { get; set; }
        public ICollection<Log>? Logs { get; set; }
    }
}
