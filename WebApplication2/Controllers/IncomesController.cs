using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncomesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetIncomes()
    {
        var incomes = new[]
        {
            new { Id = 1, Name = "Зарплата", Value = 1000 }
        };
        return Ok(incomes);
    }

    [HttpPost]
    public IActionResult CreateIncome()
    {
        return Ok("Доход добавлен (заглушка)");
    }
}