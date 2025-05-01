using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetExpenses()
    {
        var expenses = new[]
        {
            new { Id = 1, Name = "Коммуналка", Value = 500 }
        };
        return Ok(expenses);
    }

    [HttpPost]
    public IActionResult CreateExpense()
    {
        return Ok("Расход добавлен (заглушка)");
    }
}