using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Employee, Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPatch("activateClient")]
        public IActionResult ActivateClient(long clientId)
        {
            _employeeService.ActivateClient(clientId);

            return Ok();
        }

        [HttpPatch("activateBankAccount")]
        public IActionResult ActivateBankAccount(long bankAccountId)
        {
            _employeeService.ActivateBankAccount(bankAccountId);

            return Ok();
        }

        [HttpPatch("activateCredit")]
        public IActionResult ActivateCredit(long clientId, long creditId, long bankAccountRecieverId)
        {
            _employeeService.ActivateCredit(clientId, creditId, bankAccountRecieverId);

            return Ok();
        }

        [HttpPatch("activateDeposit")]
        public IActionResult ActivateDeposit(long clientId, long depositId, long bankAccountSenderId)
        {
            _employeeService.ActivateDeposit(clientId, depositId, bankAccountSenderId);

            return Ok();
        }

        [HttpPatch("blockBankAccount")]
        public IActionResult BlockBankAccount(long bankAccountId)
        {
            _employeeService.BlockBankAccount(bankAccountId);

            return Ok();
        }

        [HttpPatch("blockClient")]
        public IActionResult BlockClient(long clientId)
        {
            _employeeService.BlockClient(clientId);

            return Ok();
        }

        [HttpPatch("cancelTransaction")]
        public IActionResult CancelTransaction(long transactionId)
        {
            _employeeService.CancelTransaction(transactionId);

            return Ok();
        }

        [HttpPatch("closeBankAccount")]
        public IActionResult CloseBankAccount(long bankAccountId)
        {
            _employeeService.CloseBankAccount(bankAccountId);

            return Ok();
        }

        [HttpPatch("freezeBankAccount")]
        public IActionResult FreezeBankAccount(long bankAccountId)
        {
            _employeeService.FreezeBankAccount(bankAccountId);

            return Ok();
        }

        [HttpPatch("rejectedCredit")]
        public IActionResult RejectCredit(long creditId)
        {
            _employeeService.RejectCredit(creditId);

            return Ok();
        }

        [HttpPatch("rejectDeposit")]
        public IActionResult RejectDeposit(long depositId)
        {
            _employeeService.RejectDeposit(depositId);

            return Ok();
        }
    }
}
