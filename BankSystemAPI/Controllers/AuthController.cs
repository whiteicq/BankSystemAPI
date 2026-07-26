using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BusinessLogicLayer.DTO.Auth;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterClient([FromBody] AuthRequestDto registerDto)
        {
            await _authService.RegisterClientAsync(registerDto.Email, registerDto.Password, registerDto.Name, registerDto.Patronymic, registerDto.Surname, registerDto.PhoneNumber, registerDto.BirthDate, registerDto.Passport.IdentificationNumber, registerDto.Passport.Series, registerDto.Passport.Number);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            string token = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

            if (token == string.Empty)
            {
                return Unauthorized(token);
            }

            return Ok(token);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();

            return Ok();
        }
    }
}
