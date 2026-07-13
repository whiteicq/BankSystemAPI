using CoreLayer.Actors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer
{
    public class Passport
    {
        public long Id { get; set; }
        [Required]
        public string? Series { get; set; }
        [Required]
        public string? Number { get; set; }
        [Required]
        public string? IdentificationNumber { get; set; }
        public Client? Client { get; set; }
        public long? ClientId { get; set; }
        public Employee? Employee { get; set; }
        public long? EmployeeId { get; set; }
    }
}
