using Microsoft.AspNetCore.Mvc;
using Reservation.UsersService.API.DTOs;
using Reservation.UsersService.BLL.DTOs;
using Reservation.UsersService.BLL.Interfaces;

namespace Reservation.UsersService.API.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) {
            AuthenticationResponse response = await _authService.RegisterAsync(request);
            return CreatedAtAction(
                actionName: nameof(UsersController.GetById),
                controllerName: "Users",
                routeValues: new { id = response.UserId },
                value: response
            );
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) {
            AuthenticationResponse response = await _authService.LoginAsync(request);
            return Ok(response);
        }
    }
}