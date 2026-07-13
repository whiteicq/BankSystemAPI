using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer.Actors
{
    public class User
    {
        public long Id { get; set; }
        [Required]
        [MaxLength(25)]
        public string? Name { get; set; }
        [MaxLength(25)]
        public string? Patronymic { get; set; }
        [Required]
        [MaxLength(25)]
        public string? Surname { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        [Required]
        public Passport? Passport { get; set; }
        
    }
}
