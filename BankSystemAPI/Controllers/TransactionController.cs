using BusinessLogicLayer.DTO.Transaction;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.Claims;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly DbContext _context;
        
        public TransactionController(ITransactionService transactionService, DbContext context)
        {
            _transactionService = transactionService;
            _context = context;
        }

        [HttpPost("transfer")]
        public IActionResult TransferMoney([FromBody] TransactionRequestDto transactionRequestDto)
        {
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userClaimId))
            {
                return Unauthorized("Пользователь не найден");
            }

            long currentUserId = long.Parse(userClaimId);

            Transaction completedTransaction = _transactionService.TransferMoney(currentUserId, transactionRequestDto.Amount, transactionRequestDto.SenderBankAccountNumber, transactionRequestDto.RecieverBankAccountNumber);

            TransactionResponseDto transactionResponseDto = new TransactionResponseDto
            {
                TransactionAmount = completedTransaction.TransactionAmount,
                SenderBankAccountNumber = completedTransaction.Sender.BankAccountNumber,
                ReceiverBankAccountNumber = completedTransaction.Receiver.BankAccountNumber,
                Type = completedTransaction.Type,
                Currency = completedTransaction.Currency,
                CreatedAt = completedTransaction.CreatedAt
            };
            return Ok(transactionResponseDto);
        }
    }
}
