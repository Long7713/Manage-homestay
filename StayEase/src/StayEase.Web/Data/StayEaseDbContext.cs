using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Models;

namespace StayEase.Web.Data;

public sealed class StayEaseDbContext(DbContextOptions<StayEaseDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<RoomUnit> RoomUnits => Set<RoomUnit>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<BusinessSettings> BusinessSettings => Set<BusinessSettings>();
    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.Entity<RoomType>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<RoomType>().Property(x => x.PricePerNight).HasColumnType("decimal(18,2)");
        model.Entity<RoomUnit>().HasIndex(x => x.Code).IsUnique();
        model.Entity<RoomUnit>().HasOne(x => x.RoomType).WithMany(x => x.Units)
            .HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Booking>().HasIndex(x => x.Reference).IsUnique();
        model.Entity<ApplicationUser>().Property(u=>u.CitizenId).HasMaxLength(12);
        model.Entity<BusinessSettings>().HasData(new BusinessSettings { Id=1, DepositPercent=50 });
        model.Entity<Booking>().HasIndex(x => new { x.RoomUnitId, x.CheckIn, x.CheckOut });
        model.Entity<Booking>().Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        model.Entity<Booking>().Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        model.Entity<Booking>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        model.Entity<Booking>().HasOne(x => x.RoomUnit).WithMany().HasForeignKey(x => x.RoomUnitId)
            .OnDelete(DeleteBehavior.Restrict);
        model.Entity<Payment>().HasIndex(x => x.BookingId).IsUnique();
        model.Entity<Payment>().Property(x => x.Amount).HasColumnType("decimal(18,2)");
        model.Entity<Payment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        model.Entity<Payment>().HasOne(x => x.Booking).WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<RefundRequest>().HasIndex(x=>x.BookingId).IsUnique();
        model.Entity<RefundRequest>().HasOne(x=>x.Booking).WithOne(x=>x.RefundRequest)
            .HasForeignKey<RefundRequest>(x=>x.BookingId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<RefundRequest>().Property(x=>x.Amount).HasColumnType("decimal(18,2)");
        model.Entity<RefundRequest>().Property(x=>x.Status).HasConversion<string>().HasMaxLength(32);
        model.Entity<Favorite>().HasIndex(x => new { x.UserId, x.RoomTypeId }).IsUnique();
        model.Entity<Favorite>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        model.Entity<Favorite>().HasOne(x => x.RoomType).WithMany().HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
