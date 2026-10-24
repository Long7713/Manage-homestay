using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StayEase.Web.Models;

public sealed class ApplicationUser : IdentityUser
{
    [MaxLength(120)] public string FullName { get; set; } = "";
    [MaxLength(12)] public string? CitizenId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class RoomType
{
    public int Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = "";
    [MaxLength(120)] public string Slug { get; set; } = "";
    [MaxLength(2000)] public string Description { get; set; } = "";
    [MaxLength(260)] public string ImagePath { get; set; } = "";
    [MaxLength(200)] public string Features { get; set; } = "";
    public int AreaSqm { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<RoomUnit> Units { get; set; } = new List<RoomUnit>();
}

public sealed class RoomUnit
{
    public int Id { get; set; }
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
    [MaxLength(20)] public string Code { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public enum BookingStatus { PendingPayment, AwaitingVerification, Confirmed, CheckedIn, CheckedOut, Cancelled, Expired }
public enum PaymentStatus { Pending, AwaitingVerification, Succeeded, Failed, Refunded }
public enum RefundStatus { Pending, Refunded, NotRefunded }

public sealed class Booking
{
    public int Id { get; set; }
    [MaxLength(30)] public string Reference { get; set; } = "";
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int RoomUnitId { get; set; }
    public RoomUnit RoomUnit { get; set; } = null!;
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int CheckInHour { get; set; } = 14;
    public int CheckOutHour { get; set; } = 12;
    public DateTime CheckInLocal => CheckIn.ToDateTime(new TimeOnly(CheckInHour, 0));
    public DateTime CheckOutLocal => CheckOut.ToDateTime(new TimeOnly(CheckOutHour, 0));
    public int Adults { get; set; }
    public int Children { get; set; }
    [MaxLength(120)] public string GuestName { get; set; } = "";
    [MaxLength(150)] public string GuestEmail { get; set; } = "";
    [MaxLength(25)] public string GuestPhone { get; set; } = "";
    [MaxLength(12)] public string GuestCitizenId { get; set; } = "";
    public int DepositPercentAtBooking { get; set; } = 50;
    public DateTime? CancelledAtUtc { get; set; }
    [MaxLength(1000)] public string Note { get; set; } = "";
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? HoldExpiresAtUtc { get; set; }
    public Payment? Payment { get; set; }
    public RefundRequest? RefundRequest { get; set; }
}

public sealed class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    [MaxLength(40)] public string Method { get; set; } = "DemoSandbox";
    public DateTime? PaidAtUtc { get; set; }
    [MaxLength(20)] public string PaymentMode { get; set; } = "Deposit";
    public DateTime? CustomerReportedAtUtc { get; set; }
    public string? ConfirmedByUserId { get; set; }
}

public sealed class Favorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
}

// One row: admin-adjustable default applied only to NEW bookings.
public sealed class BusinessSettings
{
    public int Id { get; set; } = 1;
    [Range(1,100)] public int DepositPercent { get; set; } = 50;
}

public sealed class RefundRequest
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    [MaxLength(120)] public string ContactName { get; set; } = "";
    [MaxLength(150)] public string ContactEmail { get; set; } = "";
    [MaxLength(25)] public string ContactPhone { get; set; } = "";
    [MaxLength(100)] public string BankName { get; set; } = "";
    [MaxLength(100)] public string BankAccountHolder { get; set; } = "";
    [MaxLength(40)] public string BankAccountNumber { get; set; } = "";
    public decimal Amount { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
}
