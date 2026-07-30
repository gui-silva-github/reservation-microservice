using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.BLL.Services;

namespace Reservation.BookingsService.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<BookingResponse> bookings = await _bookingService.GetAllAsync(cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("search/userid/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BookingResponse> bookings = await _bookingService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("search/spaceid/{spaceId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetBySpaceId(
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BookingResponse> bookings = await _bookingService.GetBySpaceIdAsync(spaceId, cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("search/date/{date:datetime}")]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetByDate(
        DateTime date,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BookingResponse> bookings = await _bookingService.GetByDateAsync(date, cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        BookingResponse booking = await _bookingService.GetByIdAsync(id, cancellationToken);
        return Ok(booking);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<BookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        BookingResponse createdBooking = await _bookingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdBooking.Id }, createdBooking);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<BookingResponse>> Update(
        Guid id,
        [FromBody] UpdateBookingRequest request,
        CancellationToken cancellationToken)
    {
        BookingResponse updatedBooking = await _bookingService.UpdateAsync(id, request, cancellationToken);
        return Ok(updatedBooking);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _bookingService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
