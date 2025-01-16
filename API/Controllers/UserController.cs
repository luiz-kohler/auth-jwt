using API.DTOs;
using API.Handlers;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Controller]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserRequest request)
        {
            var token = await _userService.Create(request);
            return Ok(token);
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            var token = await _userService.SignIn(request);
            return Ok(token);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var request = new RefreshTokenModel 
            {
                AccessToken = HttpContext.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "") ?? "",
                RefreshToken = refreshToken,
            };

            var token = await _userService.RefreshToken(request);
            return Ok(token);
        }

        [HttpGet("is-logged")]
        [Authorize]
        public async Task<IActionResult> IsLogged()
        {
            return Ok(new { message = "user is logged" });
        }

        [HttpGet("is-admin")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<IActionResult> IsAdmin()
        {
            return Ok(new { message = "user is admin" });
        }
    }
}
