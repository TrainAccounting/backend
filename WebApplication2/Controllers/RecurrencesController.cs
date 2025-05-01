using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecurrencesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetRecurrences()
    {
        var recurrences = new[]
        {
            new { Id = 1, Name = "Ежемесячная оплата", Date = DateTime.Now }
        };
        return Ok(recurrences);
    }

    [HttpPost]
    public IActionResult CreateRecurrence()
    {
        return Ok("Повторение создано (заглушка)");
    }
}