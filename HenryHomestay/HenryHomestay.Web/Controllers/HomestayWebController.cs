using System.Text.Json;
using HenryHomestay.BLL;
using HenryHomestay.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HenryHomestay.Web.Controllers;

public class HomestayWebController : Controller
{
    private readonly AuthService _authService;
    private readonly RoomService _roomService;
    private readonly BookingService _bookingService;
    private readonly SystemService _systemService;
    private readonly IWebHostEnvironment _env;

    private const string CookieUserId = "HenryUserId";

    public HomestayWebController(
        AuthService authService,
        RoomService roomService,
        BookingService bookingService,
        SystemService systemService,
        IWebHostEnvironment env)
    {
        _authService = authService;
        _roomService = roomService;
        _bookingService = bookingService;
        _systemService = systemService;
        _env = env;
    }

    private long? GetCurrentUserId()
    {
        if (Request.Cookies.TryGetValue(CookieUserId, out var raw) && long.TryParse(raw, out var uid) && uid > 0)
        {
            var user = _authService.GetUserById(uid);
            if (user == null || user.IsLocked)
            {
                Response.Cookies.Delete(CookieUserId);
                return null;
            }
            return uid;
        }
        return null;
    }

    private IActionResult RenderTemplatePage(string fileName)
    {
        string path = Path.Combine(_env.ContentRootPath, "Templates", fileName);
        if (!System.IO.File.Exists(path))
        {
            path = Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", fileName);
        }
        if (!System.IO.File.Exists(path))
        {
            path = Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "henry-homestay", fileName);
        }
        if (!System.IO.File.Exists(path))
            return NotFound($"Không tìm thấy trang {fileName}");

        string html = System.IO.File.ReadAllText(path);

        // Ensure data.js and henry-live.js are loaded on every page
        string scriptsToInject = "";
        long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (!html.Contains("assets/js/data.js"))
        {
            scriptsToInject += $"<script src=\"/assets/js/data.js?v={ts}\"></script>\n";
        }
        if (!html.Contains("assets/js/henry-live.js"))
        {
            scriptsToInject += $"<script defer src=\"/assets/js/henry-live.js?v={ts}\"></script>\n";
        }

        int bodyEnd = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyEnd >= 0)
        {
            html = html.Insert(bodyEnd, scriptsToInject);
        }
        else
        {
            html += scriptsToInject;
        }

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("/")]
    [HttpGet("/index.html")]
    [HttpGet("/Home")]
    [HttpGet("/Home/Index")]
    public IActionResult Index() => RenderTemplatePage("index.html");

    [HttpGet("/index-guest.html")]
    [HttpGet("/Home/Guest")]
    public IActionResult IndexGuest() => RenderTemplatePage(
        System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "Templates", "index-guest.html"))
        || System.IO.File.Exists(Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", "index-guest.html"))
            ? "index-guest.html"
            : "index.html");

    [HttpGet("/rooms.html")]
    [HttpGet("/Rooms")]
    [HttpGet("/Rooms/Index")]
    public IActionResult Rooms() => RenderTemplatePage("rooms.html");

    [HttpGet("/room-detail.html")]
    [HttpGet("/Rooms/Detail")]
    public IActionResult RoomDetail() => RenderTemplatePage("room-detail.html");

    [HttpGet("/booking-detail.html")]
    [HttpGet("/Bookings/Detail")]
    public IActionResult BookingDetail() => RenderTemplatePage("booking-detail.html");

    [HttpGet("/food.html")]
    [HttpGet("/Home/Food")]
    public IActionResult Food() => RenderTemplatePage("food.html");

    [HttpGet("/contact.html")]
    [HttpGet("/Home/Contact")]
    public IActionResult Contact() => RenderTemplatePage("contact.html");

    [HttpGet("/login.html")]
    [HttpGet("/Account/Login")]
    public IActionResult Login() => RenderTemplatePage("login.html");

    [HttpGet("/register.html")]
    [HttpGet("/Account/Register")]
    public IActionResult Register() => RenderTemplatePage("register.html");

    [HttpGet("/profile.html")]
    [HttpGet("/Account/Profile")]
    public IActionResult Profile() => RenderTemplatePage("profile.html");

    private string? ResolveAdminFilePath(string fileName)
    {
        string path = Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "admin", fileName);
        if (!System.IO.File.Exists(path))
        {
            path = Path.Combine(_env.ContentRootPath, "..", "..", "admin", fileName);
        }
        return System.IO.File.Exists(path) ? path : null;
    }

    [HttpGet("/admin")]
    [HttpGet("/admin/index.html")]
    public IActionResult AdminIndex()
    {
        var path = ResolveAdminFilePath("index.html");
        if (path == null)
            return NotFound();

        string html = System.IO.File.ReadAllText(path)
            .Replace("../user/assets/", "/assets/")
            .Replace("href=\"admin.css\"", "href=\"/admin/admin.css\"")
            .Replace("src=\"admin.js\"", "src=\"/admin/admin.js\"");

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("/admin/admin.css")]
    public IActionResult AdminCss()
    {
        var path = ResolveAdminFilePath("admin.css");
        if (path == null)
            return NotFound();

        string css = System.IO.File.ReadAllText(path);
        Response.Headers["Cache-Control"] = "no-store";
        return Content(css, "text/css; charset=utf-8");
    }

    [HttpGet("/admin/admin.js")]
    public IActionResult AdminJs()
    {
        var path = ResolveAdminFilePath("admin.js");
        if (path == null)
            return NotFound();

        string js = System.IO.File.ReadAllText(path)
            .Replace("../user/assets/", "/assets/")
            .Replace("'../user/' + p.qr", "'/' + p.qr")
            .Replace("'../user/login.html'", "'/login.html'");

        Response.Headers["Cache-Control"] = "no-store";
        return Content(js, "application/javascript; charset=utf-8");
    }

    [HttpGet("/api/settings/qr-image")]
    public IActionResult GetQrImage()
    {
        var settings = _systemService.GetSettings();
        string? rawPath = settings?.QrCodeUrl?.Trim();

        string? resolvedFilePath = null;

        if (!string.IsNullOrEmpty(rawPath))
        {
            // Case 1: Absolute path on disk (e.g. D:\Anhr zalo\xxx.jpg)
            if (Path.IsPathRooted(rawPath) && System.IO.File.Exists(rawPath))
            {
                resolvedFilePath = rawPath;
            }
            else
            {
                // Case 2: Relative path (e.g. assets/images/xxx.jpg or /assets/images/...)
                string rel = rawPath.TrimStart('/', '\\');
                var candidates = new List<string>
                {
                    Path.Combine(_env.WebRootPath, rel),
                    Path.Combine(_env.WebRootPath, "assets", "images", Path.GetFileName(rel)),
                    Path.Combine(DatabaseConfigManager.GetSolutionRoot(), "HenryHomestay.Web", "wwwroot", rel),
                    Path.Combine(DatabaseConfigManager.GetSolutionRoot(), "HenryHomestay.Web", "wwwroot", "assets", "images", Path.GetFileName(rel)),
                    Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", rel),
                    Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", "assets", "images", Path.GetFileName(rel)),
                    Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "henry-homestay", rel),
                    Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "henry-homestay", "assets", "images", Path.GetFileName(rel))
                };

                foreach (var candidate in candidates)
                {
                    if (System.IO.File.Exists(candidate))
                    {
                        resolvedFilePath = candidate;
                        break;
                    }
                }
            }
        }

        // Fallback: Default payment-qr image
        if (resolvedFilePath == null || !System.IO.File.Exists(resolvedFilePath))
        {
            var defaultCandidates = new List<string>
            {
                Path.Combine(_env.WebRootPath, "assets", "images", "payment-qr.svg"),
                Path.Combine(_env.WebRootPath, "assets", "images", "payment-qr.png"),
                Path.Combine(DatabaseConfigManager.GetSolutionRoot(), "HenryHomestay.Web", "wwwroot", "assets", "images", "payment-qr.svg"),
                Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "user", "assets", "images", "payment-qr.svg"),
                Path.Combine(DatabaseConfigManager.GetWorkspaceRoot(), "henry-homestay", "assets", "images", "payment-qr.svg")
            };
            foreach (var candidate in defaultCandidates)
            {
                if (System.IO.File.Exists(candidate))
                {
                    resolvedFilePath = candidate;
                    break;
                }
            }
        }

        if (resolvedFilePath == null || !System.IO.File.Exists(resolvedFilePath))
            return NotFound("Không tìm thấy ảnh QR chuyển khoản.");

        string ext = Path.GetExtension(resolvedFilePath).ToLowerInvariant();
        string contentType = ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return PhysicalFile(resolvedFilePath, contentType);
    }

    // Dynamic /assets/js/data.js generated directly from SQL Server / Shared DB
    [HttpGet("/assets/js/data.js")]
    public IActionResult GetDynamicDataJs()
    {
        var uid = GetCurrentUserId();
        var user = uid.HasValue ? _authService.GetUserById(uid.Value) : null;
        var rooms = _roomService.GetRooms(includeHidden: false, currentUserId: uid);
        var settings = _systemService.GetSettings();
        var userBookings = uid.HasValue
            ? _bookingService.GetBookings(userId: uid.Value)
            : new List<HenryHomestay.Core.Models.BookingSummaryDto>();

        var activeBooking = userBookings.FirstOrDefault();

        // Load booked nights per roomCode for live availability filtering on Web
        using var db = DatabaseConfigManager.CreateDbContext();
        var roomNightRows = db.RoomNights
            .Include(rn => rn.Room)
            .Include(rn => rn.Booking)
            .Where(rn => rn.Room != null && rn.Booking != null && rn.Booking.Status != "CANCELLED")
            .Select(rn => new { rn.Room!.RoomCode, StayDate = rn.StayDate.ToString("yyyy-MM-dd") })
            .ToList();

        var bookedNightsDict = roomNightRows
            .GroupBy(x => x.RoomCode)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StayDate).Distinct().ToList());

        var roomsPayload = rooms.Select(r => new
        {
            id = r.RoomCode,
            dbId = r.Id,
            name = r.Name,
            type = r.TypeCode,
            guests = r.MaxGuests,
            area = r.AreaM2,
            price = r.PricePerNight,
            image = r.PrimaryImage,
            view = r.ViewDescription,
            amenities = r.AmenityCodes,
            bed = r.BedInfo,
            pets = r.AllowPets
        }).ToList();

        var bookingPayload = activeBooking != null
            ? new
            {
                id = activeBooking.BookingCode,
                roomId = activeBooking.RoomCode,
                nights = activeBooking.Nights,
                guests = (int)(activeBooking.Adults + activeBooking.Children),
                nightly = activeBooking.PricePerNight,
                cleaning = 0m,
                discount = 0m,
                paid = activeBooking.TotalAmount
            }
            : new
            {
                id = "HN-270001",
                roomId = "42",
                nights = 2,
                guests = 2,
                nightly = 1190000m,
                cleaning = 0m,
                discount = 0m,
                paid = 2380000m
            };

        var userPayload = user == null ? null : new
        {
            id = user.Id,
            fullName = user.FullName,
            email = user.Email,
            phone = user.Phone,
            cccd = user.Cccd ?? "",
            dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
            role = user.Role
        };

        var userBookingsPayload = userBookings.Select(b => new
        {
            id = b.Id,
            bookingCode = b.BookingCode,
            roomCode = b.RoomCode,
            roomName = b.RoomName,
            roomImage = b.RoomImage,
            roomMeta = b.RoomMeta,
            pricePerNight = b.PricePerNight,
            checkInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
            checkOutDate = b.CheckOutDate.ToString("yyyy-MM-dd"),
            nights = b.Nights,
            adults = b.Adults,
            children = b.Children,
            infants = b.Infants,
            pets = b.Pets,
            totalAmount = b.TotalAmount,
            paidAmount = b.PaidAmount,
            remainingAmount = b.RemainingAmount,
            status = b.Status,
            statusDisplay = b.StatusDisplay,
            refundStatus = b.RefundStatus
        }).ToList();

        var paymentPayload = new
        {
            qr = "/api/settings/qr-image?v=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            receiver = settings.BankAccountHolder,
            account = settings.BankAccountNumber,
            bank = settings.BankName,
            phone = settings.ContactPhone,
            depositRate = settings.DepositPercent
        };

        var savedRoomCodes = rooms.Where(r => r.IsSaved).Select(r => r.RoomCode).ToList();

        string jsonRooms = JsonSerializer.Serialize(roomsPayload);
        string jsonBookedNights = JsonSerializer.Serialize(bookedNightsDict);
        string jsonBooking = JsonSerializer.Serialize(bookingPayload);
        string jsonActiveBooking = JsonSerializer.Serialize(userBookingsPayload.FirstOrDefault());
        string jsonUserBookings = JsonSerializer.Serialize(userBookingsPayload);
        string jsonUser = JsonSerializer.Serialize(userPayload);
        string jsonPayment = JsonSerializer.Serialize(paymentPayload);
        string jsonSaved = JsonSerializer.Serialize(savedRoomCodes);

        string js = $@"/* Dữ liệu động từ CSDL C# (ASP.NET Core MVC + EF Core) */
const HenryData = {{
  rooms: {jsonRooms},
  bookedNights: {jsonBookedNights},
  booking: {jsonBooking},
  activeBooking: {jsonActiveBooking},
  userBookings: {jsonUserBookings},
  currentUser: {jsonUser},
  savedRoomCodes: {jsonSaved},
  payment: {jsonPayment},
  money(value){{return new Intl.NumberFormat('vi-VN').format(value)+' ₫';}}
}};
";
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        return Content(js, "application/javascript; charset=utf-8");
    }

    // ==================== API ENDPOINTS ====================

    public record LoginDto(string Email, string Password);
    [HttpPost("/api/auth/login")]
    public IActionResult ApiLogin([FromBody] LoginDto req)
    {
        var res = _authService.Login(req.Email, req.Password, requireAdminOrStaff: false);
        if (!res.Success || res.Data == null)
            return Json(new { success = false, message = res.Message });

        if (res.Data.IsLocked)
            return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên." });

        Response.Cookies.Append(CookieUserId, res.Data.Id.ToString(), new CookieOptions { HttpOnly = false, Path = "/", Expires = DateTimeOffset.Now.AddDays(7) });
        Response.Cookies.Delete("HenryLoggedOut");
        return Json(new { success = true, message = res.Message, user = new { res.Data.Id, res.Data.FullName, res.Data.Email } });
    }

    public record RegisterDto(string FullName, string Email, string Phone, string Password);
    [HttpPost("/api/auth/register")]
    public IActionResult ApiRegister([FromBody] RegisterDto req)
    {
        var phone = string.IsNullOrWhiteSpace(req.Phone) ? $"09{Random.Shared.Next(10000000, 99999999)}" : req.Phone;
        var res = _authService.RegisterCustomer(req.FullName, req.Email, phone, req.Password);
        if (!res.Success || res.Data == null)
            return Json(new { success = false, message = res.Message });

        Response.Cookies.Append(CookieUserId, res.Data.Id.ToString(), new CookieOptions { HttpOnly = false, Path = "/", Expires = DateTimeOffset.Now.AddDays(7) });
        Response.Cookies.Delete("HenryLoggedOut");
        return Json(new { success = true, message = res.Message });
    }

    [HttpPost("/api/auth/logout")]
    public IActionResult ApiLogout()
    {
        Response.Cookies.Delete(CookieUserId);
        Response.Cookies.Append("HenryLoggedOut", "1", new CookieOptions { Path = "/" });
        return Json(new { success = true, message = "Đã đăng xuất." });
    }

    public record ProfileUpdateDto(string FullName, string Email, string Phone, string? Cccd, string? DateOfBirth);
    [HttpPost("/api/profile/update")]
    public IActionResult ApiUpdateProfile([FromBody] ProfileUpdateDto req)
    {
        var uid = GetCurrentUserId();
        if (!uid.HasValue)
            return Json(new { success = false, message = "Vui lòng đăng nhập để cập nhật hồ sơ." });

        DateTime? dob = null;
        if (!string.IsNullOrWhiteSpace(req.DateOfBirth) && DateTime.TryParse(req.DateOfBirth, out var parsedDob))
            dob = parsedDob;

        var res = _authService.UpdateProfile(uid.Value, req.FullName, req.Email, req.Phone, req.Cccd, dob);
        return Json(new { success = res.Success, message = res.Message });
    }

    public record ChangePwdDto(string CurrentPassword, string NewPassword);
    [HttpPost("/api/profile/password")]
    public IActionResult ApiChangePassword([FromBody] ChangePwdDto req)
    {
        var uid = GetCurrentUserId();
        if (!uid.HasValue)
            return Json(new { success = false, message = "Vui lòng đăng nhập trước." });

        var res = _authService.ChangePassword(uid.Value, req.CurrentPassword, req.NewPassword);
        return Json(new { success = res.Success, message = res.Message });
    }

    public record ToggleSavedDto(string RoomCode);
    [HttpPost("/api/rooms/toggle-saved")]
    public IActionResult ApiToggleSaved([FromBody] ToggleSavedDto req)
    {
        var uid = GetCurrentUserId();
        if (!uid.HasValue)
            return Json(new { success = false, message = "Chưa đăng nhập." });

        var room = _roomService.GetRoomByCodeOrId(req.RoomCode);
        if (room == null)
            return Json(new { success = false, message = "Không tìm thấy phòng." });

        bool isSaved = _roomService.ToggleSavedRoom(uid.Value, room.Id);
        return Json(new { success = true, isSaved });
    }

    public record CreateBookingDto(
        string RoomCode,
        string CheckInDate,
        string CheckOutDate,
        byte Adults,
        byte Children,
        byte Infants,
        byte Pets,
        string GuestName,
        string GuestPhone,
        string GuestEmail,
        string? GuestCccd,
        string? GuestDob,
        string PaymentOption);

    [HttpPost("/api/bookings/create")]
    public IActionResult ApiCreateBooking([FromBody] CreateBookingDto req)
    {
        var uid = GetCurrentUserId();
        if (!uid.HasValue)
            return Json(new { success = false, requireAuth = true, message = "Vui lòng đăng nhập hoặc đăng ký tài khoản để đặt phòng." });

        var room = _roomService.GetRoomByCodeOrId(req.RoomCode);
        if (room == null)
            return Json(new { success = false, message = "Không tìm thấy phòng đã chọn." });

        if (!DateTime.TryParse(req.CheckInDate, out var ci) || !DateTime.TryParse(req.CheckOutDate, out var co))
            return Json(new { success = false, message = "Ngày nhận hoặc trả phòng không hợp lệ." });

        DateTime? dob = null;
        if (!string.IsNullOrWhiteSpace(req.GuestDob) && DateTime.TryParse(req.GuestDob, out var d))
            dob = d;

        var res = _bookingService.CreateBooking(
            uid,
            room.Id,
            ci,
            co,
            req.Adults,
            req.Children,
            req.Infants,
            req.Pets,
            req.GuestName,
            req.GuestPhone,
            req.GuestEmail,
            req.GuestCccd,
            dob,
            req.PaymentOption);

        if (!res.Success || res.Data == null)
            return Json(new { success = false, message = res.Message });

        return Json(new
        {
            success = true,
            message = res.Message,
            booking = new
            {
                id = res.Data.Id,
                bookingCode = res.Data.BookingCode,
                status = res.Data.Status,
                statusDisplay = res.Data.StatusDisplay
            }
        });
    }

    public record CancelBookingDto(long BookingId);
    [HttpPost("/api/bookings/cancel")]
    public IActionResult ApiCancelBooking([FromBody] CancelBookingDto req)
    {
        var uid = GetCurrentUserId();
        if (!uid.HasValue)
            return Json(new { success = false, message = "Vui lòng đăng nhập để hủy đơn đặt phòng." });

        var res = _bookingService.CancelBooking(req.BookingId, uid, "Khách tự hủy đơn trên Web");
        return Json(new { success = res.Success, message = res.Message });
    }

    public record ContactSubmitDto(string FullName, string Phone, string Email, string? ServiceType, string? StayDate, string? Message);
    [HttpPost("/api/contact/submit")]
    public IActionResult ApiSubmitContact([FromBody] ContactSubmitDto req)
    {
        DateTime? stayDate = null;
        if (!string.IsNullOrWhiteSpace(req.StayDate) && DateTime.TryParse(req.StayDate, out var d))
            stayDate = d;

        var res = _systemService.SubmitContactInquiry(req.FullName, req.Phone, req.Email, req.ServiceType, stayDate, req.Message, GetCurrentUserId());
        return Json(new { success = res.Success, message = res.Message });
    }
}
