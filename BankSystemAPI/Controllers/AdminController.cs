using BusinessLogicLayer.DTO.Auth;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registerEmployee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] AuthRequestDto registerDto)
        {
            await _authService.RegisterEmployeeAsync(registerDto.Email, registerDto.Password, registerDto.Name, registerDto.Patronymic, registerDto.Surname, registerDto.PhoneNumber, registerDto.BirthDate, registerDto.Passport.IdentificationNumber, registerDto.Passport.Series, registerDto.Passport.Number);

            return Ok();
        }
    }
}
