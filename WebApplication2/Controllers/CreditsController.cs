using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCredits()
    {
        var credits = new[]
        {
            new { Id = 1, Name = "Ипотека", Value = 500000 }
        };
        return Ok(credits);
    }

    [HttpPost]
    public IActionResult CreateCredit()
    {
        return Ok("Кредит добавлен (заглушка)");
    }
}