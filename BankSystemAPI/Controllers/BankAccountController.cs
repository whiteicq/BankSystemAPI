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

        [HttpPost]
        public IActionResult OpenBankAccount([FromBody] OpenBankAccountRequestDto requestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Пользователь не найден!");
            }
            long currentClientId = long.Parse(userIdClaim);

            BankAccount bankAccount = _bankAccountService.OpenBankAccount(currentClientId, requestDto.BankId, requestDto.bankAccountType);

            // manual mapping
            BankAccountResponseDto responseDto = new BankAccountResponseDto
            {
                BankAccountNumber = bankAccount.BankAccountNumber,
                MoneyBalance = bankAccount.MoneyBalance,
                Type = bankAccount.Type,
                Status = bankAccount.Status,
                ClientId = currentClientId
            };

            return Created(uri: string.Empty, responseDto);
        }
    }
}
