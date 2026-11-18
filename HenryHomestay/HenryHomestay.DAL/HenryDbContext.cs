using HenryHomestay.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HenryHomestay.DAL;

public class HenryDbContext : DbContext
{
    public HenryDbContext(DbContextOptions<HenryDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<SavedRoom> SavedRooms => Set<SavedRoom>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<RoomNight> RoomNights => Set<RoomNight>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Phone).IsUnique();
        });

        modelBuilder.Entity<RoomType>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Room>(e =>
        {
            e.HasIndex(x => x.RoomCode).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();
            e.HasOne(x => x.RoomType)
             .WithMany(t => t.Rooms)
             .HasForeignKey(x => x.RoomTypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Amenity>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<RoomAmenity>(e =>
        {
            e.HasKey(x => new { x.RoomId, x.AmenityId });
            e.HasOne(x => x.Room)
             .WithMany(r => r.RoomAmenities)
             .HasForeignKey(x => x.RoomId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Amenity)
             .WithMany(a => a.RoomAmenities)
             .HasForeignKey(x => x.AmenityId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomImage>(e =>
        {
            e.HasOne(x => x.Room)
             .WithMany(r => r.RoomImages)
             .HasForeignKey(x => x.RoomId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SavedRoom>(e =>
        {
            e.HasKey(x => new { x.UserId, x.RoomId });
            e.HasOne(x => x.User)
             .WithMany(u => u.SavedRooms)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Room)
             .WithMany()
             .HasForeignKey(x => x.RoomId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasIndex(x => x.BookingCode).IsUnique();
            e.HasOne(x => x.User)
             .WithMany(u => u.Bookings)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingRoom>(e =>
        {
            e.HasKey(x => new { x.BookingId, x.RoomId });
            e.HasOne(x => x.Booking)
             .WithMany(b => b.BookingRooms)
             .HasForeignKey(x => x.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Room)
             .WithMany(r => r.BookingRooms)
             .HasForeignKey(x => x.RoomId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoomNight>(e =>
        {
            e.HasKey(x => new { x.RoomId, x.StayDate });
            e.HasOne(x => x.Room)
             .WithMany(r => r.RoomNights)
             .HasForeignKey(x => x.RoomId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Booking)
             .WithMany(b => b.RoomNights)
             .HasForeignKey(x => x.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasOne(x => x.Booking)
             .WithMany(b => b.Payments)
             .HasForeignKey(x => x.BookingId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ConfirmedByUser)
             .WithMany()
             .HasForeignKey(x => x.ConfirmedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
