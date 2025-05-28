using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Models;
using Trainacc.Filters;
using Trainacc.Services;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _service;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UsersService service, ILogger<UsersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? id = null, string? email = null, string? password = null)
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetUserAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    var user = await _service.GetUserByCredentialsAsync(email, password);
                    if (user == null)
                        return NotFound("Пользователь с такими данными не найден или пароль неверный.");
                    return Ok(new { id = user?.Id });
                }
                return Ok(await _service.GetUsersAsync());
            }
            catch { return Problem(); }
        }

        [HttpPut]
        // [Authorize]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> Put(int id, [FromBody] UserUpdateDto userDto)
        {
            try
            {
                var ok = await _service.UpdateUserAsync(id, userDto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpDelete]
        // [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.DeleteUserAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDto userDto)
        {
            var (user, record, account, error) = await _service.RegisterUserWithRecordAndAccountAsync(userDto);
            if (error != null)
                return BadRequest(error);
            return Ok(new { user.Id, user.Email, recordId = record.Id, accountId = account.Id });
        }
    }
}