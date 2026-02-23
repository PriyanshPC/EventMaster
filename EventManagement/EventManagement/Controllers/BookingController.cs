using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Api.Controllers
{
    /// <summary> 
    ///  This controller handles everything related to bookings, 
    ///  like creating a booking, viewing your bookings, updating quantity, /// and cancelling a booking. 
    ///  </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("create/{UserId}")]
        public async Task<IActionResult> CreateBooking(int UserId,[FromBody] BookingCreateDto dto)
        {

            var booking = await _bookingService.CreateBookingAsync(dto, UserId);

            return Ok(new
            {
                booking.BookingId,
                booking.OccurrenceId,
                booking.CustomerId,
                booking.Quantity,
                booking.TotalAmount,
                booking.Status,
                booking.TicketNumber
            });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings([FromQuery] int UserId)
        {

            var bookings = await _bookingService.GetBookingsForCustomerAsync(UserId);
            return Ok(bookings);
        }

        [HttpPut("{bookingId}/update/{userId}")]
        public async Task<IActionResult> UpdateBooking(int bookingId, int userId, [FromBody] BookingUpdateDto dto)
        {
            try
            {
                var booking = await _bookingService.UpdateBookingAsync(bookingId, userId, dto);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpDelete("{bookingId}/cancel/{userId}")]
        public async Task<IActionResult> CancelBooking(int bookingId, int userId)
        {
            try
            {
                await _bookingService.CancelBookingAsync(bookingId, userId);
                return Ok(new { message = "Booking cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary> 
        /// Shows which HTTP methods this controller supports
        /// </summary> 
        /// <returns>Returns an Allow header with supported methods.</returns>
        [HttpOptions]
        public IActionResult GetBookingOptions()
        {
            Response.Headers.Add("Allow", "GET,POST,PUT,DELETE,OPTIONS");
            return Ok();
        }

    }
}
