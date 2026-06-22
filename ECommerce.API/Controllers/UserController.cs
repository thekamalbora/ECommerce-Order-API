using System.Security.Claims;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]

    [Route(
 "api/user")]

    public class UserController
 : ControllerBase
    {
        private readonly
        IUserService
        _userService;

        public UserController(
        IUserService userService)
        {
            _userService =
            userService;
        }

        [Authorize]

        [HttpGet(
        "profile")]

        public async
        Task<IActionResult> Profile()
        {
            var result =
            await
            _userService
            .GetProfile(
            User);

            return Ok(
            result);
        }
    }
}
