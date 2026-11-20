using HenryHomestay.Core.Entities;
using HenryHomestay.Core.Models;
using HenryHomestay.DAL;
using Microsoft.EntityFrameworkCore;

namespace HenryHomestay.BLL;

public class AuthService
{
    public OperationResult<User> Login(string account, string password, bool requireAdminOrStaff = false)
    {
        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
            return OperationResult<User>.Fail("Vui lòng nhập đầy đủ tài khoản và mật khẩu.");

        var normalized = account.Trim().ToLowerInvariant();
        var cleanPhone = account.Replace(" ", "").Trim();

        using var db = DatabaseConfigManager.CreateDbContext();
        var user = db.Users.FirstOrDefault(u =>
            u.Email.ToLower() == normalized ||
            u.Phone.Replace(" ", "") == cleanPhone);

        if (user == null || user.PasswordHash != password)
            return OperationResult<User>.Fail("Email/Số điện thoại hoặc mật khẩu không chính xác.");

        if (user.IsLocked)
            return OperationResult<User>.Fail("Tài khoản này đã bị khóa. Vui lòng liên hệ quản trị viên.");

        if (requireAdminOrStaff && user.Role != "ADMIN" && user.Role != "STAFF")
            return OperationResult<User>.Fail("Tài khoản khách hàng không có quyền truy cập hệ thống quản trị Desktop.");

        return OperationResult<User>.Ok(user, $"Xin chào {user.FullName}!");
    }

    public OperationResult<User> RegisterCustomer(string fullName, string email, string phone, string password, string? cccd = null)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            return OperationResult<User>.Fail("Vui lòng điền đầy đủ họ tên, email, số điện thoại và mật khẩu.");

        if (password.Length < 6)
            return OperationResult<User>.Fail("Mật khẩu phải có ít nhất 6 ký tự.");

        var normEmail = email.Trim().ToLowerInvariant();
        var normPhone = phone.Trim();

        using var db = DatabaseConfigManager.CreateDbContext();
        if (db.Users.Any(u => u.Email.ToLower() == normEmail))
            return OperationResult<User>.Fail("Email này đã được sử dụng cho một tài khoản khác.");

        if (db.Users.Any(u => u.Phone == normPhone))
            return OperationResult<User>.Fail("Số điện thoại này đã được đăng ký.");

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = normEmail,
            Phone = normPhone,
            Cccd = string.IsNullOrWhiteSpace(cccd) ? null : cccd.Trim(),
            PasswordHash = password,
            Role = "CUSTOMER",
            IsLocked = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Users.Add(user);
        db.SaveChanges();

        return OperationResult<User>.Ok(user, "Đăng ký tài khoản thành công!");
    }

    public User? GetUserById(long userId)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        return db.Users.FirstOrDefault(u => u.Id == userId && !u.IsLocked);
    }

    public OperationResult<User> UpdateProfile(long userId, string fullName, string email, string phone, string? cccd, DateTime? dob)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return OperationResult<User>.Fail("Không tìm thấy tài khoản.");

        var normEmail = email.Trim().ToLowerInvariant();
        var normPhone = string.IsNullOrWhiteSpace(phone) ? user.Phone : phone.Trim();

        if (db.Users.Any(u => u.Id != userId && u.Email.ToLower() == normEmail))
            return OperationResult<User>.Fail("Email này đã thuộc về tài khoản khác.");

        if (db.Users.Any(u => u.Id != userId && u.Phone == normPhone))
            return OperationResult<User>.Fail("Số điện thoại này đã thuộc về tài khoản khác.");

        user.FullName = fullName.Trim();
        user.Email = normEmail;
        user.Phone = normPhone;
        user.Cccd = string.IsNullOrWhiteSpace(cccd) ? null : cccd.Trim();
        user.DateOfBirth = dob;
        user.UpdatedAt = DateTime.Now;
        db.SaveChanges();

        return OperationResult<User>.Ok(user, "Đã cập nhật hồ sơ cá nhân thành công.");
    }

    public OperationResult ChangePassword(long userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return OperationResult.Fail("Mật khẩu mới phải từ 6 ký tự trở lên.");

        using var db = DatabaseConfigManager.CreateDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return OperationResult.Fail("Không tìm thấy người dùng.");

        if (user.PasswordHash != currentPassword)
            return OperationResult.Fail("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = newPassword;
        user.UpdatedAt = DateTime.Now;
        db.SaveChanges();
        return OperationResult.Ok("Đổi mật khẩu thành công.");
    }
}

public class RoomService
{
    public List<RoomDisplayDto> GetRooms(bool includeHidden = false, long? currentUserId = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var savedRoomIds = currentUserId.HasValue
            ? db.SavedRooms.Where(s => s.UserId == currentUserId.Value).Select(s => s.RoomId).ToHashSet()
            : new HashSet<int>();

        var activeBookings = db.BookingRooms
            .Include(br => br.Booking)
            .Where(br => br.Booking != null && br.Booking.Status != "CANCELLED" && br.Booking.Status != "COMPLETED")
            .ToList();

        var query = db.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.RoomImages)
            .Include(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
            .AsQueryable();

        if (!includeHidden)
            query = query.Where(r => r.Status == "ACTIVE");

        var rooms = query.OrderBy(r => r.RoomCode).ToList();

        return rooms.Select(r =>
        {
            var rBookings = activeBookings.Where(b => b.RoomId == r.Id).Select(b => b.Booking!).ToList();
            string occ = "Chưa ai đặt";
            if (rBookings.Any(b => b.Status == "CHECKED_IN"))
                occ = "Đang hoạt động (có người ở)";
            else if (rBookings.Any())
                occ = "Đã có lịch đặt";

            var primaryImg = r.RoomImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl ?? "photo-02.jpg";

            return new RoomDisplayDto
            {
                Id = r.Id,
                RoomCode = r.RoomCode,
                Name = r.Name,
                RoomTypeId = r.RoomTypeId,
                TypeCode = r.RoomType?.Code ?? "double",
                TypeName = r.RoomType?.Name ?? "Phòng đôi",
                AreaM2 = r.AreaM2,
                BedInfo = r.BedInfo ?? "1 giường Queen",
                ViewDescription = r.ViewDescription ?? "Tầm nhìn thoáng đãng",
                Description = r.Description ?? "",
                MaxGuests = r.MaxGuests,
                AllowPets = r.AllowPets,
                PricePerNight = r.PricePerNight,
                Status = r.Status,
                PrimaryImage = primaryImg,
                AmenityCodes = r.RoomAmenities.Where(a => a.Amenity != null).Select(a => a.Amenity!.Code).ToList(),
                AmenityNames = r.RoomAmenities.Where(a => a.Amenity != null).Select(a => a.Amenity!.Name).ToList(),
                OccupancyStatus = occ,
                IsSaved = savedRoomIds.Contains(r.Id)
            };
        }).ToList();
    }

    public List<RoomDisplayDto> SearchRooms(
        DateTime? checkIn,
        DateTime? checkOut,
        int guests = 1,
        bool hasPets = false,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        List<string>? typeCodes = null,
        List<string>? amenityCodes = null,
        long? currentUserId = null)
    {
        var allRooms = GetRooms(includeHidden: false, currentUserId: currentUserId);

        HashSet<int> busyRoomIds = new();
        if (checkIn.HasValue && checkOut.HasValue && checkOut.Value.Date > checkIn.Value.Date)
        {
            var ci = checkIn.Value.Date;
            var co = checkOut.Value.Date;
            using var db = DatabaseConfigManager.CreateDbContext();
            busyRoomIds = db.RoomNights
                .Include(rn => rn.Booking)
                .Where(rn => rn.StayDate >= ci && rn.StayDate < co && rn.Booking != null && rn.Booking.Status != "CANCELLED")
                .Select(rn => rn.RoomId)
                .ToHashSet();
        }

        return allRooms.Where(r =>
            !busyRoomIds.Contains(r.Id) &&
            r.MaxGuests >= Math.Max(1, guests) &&
            (!hasPets || r.AllowPets) &&
            (!minPrice.HasValue || minPrice.Value <= 0 || r.PricePerNight >= minPrice.Value) &&
            (!maxPrice.HasValue || maxPrice.Value <= 0 || r.PricePerNight <= maxPrice.Value) &&
            (typeCodes == null || typeCodes.Count == 0 || typeCodes.Contains(r.TypeCode)) &&
            (amenityCodes == null || amenityCodes.Count == 0 || amenityCodes.All(a => r.AmenityCodes.Contains(a)))
        ).ToList();
    }

    public RoomDisplayDto? GetRoomByCodeOrId(string? codeOrId, long? currentUserId = null)
    {
        var rooms = GetRooms(includeHidden: true, currentUserId: currentUserId);
        if (string.IsNullOrWhiteSpace(codeOrId))
            return rooms.FirstOrDefault();

        return rooms.FirstOrDefault(r =>
            r.RoomCode.Equals(codeOrId, StringComparison.OrdinalIgnoreCase) ||
            r.Id.ToString() == codeOrId);
    }

    public List<RoomType> GetRoomTypes()
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        return db.RoomTypes.OrderBy(t => t.Id).ToList();
    }

    public List<Amenity> GetAllAmenities()
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        return db.Amenities.OrderBy(a => a.Id).ToList();
    }

    public OperationResult SaveRoom(
        int? id,
        string roomCode,
        string name,
        int roomTypeId,
        int areaM2,
        string bedInfo,
        string viewDesc,
        string description,
        byte maxGuests,
        bool allowPets,
        decimal pricePerNight,
        string status,
        string imageUrl,
        List<string> amenityCodes)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(roomCode))
            return OperationResult.Fail("Tên phòng và mã phòng không được để trống.");
        if (pricePerNight < 0)
            return OperationResult.Fail("Giá phòng mỗi đêm phải lớn hơn hoặc bằng 0.");
        if (maxGuests < 1)
            return OperationResult.Fail("Số khách tối đa phải từ 1 trở lên.");

        using var db = DatabaseConfigManager.CreateDbContext();
        var codeTrim = roomCode.Trim();
        var nameTrim = name.Trim();

        if (db.Rooms.Any(r => r.Id != (id ?? 0) && r.RoomCode == codeTrim))
            return OperationResult.Fail($"Mã phòng '{codeTrim}' đã tồn tại.");
        if (db.Rooms.Any(r => r.Id != (id ?? 0) && r.Name == nameTrim))
            return OperationResult.Fail($"Tên phòng '{nameTrim}' đã tồn tại.");

        Room room;
        if (id.HasValue && id.Value > 0)
        {
            room = db.Rooms
                .Include(r => r.RoomAmenities)
                .Include(r => r.RoomImages)
                .FirstOrDefault(r => r.Id == id.Value)!;
            if (room == null)
                return OperationResult.Fail("Không tìm thấy phòng cần cập nhật.");
        }
        else
        {
            room = new Room { CreatedAt = DateTime.Now };
            db.Rooms.Add(room);
        }

        room.RoomCode = codeTrim;
        room.Name = nameTrim;
        room.RoomTypeId = roomTypeId;
        room.AreaM2 = Math.Max(10, areaM2);
        room.BedInfo = string.IsNullOrWhiteSpace(bedInfo) ? "1 giường Queen" : bedInfo.Trim();
        room.ViewDescription = string.IsNullOrWhiteSpace(viewDesc) ? "Tầm nhìn thành phố" : viewDesc.Trim();
        room.Description = string.IsNullOrWhiteSpace(description)
            ? $"{room.ViewDescription}. {room.BedInfo}, {room.AreaM2} m²."
            : description.Trim();
        room.MaxGuests = maxGuests;
        room.AllowPets = allowPets;
        room.PricePerNight = pricePerNight;
        room.Status = status == "HIDDEN" ? "HIDDEN" : "ACTIVE";
        room.UpdatedAt = DateTime.Now;
        db.SaveChanges();

        // Update primary image
        var currentPrimary = db.RoomImages.FirstOrDefault(i => i.RoomId == room.Id && i.IsPrimary);
        var cleanImg = string.IsNullOrWhiteSpace(imageUrl) ? "photo-02.jpg" : imageUrl.Trim();
        if (currentPrimary != null)
        {
            currentPrimary.ImageUrl = cleanImg;
        }
        else
        {
            db.RoomImages.Add(new RoomImage { RoomId = room.Id, ImageUrl = cleanImg, IsPrimary = true, DisplayOrder = 1 });
        }

        // Update amenities
        var existingAmenities = db.RoomAmenities.Where(ra => ra.RoomId == room.Id).ToList();
        db.RoomAmenities.RemoveRange(existingAmenities);

        var amenities = db.Amenities.Where(a => amenityCodes.Contains(a.Code)).ToList();
        foreach (var a in amenities)
        {
            db.RoomAmenities.Add(new RoomAmenity { RoomId = room.Id, AmenityId = a.Id });
        }

        db.SaveChanges();
        return OperationResult.Ok(id.HasValue ? "Đã lưu thay đổi thông tin phòng." : "Đã thêm phòng mới thành công.");
    }

    public OperationResult ToggleRoomVisibility(int roomId)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var room = db.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null) return OperationResult.Fail("Không tìm thấy phòng.");

        room.Status = room.Status == "ACTIVE" ? "HIDDEN" : "ACTIVE";
        room.UpdatedAt = DateTime.Now;
        db.SaveChanges();
        return OperationResult.Ok(room.Status == "ACTIVE" ? "Đã hiển thị phòng trên website." : "Đã ẩn phòng khỏi website.");
    }

    public OperationResult DeleteRoom(int roomId)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var room = db.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null) return OperationResult.Fail("Không tìm thấy phòng.");

        if (db.BookingRooms.Any(br => br.RoomId == roomId))
            return OperationResult.Fail("Phòng này đã có lịch sử đơn đặt phòng nên không thể xóa vĩnh viễn. Bạn có thể chuyển sang trạng thái 'Ẩn'.");

        db.Rooms.Remove(room);
        db.SaveChanges();
        return OperationResult.Ok($"Đã xóa phòng {room.Name}.");
    }

    public bool ToggleSavedRoom(long userId, int roomId)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var existing = db.SavedRooms.FirstOrDefault(s => s.UserId == userId && s.RoomId == roomId);
        if (existing != null)
        {
            db.SavedRooms.Remove(existing);
            db.SaveChanges();
            return false;
        }
        db.SavedRooms.Add(new SavedRoom { UserId = userId, RoomId = roomId, CreatedAt = DateTime.Now });
        db.SaveChanges();
        return true;
    }
}

public class BookingService
{
    public List<BookingSummaryDto> GetBookings(
        string? keyword = null,
        string? status = null,
        DateTime? checkInDate = null,
        int? roomId = null,
        long? userId = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var query = db.Bookings
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomImages)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(b => b.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == status);

        if (checkInDate.HasValue)
        {
            var d = checkInDate.Value.Date;
            query = query.Where(b => b.CheckInDate == d);
        }

        if (roomId.HasValue && roomId.Value > 0)
            query = query.Where(b => b.BookingRooms.Any(br => br.RoomId == roomId.Value));

        var list = query.OrderByDescending(b => b.CheckInDate).ThenByDescending(b => b.Id).ToList();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLowerInvariant();
            list = list.Where(b =>
                b.BookingCode.ToLowerInvariant().Contains(kw) ||
                b.GuestName.ToLowerInvariant().Contains(kw) ||
                b.GuestPhone.ToLowerInvariant().Contains(kw)).ToList();
        }

        return list.Select(MapToSummaryDto).ToList();
    }

    public BookingSummaryDto? GetBookingByCodeOrId(string? code = null, long? id = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var query = db.Bookings
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomImages)
            .AsQueryable();

        Booking? bk = null;
        if (id.HasValue && id.Value > 0)
            bk = query.FirstOrDefault(b => b.Id == id.Value);
        else if (!string.IsNullOrWhiteSpace(code))
            bk = query.FirstOrDefault(b => b.BookingCode == code.Trim());
        else
            bk = query.OrderByDescending(b => b.Id).FirstOrDefault();

        return bk == null ? null : MapToSummaryDto(bk);
    }

    private static BookingSummaryDto MapToSummaryDto(Booking b)
    {
        var br = b.BookingRooms.FirstOrDefault();
        var rm = br?.Room;
        var img = rm?.RoomImages.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl ?? "photo-03.jpg";
        var nights = Math.Max(1, (int)(b.CheckOutDate.Date - b.CheckInDate.Date).TotalDays);

        return new BookingSummaryDto
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            UserId = b.UserId,
            CustomerName = b.GuestName,
            CustomerPhone = b.GuestPhone,
            CustomerEmail = b.GuestEmail,
            CustomerCccd = b.GuestCccd ?? "",
            GuestDob = b.GuestDob,
            RoomId = rm?.Id ?? 0,
            RoomCode = rm?.RoomCode ?? "",
            RoomName = rm?.Name ?? "—",
            RoomImage = img,
            RoomMeta = rm != null ? $"{rm.ViewDescription} · {rm.AreaM2} m² · {rm.BedInfo}" : "",
            PricePerNight = br?.PricePerNight ?? (rm?.PricePerNight ?? 0),
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            Nights = nights,
            Adults = b.Adults,
            Children = b.Children,
            Infants = b.Infants,
            Pets = b.Pets,
            TotalAmount = b.TotalAmount,
            PaymentOption = b.PaymentOption,
            DepositPercent = b.DepositPercent,
            DepositAmount = b.DepositAmount,
            PaidAmount = b.PaidAmount,
            RemainingAmount = Math.Max(0, b.TotalAmount - b.PaidAmount),
            Status = b.Status,
            RefundStatus = b.RefundStatus,
            CreatedAt = b.CreatedAt,
            CancelledAt = b.CancelledAt
        };
    }

    public OperationResult<BookingSummaryDto> CreateBooking(
        long? userId,
        int roomId,
        DateTime checkIn,
        DateTime checkOut,
        byte adults,
        byte children,
        byte infants,
        byte pets,
        string guestName,
        string guestPhone,
        string guestEmail,
        string? guestCccd,
        DateTime? guestDob,
        string paymentOption)
    {
        var ci = checkIn.Date;
        var co = checkOut.Date;
        if (co <= ci)
            return OperationResult<BookingSummaryDto>.Fail("Ngày trả phòng phải sau ngày nhận phòng.");
        if (ci < DateTime.Today)
            return OperationResult<BookingSummaryDto>.Fail("Ngày nhận phòng không được nằm trong quá khứ.");
        if (string.IsNullOrWhiteSpace(guestName) || string.IsNullOrWhiteSpace(guestPhone) || string.IsNullOrWhiteSpace(guestEmail))
            return OperationResult<BookingSummaryDto>.Fail("Vui lòng nhập đầy đủ họ tên, số điện thoại và email.");

        using var db = DatabaseConfigManager.CreateDbContext();
        if (userId.HasValue && userId.Value > 0)
        {
            var authUser = db.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (authUser == null)
                return OperationResult<BookingSummaryDto>.Fail("Không tìm thấy tài khoản người dùng.");
            if (authUser.IsLocked)
                return OperationResult<BookingSummaryDto>.Fail("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        var room = db.Rooms.Include(r => r.RoomImages).FirstOrDefault(r => r.Id == roomId);
        if (room == null || room.Status != "ACTIVE")
            return OperationResult<BookingSummaryDto>.Fail("Phòng không tồn tại hoặc đang tạm ngưng nhận khách.");

        int totalGuests = Math.Max(1, adults + children);
        if (totalGuests > room.MaxGuests)
            return OperationResult<BookingSummaryDto>.Fail($"{room.Name} chỉ nhận tối đa {room.MaxGuests} khách (đơn có {totalGuests} khách).");

        if (pets > 0 && !room.AllowPets)
            return OperationResult<BookingSummaryDto>.Fail($"{room.Name} không cho phép mang theo thú cưng.");

        // Check room_nights clash
        var clash = db.RoomNights
            .Include(rn => rn.Booking)
            .FirstOrDefault(rn => rn.RoomId == roomId && rn.StayDate >= ci && rn.StayDate < co && rn.Booking != null && rn.Booking.Status != "CANCELLED");

        if (clash != null)
            return OperationResult<BookingSummaryDto>.Fail($"Phòng {room.Name} đã có khách đặt trong khoảng ngày {clash.StayDate:dd/MM/yyyy} (Mã đơn: {clash.Booking!.BookingCode}). Vui lòng chọn ngày hoặc phòng khác.");

        // Resolve customer user if not logged in
        long finalUserId = userId ?? 0;
        if (finalUserId <= 0)
        {
            var existingUser = db.Users.FirstOrDefault(u => u.Email.ToLower() == guestEmail.Trim().ToLower() || u.Phone == guestPhone.Trim());
            if (existingUser != null)
            {
                if (existingUser.IsLocked)
                    return OperationResult<BookingSummaryDto>.Fail("Tài khoản khách hàng này đã bị khóa. Không thể tạo đơn đặt phòng.");
                finalUserId = existingUser.Id;
            }
            else
            {
                var guestUser = new User
                {
                    FullName = guestName.Trim(),
                    Email = guestEmail.Trim().ToLowerInvariant(),
                    Phone = guestPhone.Trim(),
                    Cccd = guestCccd?.Trim(),
                    DateOfBirth = guestDob,
                    PasswordHash = "123456",
                    Role = "CUSTOMER"
                };
                db.Users.Add(guestUser);
                db.SaveChanges();
                finalUserId = guestUser.Id;
            }
        }

        var settings = db.SystemSettings.FirstOrDefault() ?? new SystemSetting();
        int nights = (int)(co - ci).TotalDays;
        decimal total = nights * room.PricePerNight;
        string opt = string.Equals(paymentOption, "DEPOSIT", StringComparison.OrdinalIgnoreCase) ? "DEPOSIT" : "FULL";
        decimal depAmt = opt == "DEPOSIT" ? Math.Ceiling(total * settings.DepositPercent / 100m) : 0m;
        decimal payNow = opt == "DEPOSIT" ? depAmt : total;

        int nextNum = 270001 + db.Bookings.Count() + 1;
        string code = $"HN-{nextNum}";
        while (db.Bookings.Any(b => b.BookingCode == code))
        {
            nextNum++;
            code = $"HN-{nextNum}";
        }

        var bk = new Booking
        {
            BookingCode = code,
            UserId = finalUserId,
            GuestName = guestName.Trim(),
            GuestPhone = guestPhone.Trim(),
            GuestEmail = guestEmail.Trim(),
            GuestCccd = string.IsNullOrWhiteSpace(guestCccd) ? null : guestCccd.Trim(),
            GuestDob = guestDob,
            CheckInDate = ci,
            CheckOutDate = co,
            Adults = Math.Max((byte)1, adults),
            Children = children,
            Infants = infants,
            Pets = pets,
            TotalAmount = total,
            PaymentOption = opt,
            DepositPercent = settings.DepositPercent,
            DepositAmount = depAmt,
            PaidAmount = 0,
            Status = "PENDING_PAYMENT",
            ExpiresAt = DateTime.Now.AddMinutes(settings.HoldMinutes),
            RefundStatus = "NONE",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        db.Bookings.Add(bk);
        db.SaveChanges();

        db.BookingRooms.Add(new BookingRoom
        {
            BookingId = bk.Id,
            RoomId = room.Id,
            PricePerNight = room.PricePerNight
        });

        for (var d = ci; d < co; d = d.AddDays(1))
        {
            db.RoomNights.Add(new RoomNight
            {
                RoomId = room.Id,
                StayDate = d,
                BookingId = bk.Id
            });
        }

        int txNum = db.Payments.Count() + 1;
        string payCode = $"GD-{txNum}";
        while (db.Payments.Any(p => p.PaymentCode == payCode))
        {
            txNum++;
            payCode = $"GD-{txNum}";
        }
        db.Payments.Add(new Payment
        {
            PaymentCode = payCode,
            BookingId = bk.Id,
            Amount = payNow,
            Type = opt == "DEPOSIT" ? "DEPOSIT" : "FULL",
            Method = "BANK_TRANSFER",
            Status = "PENDING",
            Note = "Khách xác nhận chuyển khoản trên Web",
            CreatedAt = DateTime.Now
        });
        db.SaveChanges();

        var summary = GetBookingByCodeOrId(bk.BookingCode)!;
        return OperationResult<BookingSummaryDto>.Ok(summary, $"Đặt phòng thành công! Mã đơn của bạn là {bk.BookingCode}.");
    }

    public OperationResult UpdateBookingScheduleOrRoom(long bookingId, DateTime newCheckIn, DateTime newCheckOut, int newRoomId)
    {
        var ci = newCheckIn.Date;
        var co = newCheckOut.Date;
        if (co <= ci)
            return OperationResult.Fail("Ngày trả phòng phải sau ngày nhận phòng.");

        using var db = DatabaseConfigManager.CreateDbContext();
        var bk = db.Bookings
            .Include(b => b.BookingRooms)
            .Include(b => b.RoomNights)
            .FirstOrDefault(b => b.Id == bookingId);

        if (bk == null)
            return OperationResult.Fail("Không tìm thấy đơn đặt phòng.");

        if (bk.Status == "CANCELLED" || bk.Status == "COMPLETED")
            return OperationResult.Fail("Không thể sửa đơn đã hủy hoặc đã hoàn thành.");

        if (ci < DateTime.Today && ci != bk.CheckInDate.Date)
            return OperationResult.Fail("Không thể đổi sang ngày nhận phòng đã qua.");

        var newRoom = db.Rooms.FirstOrDefault(r => r.Id == newRoomId);
        if (newRoom == null)
            return OperationResult.Fail("Không tìm thấy phòng mới.");

        int currentRoomId = bk.BookingRooms.FirstOrDefault()?.RoomId ?? 0;
        bool changedRoom = currentRoomId != newRoomId;

        // Check clash in bookings / room_nights excluding current booking
        var clashBooking = db.Bookings
            .Include(b => b.BookingRooms)
            .FirstOrDefault(b =>
                b.Id != bk.Id &&
                b.Status != "CANCELLED" &&
                b.BookingRooms.Any(br => br.RoomId == newRoomId) &&
                b.CheckInDate < co &&
                ci < b.CheckOutDate);

        if (clashBooking != null)
        {
            return OperationResult.Fail(changedRoom
                ? $"Không thể đổi phòng: {newRoom.Name} đã có đơn {clashBooking.BookingCode} ({clashBooking.CheckInDate:dd/MM/yyyy} – {clashBooking.CheckOutDate:dd/MM/yyyy}) trùng ngày nhận phòng."
                : $"Không thể đổi ngày: ngày mới trùng lịch với đơn {clashBooking.BookingCode} ({clashBooking.CheckInDate:dd/MM/yyyy} – {clashBooking.CheckOutDate:dd/MM/yyyy}).");
        }

        if (bk.Adults + bk.Children > newRoom.MaxGuests)
            return OperationResult.Fail($"{newRoom.Name} chỉ nhận tối đa {newRoom.MaxGuests} người, đơn này có {bk.Adults + bk.Children} khách.");

        if (bk.Pets > 0 && !newRoom.AllowPets)
            return OperationResult.Fail($"{newRoom.Name} không cho phép mang thú cưng.");

        // Update schedule & room_nights
        db.RoomNights.RemoveRange(bk.RoomNights);
        db.BookingRooms.RemoveRange(bk.BookingRooms);
        db.SaveChanges();

        int nights = (int)(co - ci).TotalDays;
        bk.CheckInDate = ci;
        bk.CheckOutDate = co;
        bk.TotalAmount = nights * newRoom.PricePerNight;
        if (bk.PaymentOption == "DEPOSIT")
            bk.DepositAmount = Math.Ceiling(bk.TotalAmount * bk.DepositPercent / 100m);
        bk.UpdatedAt = DateTime.Now;

        db.BookingRooms.Add(new BookingRoom
        {
            BookingId = bk.Id,
            RoomId = newRoom.Id,
            PricePerNight = newRoom.PricePerNight
        });

        for (var d = ci; d < co; d = d.AddDays(1))
        {
            db.RoomNights.Add(new RoomNight
            {
                RoomId = newRoom.Id,
                StayDate = d,
                BookingId = bk.Id
            });
        }

        // Update pending payment amount if still pending
        var pendingPay = db.Payments.FirstOrDefault(p => p.BookingId == bk.Id && p.Status == "PENDING");
        if (pendingPay != null)
        {
            pendingPay.Amount = bk.PaymentOption == "DEPOSIT" ? bk.DepositAmount : bk.TotalAmount;
        }

        db.SaveChanges();
        return OperationResult.Ok($"Đã cập nhật đơn {bk.BookingCode} thành công.");
    }

    public OperationResult AdvanceBookingStatus(long bookingId, string targetStatus)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var bk = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (bk == null) return OperationResult.Fail("Không tìm thấy đơn đặt phòng.");

        bk.Status = targetStatus;
        if (targetStatus == "COMPLETED")
        {
            bk.PaidAmount = bk.TotalAmount;
        }
        bk.UpdatedAt = DateTime.Now;
        db.SaveChanges();

        return OperationResult.Ok($"Đã chuyển trạng thái đơn {bk.BookingCode} sang '{BookingSummaryDto.ToVietnameseStatus(targetStatus)}'.");
    }

    public OperationResult CancelBooking(long bookingId, long? cancelledByUserId = null, string? reason = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var bk = db.Bookings
            .Include(b => b.RoomNights)
            .Include(b => b.Payments)
            .FirstOrDefault(b => b.Id == bookingId);

        if (bk == null)
            return OperationResult.Fail("Không tìm thấy đơn đặt phòng.");

        if (bk.Status == "CANCELLED" || bk.Status == "COMPLETED")
            return OperationResult.Fail("Đơn này đã hủy hoặc đã hoàn thành.");

        var settings = db.SystemSettings.FirstOrDefault() ?? new SystemSetting();
        var checkIn14h = bk.CheckInDate.Date.AddHours(14);
        double hoursUntilCheckIn = (checkIn14h - DateTime.Now).TotalHours;

        bk.Status = "CANCELLED";
        bk.CancelledAt = DateTime.Now;
        bk.CancelledBy = cancelledByUserId;
        bk.CancelReason = string.IsNullOrWhiteSpace(reason) ? "Khách liên hệ hủy đơn" : reason.Trim();

        if (bk.PaidAmount > 0)
        {
            bk.RefundStatus = hoursUntilCheckIn >= settings.ChangeCancelHours ? "WAITING_REFUND" : "NO_REFUND";
        }
        else
        {
            bk.RefundStatus = "NONE";
        }

        // Release room nights so other guests can book
        db.RoomNights.RemoveRange(bk.RoomNights);

        // Cancel pending payments
        foreach (var p in bk.Payments.Where(p => p.Status == "PENDING"))
        {
            p.Status = "CANCELLED";
        }

        bk.UpdatedAt = DateTime.Now;
        db.SaveChanges();

        string msg = bk.RefundStatus == "WAITING_REFUND"
            ? $"Đã hủy đơn {bk.BookingCode} (trước {settings.ChangeCancelHours}h nhận phòng). Cần hoàn tiền {bk.PaidAmount:N0} ₫ cho khách."
            : $"Đã hủy đơn {bk.BookingCode}.";
        return OperationResult.Ok(msg);
    }
}

public class PaymentService
{
    public List<PaymentSummaryDto> GetPayments(bool onlyPending = false)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var query = db.Payments
            .Include(p => p.Booking)
            .AsQueryable();

        if (onlyPending)
            query = query.Where(p => p.Status == "PENDING");

        var list = query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).ToList();

        return list.Select(p => new PaymentSummaryDto
        {
            Id = p.Id,
            PaymentCode = string.IsNullOrWhiteSpace(p.PaymentCode) ? $"GD-{p.Id}" : p.PaymentCode,
            BookingId = p.BookingId,
            BookingCode = p.Booking?.BookingCode ?? "—",
            CustomerName = p.Booking?.GuestName ?? "—",
            CustomerPhone = p.Booking?.GuestPhone ?? "—",
            Amount = p.Type == "REFUND" ? -Math.Abs(p.Amount) : p.Amount,
            Type = p.Type,
            Method = p.Method,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            ConfirmedAt = p.ConfirmedAt
        }).ToList();
    }

    public OperationResult ConfirmPayment(long paymentId, long? staffUserId = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var payment = db.Payments.Include(p => p.Booking).FirstOrDefault(p => p.Id == paymentId);
        if (payment == null)
            return OperationResult.Fail("Không tìm thấy giao dịch thanh toán.");

        if (payment.Status == "CONFIRMED")
            return OperationResult.Fail("Giao dịch này đã được xác nhận trước đó.");
        if (payment.Status == "CANCELLED" || payment.Booking?.Status == "CANCELLED")
            return OperationResult.Fail("Giao dịch hoặc đơn đặt phòng này đã bị hủy, không thể xác nhận.");

        payment.Status = "CONFIRMED";
        payment.ConfirmedBy = staffUserId;
        payment.ConfirmedAt = DateTime.Now;

        if (payment.Booking != null)
        {
            payment.Booking.PaidAmount = Math.Min(payment.Booking.TotalAmount, payment.Booking.PaidAmount + payment.Amount);
            if (payment.Booking.Status == "PENDING_PAYMENT")
                payment.Booking.Status = "CONFIRMED";
            payment.Booking.UpdatedAt = DateTime.Now;
        }

        db.SaveChanges();
        return OperationResult.Ok($"Đã xác nhận thanh toán đơn {payment.Booking?.BookingCode}.");
    }

    public OperationResult ConfirmRefund(long bookingId, long? staffUserId = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var bk = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (bk == null)
            return OperationResult.Fail("Không tìm thấy đơn đặt phòng.");

        if (bk.Status != "CANCELLED" || bk.RefundStatus != "WAITING_REFUND")
            return OperationResult.Fail("Đơn này không nằm trong trạng thái chờ hoàn tiền.");

        bk.RefundStatus = "REFUNDED";
        bk.UpdatedAt = DateTime.Now;

        int nextSeq = db.Payments.Count() + 1;
        string refundPayCode = $"GD-{nextSeq}";
        while (db.Payments.Any(p => p.PaymentCode == refundPayCode))
        {
            nextSeq++;
            refundPayCode = $"GD-{nextSeq}";
        }
        db.Payments.Add(new Payment
        {
            PaymentCode = refundPayCode,
            BookingId = bk.Id,
            Amount = bk.PaidAmount,
            Type = "REFUND",
            Method = "BANK_TRANSFER",
            Status = "CONFIRMED",
            Note = $"Hoàn tiền đơn hủy {bk.BookingCode}",
            ConfirmedBy = staffUserId,
            ConfirmedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        });

        db.SaveChanges();
        return OperationResult.Ok($"Đã xác nhận hoàn tiền {bk.PaidAmount:N0} ₫ cho đơn {bk.BookingCode}.");
    }
}

public class UserService
{
    public List<User> GetCustomers(string? keyword = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var query = db.Users
            .Include(u => u.Bookings)
            .Where(u => u.Role == "CUSTOMER")
            .AsQueryable();

        var list = query.OrderByDescending(u => u.CreatedAt).ToList();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLowerInvariant();
            list = list.Where(u =>
                u.FullName.ToLowerInvariant().Contains(kw) ||
                u.Phone.ToLowerInvariant().Contains(kw) ||
                u.Email.ToLowerInvariant().Contains(kw)).ToList();
        }
        return list;
    }

    public List<User> GetStaffMembers(string? keyword = null)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var list = db.Users
            .Where(u => u.Role == "ADMIN" || u.Role == "STAFF")
            .OrderBy(u => u.Id)
            .ToList();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLowerInvariant();
            list = list.Where(u =>
                u.FullName.ToLowerInvariant().Contains(kw) ||
                (u.StaffPosition ?? "").ToLowerInvariant().Contains(kw) ||
                u.Phone.ToLowerInvariant().Contains(kw) ||
                u.Email.ToLowerInvariant().Contains(kw)).ToList();
        }
        return list;
    }

    public OperationResult SaveStaff(
        long? id,
        string fullName,
        string staffPosition,
        string phone,
        string email,
        DateTime hireDate,
        string? password = null)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email))
            return OperationResult.Fail("Họ tên, số điện thoại và email không được để trống.");

        var normEmail = email.Trim().ToLowerInvariant();
        var normPhone = phone.Trim();

        using var db = DatabaseConfigManager.CreateDbContext();
        if (db.Users.Any(u => u.Id != (id ?? 0) && u.Email.ToLower() == normEmail))
            return OperationResult.Fail("Email này đã tồn tại trên hệ thống.");
        if (db.Users.Any(u => u.Id != (id ?? 0) && u.Phone == normPhone))
            return OperationResult.Fail("Số điện thoại này đã tồn tại trên hệ thống.");

        User staff;
        if (id.HasValue && id.Value > 0)
        {
            staff = db.Users.FirstOrDefault(u => u.Id == id.Value)!;
            if (staff == null) return OperationResult.Fail("Không tìm thấy nhân viên.");
        }
        else
        {
            staff = new User
            {
                PasswordHash = string.IsNullOrWhiteSpace(password) ? "123456" : password.Trim(),
                IsLocked = false,
                CreatedAt = DateTime.Now
            };
            db.Users.Add(staff);
        }

        staff.FullName = fullName.Trim();
        staff.StaffPosition = staffPosition.Trim();
        staff.Role = staffPosition.Trim() == "Quản lý" ? "ADMIN" : "STAFF";
        staff.Phone = normPhone;
        staff.Email = normEmail;
        staff.HireDate = hireDate.Date;
        if (!string.IsNullOrWhiteSpace(password))
            staff.PasswordHash = password.Trim();
        staff.UpdatedAt = DateTime.Now;

        db.SaveChanges();
        return OperationResult.Ok(id.HasValue ? "Đã lưu thông tin nhân viên." : "Đã thêm nhân viên mới (mật khẩu mặc định: 123456).");
    }

    public OperationResult ToggleUserLock(long userId)
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return OperationResult.Fail("Không tìm thấy tài khoản.");

        user.IsLocked = !user.IsLocked;
        user.UpdatedAt = DateTime.Now;
        db.SaveChanges();

        return OperationResult.Ok(user.IsLocked ? $"Đã khóa tài khoản {user.FullName}." : $"Đã mở khóa tài khoản {user.FullName}.");
    }
}

public class SystemService
{
    public SystemSetting GetSettings()
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        return db.SystemSettings.FirstOrDefault() ?? new SystemSetting();
    }

    public OperationResult SaveSettings(string bankName, string accountNumber, string accountHolder, string contactPhone, byte depositPercent, string? qrUrl = null, long? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(bankName) || string.IsNullOrWhiteSpace(accountNumber) ||
            string.IsNullOrWhiteSpace(accountHolder) || string.IsNullOrWhiteSpace(contactPhone))
            return OperationResult.Fail("Vui lòng nhập đầy đủ thông tin ngân hàng và số điện thoại liên hệ.");

        if (depositPercent < 1 || depositPercent > 100)
            return OperationResult.Fail("Tỷ lệ đặt cọc phải nằm trong khoảng 1% – 100%.");

        using var db = DatabaseConfigManager.CreateDbContext();
        var setting = db.SystemSettings.FirstOrDefault(s => s.Id == 1);
        if (setting == null)
        {
            setting = new SystemSetting { Id = 1 };
            db.SystemSettings.Add(setting);
        }

        setting.BankName = bankName.Trim();
        setting.BankAccountNumber = accountNumber.Trim();
        setting.BankAccountHolder = accountHolder.Trim();
        setting.ContactPhone = contactPhone.Trim();
        setting.DepositPercent = depositPercent;
        if (!string.IsNullOrWhiteSpace(qrUrl))
            setting.QrCodeUrl = qrUrl.Trim();
        setting.UpdatedBy = updatedBy;
        setting.UpdatedAt = DateTime.Now;

        db.SaveChanges();
        return OperationResult.Ok("Đã lưu cấu hình hệ thống thành công.");
    }

    public OperationResult ResetDefaultSettings()
    {
        return SaveSettings("MB Bank", "0708670042", "Võ Văn Việt", "0795443500", 30, "assets/images/payment-qr.svg");
    }

    public OperationResult SubmitContactInquiry(string fullName, string phone, string email, string? serviceType, DateTime? stayDate, string? message, long? userId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email))
            return OperationResult.Fail("Vui lòng điền họ tên, số điện thoại và email.");

        using var db = DatabaseConfigManager.CreateDbContext();
        db.ContactInquiries.Add(new ContactInquiry
        {
            UserId = userId,
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            Email = email.Trim(),
            ServiceType = serviceType,
            ExpectedStayDate = stayDate,
            Message = message,
            Status = "NEW",
            CreatedAt = DateTime.Now
        });
        db.SaveChanges();
        return OperationResult.Ok("Cảm ơn bạn! HENRY đã tiếp nhận thông tin và sẽ phản hồi trong thời gian sớm nhất.");
    }

    public DashboardStatsDto GetDashboardStats()
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        var bookings = db.Bookings.ToList();
        var rooms = db.Rooms.ToList();
        var payments = db.Payments.ToList();
        var now = DateTime.Now;

        decimal totalRev = bookings.Where(b => b.Status != "CANCELLED").Sum(b => b.PaidAmount);
        decimal monthRev = bookings.Where(b => b.Status != "CANCELLED" && b.CreatedAt.Month == now.Month && b.CreatedAt.Year == now.Year).Sum(b => b.PaidAmount);

        return new DashboardStatsDto
        {
            TotalRooms = rooms.Count,
            ActiveRooms = rooms.Count(r => r.Status == "ACTIVE"),
            TotalBookings = bookings.Count,
            PendingBookings = bookings.Count(b => b.Status == "PENDING_PAYMENT"),
            ConfirmedBookings = bookings.Count(b => b.Status == "CONFIRMED"),
            CheckedInBookings = bookings.Count(b => b.Status == "CHECKED_IN"),
            CompletedBookings = bookings.Count(b => b.Status == "COMPLETED"),
            CancelledBookings = bookings.Count(b => b.Status == "CANCELLED"),
            PendingPaymentsCount = payments.Count(p => p.Status == "PENDING"),
            TotalRevenue = totalRev,
            MonthlyRevenue = monthRev,
            TotalCustomers = db.Users.Count(u => u.Role == "CUSTOMER"),
            TotalStaff = db.Users.Count(u => u.Role == "ADMIN" || u.Role == "STAFF")
        };
    }
}
