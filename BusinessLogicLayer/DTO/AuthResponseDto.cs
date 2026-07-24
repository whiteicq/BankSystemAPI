using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; } = null!;
        public string ErrorMessage { get; set; } = null!;
    }
}
