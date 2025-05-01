using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepositsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetDeposits()
    {
        var deposits = new[]
        {
            new { Id = 1, Value = 10000, OpenedDate = DateTime.Now }
        };
        return Ok(deposits);
    }

    [HttpPost]
    public IActionResult CreateDeposit()
    {
        return Ok("Вклад создан (заглушка)");
    }
}