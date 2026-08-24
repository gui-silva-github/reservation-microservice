using Microsoft.AspNetCore.Mvc;
using Reservation.UsersService.API.DTOs;
using Reservation.UsersService.BLL.DTOs;
using Reservation.UsersService.BLL.Interfaces;

namespace Reservation.UsersService.API.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly IUserService _userService;

        public UsersController(IUserService userService) {
            _userService = userService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id) {
            UserResponse user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }
    }
}