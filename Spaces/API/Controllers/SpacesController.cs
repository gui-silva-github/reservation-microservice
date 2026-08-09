using Microsoft.AspNetCore.Mvc;
using Reservation.SpacesService.BLL.DTO;
using Reservation.SpacesService.BLL.Services;

namespace Reservation.SpacesService.API.Controllers
{
    [ApiController]
    [Route("api/spaces")]
    public class SpacesController : ControllerBase
    {
        private readonly ISpaceService _spaceService;

        public SpacesController(ISpaceService spaceService)
        {
            _spaceService = spaceService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<SpaceResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<SpaceResponse>>> GetAll(CancellationToken cancellationToken)
        {   
            IReadOnlyList<SpaceResponse> spaces = await _spaceService.GetAllAsync(cancellationToken);
            return Ok(spaces);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SpaceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpaceResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            SpaceResponse space = await _spaceService.GetByIdAsync(id, cancellationToken);
            return Ok(space);
        }

        [HttpGet("search/{termo}")]
        [ProducesResponseType(typeof(IReadOnlyList<SpaceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<SpaceResponse>>> Search(string termo, CancellationToken cancellationToken)
        {
            IReadOnlyList<SpaceResponse> spaces = await _spaceService.SearchAsync(termo, cancellationToken);
            return Ok(spaces);
        }

        [HttpGet("{id:guid}/availability")]
        [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<AvailabilityResponse>> CheckAvailability(
            Guid id,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            CancellationToken cancellationToken
        )
        {
            AvailabilityResponse availability = await _spaceService.CheckAvailabilityAsync(id, from, to, cancellationToken);
            return Ok(availability);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SpaceResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SpaceResponse>> Create(
            [FromBody] CreateSpaceRequest request,
            CancellationToken cancellationToken
        )
        {
            SpaceResponse createdSpace = await _spaceService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdSpace.Id }, createdSpace);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(SpaceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SpaceResponse>> Update(
            Guid id,
            [FromBody] UpdateSpaceRequest request,
            CancellationToken cancellationToken
        )
        {
            SpaceResponse updatedSpace = await _spaceService.UpdateAsync(id, request, cancellationToken);
            return Ok(updatedSpace);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _spaceService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}