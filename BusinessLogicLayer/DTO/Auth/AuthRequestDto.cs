using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BusinessLogicLayer.DTO.Auth
{
    public class AuthRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Patronymic { get; set; }
        public string Surname { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public virtual PassportDto Passport { get; set; } = null!;

    }
}
