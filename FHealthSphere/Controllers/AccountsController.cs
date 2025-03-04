using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.AccountModelViews;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Authorize] // Thêm authorization nếu cần
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: api/Accounts?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<Account>>> GetAllAccounts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var accounts = await _accountService.GetAllAccounts(pageNumber, pageSize);
                return Ok(BaseResponse<BasePaginatedList<Account>>.OkResponse(accounts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving Accounts: {ex.Message}");
            }
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var account = await _accountService.CreateAccount(model);
                return Ok(BaseResponse<Account>.OkResponse(account));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create Account: {ex.Message}");
            }
        }

        // PUT: api/Accounts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Account>> UpdateAccount(int id, [FromBody] UpdateAccountModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var account = await _accountService.UpdateAccount(id, model);
                return Ok(BaseResponse<Account>.OkResponse(account));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update Account with ID {id}: {ex.Message}");
            }
        }

        // DELETE: api/Accounts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var result = await _accountService.DeleteAccount(id);
                if (!result)
                {
                    return NotFound($"Account with ID {id} not found or already deleted.");
                }
                return Ok($"Account with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete Account with ID {id}: {ex.Message}");
            }
        }
    }
}