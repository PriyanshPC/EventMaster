using EventTicketManagement.Api.Data;
using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventTicketManagement.Api.Services
{
    public class PaymentService
    {
        /// <summary>
        /// Handles payment processing for bookings. Validates the booking and simulates payment approval based on card number.
        /// </summary>
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Processes a payment for a given booking. Checks if the booking exists and belongs to the customer, then simulates payment approval based on the card number. Updates booking status if payment is successful and saves the payment record in the database.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Payment> ProcessPaymentAsync(PaymentRequestDto dto, int customerId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == dto.BookingId && b.CustomerId == customerId);

            if (booking == null)
                throw new Exception("Booking not found or unauthorized");

            bool approved = dto.Card.EndsWith("1111");

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalAmount,
                Card = dto.Card,
                Status = approved ? "Success" : "Failed",
                Details = approved ? "Payment approved" : "Card declined",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            if (approved)
                booking.Status = "Paid";

            await _context.SaveChangesAsync();

            return payment;
        }
    }
}