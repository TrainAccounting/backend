using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Models;
using Trainacc.Services;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionsService _service;
        public SubscriptionsController(SubscriptionsService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptions()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            return await _service.GetSubscriptionsAsync(userId);
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(SubscriptionCreateDto dto)
        {
            var created = await _service.CreateSubscriptionAsync(dto);
            return CreatedAtAction(nameof(GetSubscriptions), new { id = created.Id }, created);
        }

        [HttpPost("process")] 
        public async Task<IActionResult> ProcessSubscriptions()
        {
            var count = await _service.ProcessActiveSubscriptionsAsync();
            return Ok($"Создано транзакций: {count}");
        }
    }
}
