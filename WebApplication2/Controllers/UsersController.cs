using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Models;
using Trainacc.Filters;
using Trainacc.Services;

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
        public async Task<IActionResult> Get(int? id = null)
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetUserAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
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
    }
}