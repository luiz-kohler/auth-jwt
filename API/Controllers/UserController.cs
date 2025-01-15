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

        [HttpGet("is-logged")]
        [Authorize]
        public async Task<IActionResult> IsLogged()
        {
            return Ok(new { message = "user is logged" });
        }

        [HttpGet("is-admin")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> IsAdmin()
        {
            return Ok(new { message = "user is admin" });
        }
    }
}
