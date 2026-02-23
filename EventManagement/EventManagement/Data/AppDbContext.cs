using Microsoft.EntityFrameworkCore;

using EventTicketManagement.Api.Models;

namespace EventTicketManagement.Api.Data
{public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<EventOccurrence> EventOccurrences { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }

    public DbSet<Reply> Replies { get; set; }
    public DbSet<Media> Media { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.OccurrenceId, r.CustomerId })
            .IsUnique();

             modelBuilder.Entity<Event>()
        .HasOne(e => e.Organizer)
        .WithMany(u => u.OrganizedEvents)
        .HasForeignKey(e => e.OrgId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}

}
