using Microsoft.AspNetCore.Mvc;

namespace Trainacc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = new[]
        {
            new { Id = 1, FullName = "Иванов Иван", Port = "Порт 1", NumberType = "Тип 1" }
        };
        return Ok(users);
    }

    [HttpPost]
    public IActionResult CreateUser()
    {
        return Ok("Пользователь создан (заглушка)");
    }
}