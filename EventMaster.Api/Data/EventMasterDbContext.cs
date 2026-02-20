using EventMaster.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventMaster.Api.Data;

public partial class EventMasterDbContext : DbContext
{
    public EventMasterDbContext(DbContextOptions<EventMasterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<_event> events { get; set; }

    public virtual DbSet<booking> bookings { get; set; }

    public virtual DbSet<event_occurrence> event_occurrences { get; set; }

    public virtual DbSet<payment> payments { get; set; }

    public virtual DbSet<reply> replies { get; set; }

    public virtual DbSet<review> reviews { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<venue> venues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<_event>(entity =>
        {
            entity.HasKey(e => e.event_id).HasName("PRIMARY");

            entity.HasIndex(e => e.category, "idx_events_category");

            entity.HasIndex(e => e.name, "idx_events_name");

            entity.HasIndex(e => e.org_id, "idx_events_org");

            entity.Property(e => e.category).HasMaxLength(80);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.image).HasMaxLength(500);
            entity.Property(e => e.name).HasMaxLength(180);
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.org).WithMany(p => p._events)
                .HasForeignKey(d => d.org_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_events_org");
        });

        modelBuilder.Entity<booking>(entity =>
        {
            entity.HasKey(e => e.booking_id).HasName("PRIMARY");

            entity.HasIndex(e => e.customer_id, "idx_booking_customer");

            entity.HasIndex(e => e.occurrence_id, "idx_booking_occurrence");

            entity.HasIndex(e => e.status, "idx_booking_status");

            entity.HasIndex(e => e.ticket_number, "uq_booking_ticket").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.seats_occupied).HasColumnType("text");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'Confirmed'")
                .HasColumnType("enum('Confirmed','Cancelled')");
            entity.Property(e => e.ticket_number).HasMaxLength(64);
            entity.Property(e => e.total_amount).HasPrecision(10, 2);
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.customer).WithMany(p => p.bookings)
                .HasForeignKey(d => d.customer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_customer");

            entity.HasOne(d => d.occurrence).WithMany(p => p.bookings)
                .HasForeignKey(d => d.occurrence_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_occ");
        });

        modelBuilder.Entity<event_occurrence>(entity =>
        {
            entity.HasKey(e => e.occurrence_id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.date, e.time }, "idx_occ_datetime");

            entity.HasIndex(e => e.event_id, "idx_occ_event");

            entity.HasIndex(e => e.status, "idx_occ_status");

            entity.HasIndex(e => e.venue_id, "idx_occ_venue");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.price).HasPrecision(10, 2);
            entity.Property(e => e.seats_occupied).HasColumnType("text");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'Scheduled'")
                .HasColumnType("enum('Scheduled','Cancelled','Completed')");
            entity.Property(e => e.time).HasColumnType("time");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d._event).WithMany(p => p.event_occurrences)
                .HasForeignKey(d => d.event_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_occ_event");

            entity.HasOne(d => d.venue).WithMany(p => p.event_occurrences)
                .HasForeignKey(d => d.venue_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_occ_venue");
        });

        modelBuilder.Entity<payment>(entity =>
        {
            entity.HasKey(e => e.payment_id).HasName("PRIMARY");

            entity.HasIndex(e => e.booking_id, "idx_payment_booking");

            entity.HasIndex(e => e.created_at, "idx_payment_created");

            entity.HasIndex(e => e.status, "idx_payment_status");

            entity.Property(e => e.amount).HasPrecision(10, 2);
            entity.Property(e => e.card).HasMaxLength(120);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.details).HasMaxLength(255);
            entity.Property(e => e.status).HasColumnType("enum('Success','Failed','Refunded')");

            entity.HasOne(d => d.booking).WithMany(p => p.payments)
                .HasForeignKey(d => d.booking_id)
                .HasConstraintName("fk_payment_booking");
        });

        modelBuilder.Entity<reply>(entity =>
        {
            entity.HasKey(e => e.reply_id).HasName("PRIMARY");

            entity.HasIndex(e => e.organizer_id, "idx_reply_org");

            entity.HasIndex(e => e.review_id, "idx_reply_review");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.reply_text).HasColumnType("text");

            entity.HasOne(d => d.organizer).WithMany(p => p.replies)
                .HasForeignKey(d => d.organizer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reply_org");

            entity.HasOne(d => d.review).WithMany(p => p.replies)
                .HasForeignKey(d => d.review_id)
                .HasConstraintName("fk_reply_review");
        });

        modelBuilder.Entity<review>(entity =>
        {
            entity.HasKey(e => e.review_id).HasName("PRIMARY");

            entity.HasIndex(e => e.customer_id, "idx_review_customer");

            entity.HasIndex(e => e.occurrence_id, "idx_review_occ");

            entity.HasIndex(e => new { e.occurrence_id, e.customer_id }, "uq_review_once").IsUnique();

            entity.Property(e => e.comment).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.customer).WithMany(p => p.reviews)
                .HasForeignKey(d => d.customer_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_customer");

            entity.HasOne(d => d.occurrence).WithMany(p => p.reviews)
                .HasForeignKey(d => d.occurrence_id)
                .HasConstraintName("fk_review_occ");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PRIMARY");

            entity.HasIndex(e => e.role, "idx_users_role");

            entity.HasIndex(e => e.email, "uq_users_email").IsUnique();

            entity.HasIndex(e => e.username, "uq_users_username").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.email).HasMaxLength(190);
            entity.Property(e => e.name).HasMaxLength(120);
            entity.Property(e => e.password).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(30);
            entity.Property(e => e.role)
                .HasDefaultValueSql("'CUSTOMER'")
                .HasColumnType("enum('CUSTOMER','ORGANIZER')");
            entity.Property(e => e.status)
               .HasDefaultValueSql("'Active'")
               .HasColumnType("enum('Active','Deactivated')");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.username).HasMaxLength(60);
        });

        modelBuilder.Entity<venue>(entity =>
        {
            entity.HasKey(e => e.venue_id).HasName("PRIMARY");

            entity.HasIndex(e => e.city, "idx_venues_city");

            entity.HasIndex(e => e.seating, "idx_venues_seating");

            entity.Property(e => e.address).HasMaxLength(255);
            entity.Property(e => e.city).HasMaxLength(120);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.name).HasMaxLength(180);
            entity.Property(e => e.postal_code).HasMaxLength(20);
            entity.Property(e => e.province).HasMaxLength(80);
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
