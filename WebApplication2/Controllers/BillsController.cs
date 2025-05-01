using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetBills()
    {
        var bills = new[]
        {
            new { Id = 1, Name = "Электричество", Value = 200 }
        };
        return Ok(bills);
    }

    [HttpPost]
    public IActionResult CreateBill()
    {
        return Ok("Счет создан (заглушка)");
    }
}