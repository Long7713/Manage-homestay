using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HenryHomestay.Core.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required, MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("cccd")]
    public string? Cccd { get; set; }

    [Column("date_of_birth", TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(500)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Required, MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column("role")]
    public string Role { get; set; } = "CUSTOMER"; // CUSTOMER, STAFF, ADMIN

    [MaxLength(50)]
    [Column("staff_position")]
    public string? StaffPosition { get; set; } // Quản lý, Lễ tân, Kế toán, Buồng phòng, Bảo vệ

    [Column("hire_date", TypeName = "date")]
    public DateTime? HireDate { get; set; }

    [Column("is_locked")]
    public bool IsLocked { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<SavedRoom> SavedRooms { get; set; } = new List<SavedRoom>();
}

[Table("room_types")]
public class RoomType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(30)]
    [Column("code")]
    public string Code { get; set; } = string.Empty; // double, studio, family

    [Required, MaxLength(80)]
    [Column("name")]
    public string Name { get; set; } = string.Empty; // Phòng đôi, Studio, Phòng gia đình

    [MaxLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}

[Table("rooms")]
public class Room
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("room_code")]
    public string RoomCode { get; set; } = string.Empty; // 41, 42, 51, 52, 71, 81

    [Required, MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("room_type_id")]
    public int RoomTypeId { get; set; }

    [ForeignKey(nameof(RoomTypeId))]
    public RoomType? RoomType { get; set; }

    [Column("area_m2")]
    public int AreaM2 { get; set; } = 35;

    [MaxLength(100)]
    [Column("bed_info")]
    public string? BedInfo { get; set; }

    [MaxLength(150)]
    [Column("view_description")]
    public string? ViewDescription { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("max_guests")]
    public byte MaxGuests { get; set; } = 2;

    [Column("allow_pets")]
    public bool AllowPets { get; set; }

    [Column("price_per_night", TypeName = "decimal(12,0)")]
    public decimal PricePerNight { get; set; }

    [Required, MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, HIDDEN

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    public ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<RoomNight> RoomNights { get; set; } = new List<RoomNight>();
}

[Table("amenities")]
public class Amenity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(30)]
    [Column("code")]
    public string Code { get; set; } = string.Empty; // wifi, aircon, balcony, kitchen

    [Required, MaxLength(80)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("icon")]
    public string? Icon { get; set; }

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}

[Table("room_amenities")]
public class RoomAmenity
{
    [Column("room_id")]
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Column("amenity_id")]
    public int AmenityId { get; set; }

    [ForeignKey(nameof(AmenityId))]
    public Amenity? Amenity { get; set; }
}

[Table("room_images")]
public class RoomImage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("room_id")]
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Required, MaxLength(500)]
    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }
}

[Table("saved_rooms")]
public class SavedRoom
{
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Column("room_id")]
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("bookings")]
public class Booking
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required, MaxLength(20)]
    [Column("booking_code")]
    public string BookingCode { get; set; } = string.Empty;

    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required, MaxLength(100)]
    [Column("guest_name")]
    public string GuestName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column("guest_phone")]
    public string GuestPhone { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("guest_email")]
    public string GuestEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("guest_cccd")]
    public string? GuestCccd { get; set; }

    [Column("guest_dob", TypeName = "date")]
    public DateTime? GuestDob { get; set; }

    [Column("check_in_date", TypeName = "date")]
    public DateTime CheckInDate { get; set; }

    [Column("check_out_date", TypeName = "date")]
    public DateTime CheckOutDate { get; set; }

    [NotMapped]
    public int Nights => Math.Max(1, (int)(CheckOutDate.Date - CheckInDate.Date).TotalDays);

    [Column("adults")]
    public byte Adults { get; set; } = 1;

    [Column("children")]
    public byte Children { get; set; }

    [Column("infants")]
    public byte Infants { get; set; }

    [Column("pets")]
    public byte Pets { get; set; }

    [Column("total_amount", TypeName = "decimal(12,0)")]
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(20)]
    [Column("payment_option")]
    public string PaymentOption { get; set; } = "FULL"; // FULL, DEPOSIT

    [Column("deposit_percent")]
    public byte DepositPercent { get; set; } = 30;

    [Column("deposit_amount", TypeName = "decimal(12,0)")]
    public decimal DepositAmount { get; set; }

    [Column("paid_amount", TypeName = "decimal(12,0)")]
    public decimal PaidAmount { get; set; }

    [NotMapped]
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);

    [Required, MaxLength(25)]
    [Column("status")]
    public string Status { get; set; } = "PENDING_PAYMENT"; // PENDING_PAYMENT, CONFIRMED, CHECKED_IN, COMPLETED, CANCELLED

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by")]
    public long? CancelledBy { get; set; }

    [MaxLength(255)]
    [Column("cancel_reason")]
    public string? CancelReason { get; set; }

    [Required, MaxLength(25)]
    [Column("refund_status")]
    public string RefundStatus { get; set; } = "NONE"; // NONE, WAITING_REFUND, REFUNDED, NO_REFUND

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<RoomNight> RoomNights { get; set; } = new List<RoomNight>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

[Table("booking_rooms")]
public class BookingRoom
{
    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Column("room_id")]
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Column("price_per_night", TypeName = "decimal(12,0)")]
    public decimal PricePerNight { get; set; }
}

[Table("room_nights")]
public class RoomNight
{
    [Column("room_id")]
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Column("stay_date", TypeName = "date")]
    public DateTime StayDate { get; set; }

    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }
}

[Table("payments")]
public class Payment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [MaxLength(20)]
    [Column("payment_code")]
    public string? PaymentCode { get; set; }

    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Column("amount", TypeName = "decimal(12,0)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(20)]
    [Column("type")]
    public string Type { get; set; } = "FULL"; // DEPOSIT, FULL, BALANCE, REFUND

    [Required, MaxLength(25)]
    [Column("method")]
    public string Method { get; set; } = "BANK_TRANSFER"; // ONLINE, BANK_TRANSFER, CASH

    [Required, MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "PENDING"; // PENDING, CONFIRMED, FAILED, CANCELLED

    [MaxLength(100)]
    [Column("gateway_ref")]
    public string? GatewayRef { get; set; }

    [MaxLength(255)]
    [Column("note")]
    public string? Note { get; set; }

    [Column("confirmed_by")]
    public long? ConfirmedBy { get; set; }

    [ForeignKey(nameof(ConfirmedBy))]
    public User? ConfirmedByUser { get; set; }

    [Column("confirmed_at")]
    public DateTime? ConfirmedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("system_settings")]
public class SystemSetting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public byte Id { get; set; } = 1;

    [MaxLength(500)]
    [Column("qr_code_url")]
    public string? QrCodeUrl { get; set; } = "assets/images/payment-qr.svg";

    [Required, MaxLength(100)]
    [Column("bank_name")]
    public string BankName { get; set; } = "MB Bank";

    [Required, MaxLength(30)]
    [Column("bank_account_number")]
    public string BankAccountNumber { get; set; } = "0708670042";

    [Required, MaxLength(100)]
    [Column("bank_account_holder")]
    public string BankAccountHolder { get; set; } = "Võ Văn Việt";

    [Required, MaxLength(20)]
    [Column("contact_phone")]
    public string ContactPhone { get; set; } = "0795443500";

    [Column("deposit_percent")]
    public byte DepositPercent { get; set; } = 30;

    [Column("change_cancel_hours")]
    public short ChangeCancelHours { get; set; } = 24;

    [Column("hold_minutes")]
    public short HoldMinutes { get; set; } = 30;

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

[Table("contact_inquiries")]
public class ContactInquiry
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long? UserId { get; set; }

    [Required, MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("service_type")]
    public string? ServiceType { get; set; }

    [Column("expected_stay_date", TypeName = "date")]
    public DateTime? ExpectedStayDate { get; set; }

    [MaxLength(1000)]
    [Column("message")]
    public string? Message { get; set; }

    [Required, MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "NEW"; // NEW, PROCESSED

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
