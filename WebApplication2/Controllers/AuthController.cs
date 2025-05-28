// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Trainacc.Data;
// using Trainacc.Services;
// using Trainacc.Models;
// using Trainacc.Filters;

// namespace Trainacc.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [ServiceFilter(typeof(LogActionFilter))]
//     [ServiceFilter(typeof(ETagFilter))]
//     public class AuthController : ControllerBase
//     {
//         private readonly AuthService _authService;
//         public AuthController(AuthService authService)
//         {
//             _authService = authService;
//         }

//         [HttpPost("register")]
//         public async Task<ActionResult<UserAuthDto>> Register(UserCreateDto userDto)
//         {
//             try
//             {
//                 var (result, error) = await _authService.RegisterAsync(userDto);
//                 if (error != null) return BadRequest(error);
//                 if (result == null) return Problem();
//                 if (!string.IsNullOrEmpty(result.Token))
//                 {
//                     Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
//                     {
//                         HttpOnly = true,
//                         Secure = true,
//                         SameSite = SameSiteMode.Strict,
//                         Expires = DateTimeOffset.UtcNow.AddHours(1)
//                     });
//                 }
//                 return result;
//             }
//             catch { return Problem(); }
//         }

//         [HttpPost("login")]
//         public async Task<ActionResult<UserAuthDto>> Login([FromBody] UserLoginDto loginDto)
//         {
//             try
//             {
//                 var (result, error) = await _authService.LoginAsync(loginDto);
//                 if (error != null) return Unauthorized(error);
//                 if (result == null) return Problem();
//                 if (!string.IsNullOrEmpty(result.Token))
//                 {
//                     Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
//                     {
//                         HttpOnly = true,
//                         Secure = true,
//                         SameSite = SameSiteMode.Strict,
//                         Expires = DateTimeOffset.UtcNow.AddHours(1)
//                     });
//                 }
//                 return result;
//             }
//             catch { return Problem(); }
//         }
//     }
// }