using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Api.Controllers
{
    /// <summary> 
    ///  Handles payment processing for bookings. 
    ///  </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("submit/{customerId}")]
        public async Task<IActionResult> SubmitPayment(int customerId, [FromBody] PaymentRequestDto dto)
        {

            var payment = await _paymentService.ProcessPaymentAsync(dto, customerId);

            return Ok(new
            {
                payment.PaymentId,
                payment.BookingId,
                payment.Amount,
                payment.Status,
                payment.Details,
                payment.CreatedAt
            });
        }
    }
}
