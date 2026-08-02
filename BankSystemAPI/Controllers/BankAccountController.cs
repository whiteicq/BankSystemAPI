using BusinessLogicLayer.Services;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using BusinessLogicLayer.DTO.BankAccount;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Diagnostics.Eventing.Reader;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpPost("open")]
        public IActionResult OpenBankAccount([FromBody] OpenBankAccountRequestDto requestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Пользователь не найден!");
            }

            long currentUserId = long.Parse(userIdClaim);

            BankAccount bankAccount = _bankAccountService.OpenBankAccount(currentUserId, requestDto.BankId, requestDto.bankAccountType);

            // manual mapping
            BankAccountResponseDto responseDto = new BankAccountResponseDto
            {
                BankAccountNumber = bankAccount.BankAccountNumber,
                MoneyBalance = bankAccount.MoneyBalance,
                Type = bankAccount.Type,
                Status = bankAccount.Status,
                ClientId = bankAccount.ClientId
            };

            return Created(uri: string.Empty, responseDto);
        }

        [HttpPatch("close")]
        public IActionResult CloseBankAccount(string bankAccountNumber)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Пользователь не найден!");
            }

            long currentUserId = long.Parse(userIdClaim);

            _bankAccountService.CloseBankAccount(currentUserId, bankAccountNumber);

            return Ok();
        }
    }
}
