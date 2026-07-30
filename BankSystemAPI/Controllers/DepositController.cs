using BusinessLogicLayer.DTO.Deposit;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataAccessLayer.Entities;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        private readonly IDepositService _depositService;

        public DepositController(IDepositService depositService)
        {
            _depositService = depositService;
        }

        [HttpPost("depositRequest")]
        public IActionResult CreateDepositRequeste([FromBody] DepositRequestDto depositRequestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null)
            {
                return Unauthorized("Пользователь не найден");
            }

            long currentUserId = long.Parse(userIdClaim);

            Deposit currentDeposit = _depositService.RequestDeposit(currentUserId, depositRequestDto.DepositAmount, depositRequestDto.DepositTerm, depositRequestDto.DepositInterest);

            DepositResponseDto depositResponseDto = new DepositResponseDto
            {
                DepositAmount = currentDeposit.DepositAmount,
                DepositTerm = currentDeposit.DepositTerm,
                DepositInterest = currentDeposit.DepositInterest,
                Status = currentDeposit.Status,
                ClientId = currentDeposit.ClientId,
                OpenedAt = currentDeposit.OpenedAt
            };

            return Created(uri: string.Empty, depositResponseDto);
        }
    }
}
