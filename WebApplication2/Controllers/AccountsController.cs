using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAccounts()
    {
        var accounts = new[]
        {
            new { Id = 1, Name = "Основной счет", CreatedAt = DateTime.Now }
        };
        return Ok(accounts);
    }

    [HttpPost]
    public IActionResult CreateAccount()
    {
        return Ok("Учет создан (заглушка)");
    }
}