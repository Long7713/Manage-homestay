namespace HenryHomestay.Core.Models;

public class OperationResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static OperationResult<T> Ok(T data, string message = "Thành công.")
        => new() { Success = true, Message = message, Data = data };

    public static OperationResult<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static OperationResult Ok(string message = "Thành công.")
        => new() { Success = true, Message = message };

    public static OperationResult Fail(string message)
        => new() { Success = false, Message = message };
}

public class RoomDisplayDto
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string TypeCode { get; set; } = string.Empty;      // double, studio, family
    public string TypeName { get; set; } = string.Empty;      // Phòng đôi, Studio, Phòng gia đình
    public int AreaM2 { get; set; }
    public string BedInfo { get; set; } = string.Empty;
    public string ViewDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte MaxGuests { get; set; }
    public bool AllowPets { get; set; }
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string PrimaryImage { get; set; } = "photo-01.jpg";
    public List<string> AmenityCodes { get; set; } = new();
    public List<string> AmenityNames { get; set; } = new();
    public string OccupancyStatus { get; set; } = "Chưa ai đặt";
    public bool IsSaved { get; set; }
}

public class BookingSummaryDto
{
    public long Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerCccd { get; set; } = string.Empty;
    public DateTime? GuestDob { get; set; }
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string RoomImage { get; set; } = "photo-03.jpg";
    public string RoomMeta { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Nights { get; set; }
    public byte Adults { get; set; }
    public byte Children { get; set; }
    public byte Infants { get; set; }
    public byte Pets { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentOption { get; set; } = "FULL";
    public byte DepositPercent { get; set; } = 30;
    public decimal DepositAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = "PENDING_PAYMENT";
    public string StatusDisplay => ToVietnameseStatus(Status);
    public string RefundStatus { get; set; } = "NONE";
    public string RefundDisplay => ToVietnameseRefund(RefundStatus);
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public static string ToVietnameseStatus(string status) => status switch
    {
        "PENDING_PAYMENT" => "Chờ xác nhận",
        "CONFIRMED" => "Đã xác nhận",
        "CHECKED_IN" => "Đã nhận phòng",
        "COMPLETED" => "Hoàn thành",
        "CANCELLED" => "Đã hủy",
        _ => status
    };

    public static string ToVietnameseRefund(string refund) => refund switch
    {
        "WAITING_REFUND" => "Chờ hoàn tiền",
        "REFUNDED" => "Đã hoàn tiền",
        "NO_REFUND" => "Không hoàn tiền",
        _ => "Không có"
    };
}

public class PaymentSummaryDto
{
    public long Id { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    public long BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public string MethodDisplay => Type switch
    {
        "REFUND" => "Hoàn tiền",
        "DEPOSIT" => "Tiền cọc (còn lại thu trực tiếp)",
        _ => "Thanh toán toàn bộ"
    };

    public string StatusDisplay => Status switch
    {
        "PENDING" => "Chờ xác nhận",
        "CONFIRMED" => Type == "REFUND" ? "Đã hoàn tiền" : "Đã xác nhận",
        "CANCELLED" => "Đã hủy",
        "FAILED" => "Thất bại",
        _ => Status
    };
}

public class DashboardStatsDto
{
    public int TotalRooms { get; set; }
    public int ActiveRooms { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingPaymentsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalStaff { get; set; }
}
