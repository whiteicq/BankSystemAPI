using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTO
{
    public class PassportDto
    {
        public string IdentificationNumber { get; set; } = null!;
        public string Series { get; set; } = null!;
        public string Number { get; set; } = null!;
    }
}