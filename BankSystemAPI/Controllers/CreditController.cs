using DataAccessLayer.Entities;
using BusinessLogicLayer.DTO.Credit;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Client")]
    public class CreditController : ControllerBase
    {
        private readonly ICreditService _creditService;

        public CreditController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        [HttpPost("creditRequest")]
        public IActionResult CreateCreditRequest([FromBody] CreditRequestDto creditRequestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null)
            {
                return Unauthorized("Пользователь не найден");
            }

            long currentUserId = long.Parse(userIdClaim);

            Credit creditRequest = _creditService.RequestCredit(currentUserId, creditRequestDto.SumOfLoan, creditRequestDto.Term, creditRequestDto.Interest);

            CreditResponseDto creditResponseDto = new CreditResponseDto
            {
                LoanAmount = creditRequest.LoanAmount,
                LoanBalance = creditRequest.LoanBalance,
                LoanInterest = creditRequest.LoanInterest,
                LoanTerm = creditRequest.LoanTerm,
                OpenedAt = creditRequest.OpenedAt,
                Currency = creditRequest.Currency,
                Status = creditRequest.Status,
                ClientId = creditRequest.ClientId
            };

            return Created("", creditResponseDto);
        }
    }
}
