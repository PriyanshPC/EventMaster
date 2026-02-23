using EventTicketManagement.Api.Data;
using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventTicketManagement.Api.Services
{
    /// <summary> 
    ///  Handles all booking-related logic such as creating a booking, /// updating quantity, checking capacity, and cancelling bookings. 
    ///  </summary>
    public class BookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary> 
        ///  Creates a new booking for a customer and reduces the event's capacity. 
        /// </summary>
        public async Task<Booking> CreateBookingAsync(BookingCreateDto dto, int customerId)
        {
            var occurrence = await _context.EventOccurrences.FindAsync(dto.OccurrenceId);
            if (occurrence == null)
                throw new Exception("Event occurrence not found");

            if (occurrence.Status != "Scheduled")
                throw new Exception("Event is not bookable");

            if (occurrence.RemainingCapacity < dto.Quantity)
                throw new Exception("Not enough capacity");

            // Reduce capacity
            occurrence.RemainingCapacity -= dto.Quantity;

            var booking = new Booking
            {
                OccurrenceId = dto.OccurrenceId,
                CustomerId = customerId,
                Quantity = dto.Quantity,
                TotalAmount = occurrence.Price * dto.Quantity,
                Status = "Confirmed",
                TicketNumber = Guid.NewGuid().ToString().Substring(0, 8),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking;
        }
        /// <summary> 
        /// /// Gets all bookings made by a specific customer. 
        /// /// </summary>
        public async Task<List<Booking>> GetBookingsForCustomerAsync(int customerId)
        {
            return await _context.Bookings
                .Where(b => b.CustomerId == customerId)
                .Include(b => b.Occurrence)
                .ToListAsync();
        }

        /// <summary>
        ///   Updates the quantity of an existing booking and adjusts capacity. 
        ///  </summary>
        public async Task<Booking> UpdateBookingAsync(int bookingId, int customerId, BookingUpdateDto dto)
        {
            var booking = await _context.Bookings
                .Include(b => b.Occurrence)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.CustomerId == customerId);

            if (booking == null)
                throw new Exception("Booking not found or unauthorized");

            if (booking.Status == "Paid")
                throw new Exception("Cannot edit a paid booking");

            var occurrence = booking.Occurrence!;
            int difference = dto.Quantity - booking.Quantity;

            if (difference > 0 && occurrence.RemainingCapacity < difference)
                throw new Exception("Not enough capacity");

            // Adjust capacity
            occurrence.RemainingCapacity -= difference;

            // Update booking
            booking.Quantity = dto.Quantity;
            booking.TotalAmount = occurrence.Price * dto.Quantity;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return booking;
        }

        /// <summary> 
        ///  Cancels a booking and restores the event's capacity. 
        ///  </summary>
        public async Task<bool> CancelBookingAsync(int bookingId, int customerId)
            {
                var booking = await _context.Bookings
                    .Include(b => b.Occurrence)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.CustomerId == customerId);

                if (booking == null)
                    throw new Exception("Booking not found or unauthorized");

                if (booking.Status == "Paid")
                    throw new Exception("Cannot cancel a paid booking");

                var occurrence = booking.Occurrence!;
                occurrence.RemainingCapacity += booking.Quantity;

                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }


    }
}
