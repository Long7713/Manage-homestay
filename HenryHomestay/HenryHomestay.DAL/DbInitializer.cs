using HenryHomestay.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HenryHomestay.DAL;

public static class DbInitializer
{
    public static void Initialize()
    {
        using var db = DatabaseConfigManager.CreateDbContext();
        db.Database.EnsureCreated();

        if (db.RoomTypes.Any() || db.Rooms.Any())
        {
            return;
        }

        var today = DateTime.Today;

        // 1. System Settings
        db.SystemSettings.Add(new SystemSetting
        {
            Id = 1,
            QrCodeUrl = "assets/images/payment-qr.svg",
            BankName = "MB Bank",
            BankAccountNumber = "0708670042",
            BankAccountHolder = "Võ Văn Việt",
            ContactPhone = "0795443500",
            DepositPercent = 30,
            ChangeCancelHours = 24,
            HoldMinutes = 30
        });

        // 2. Users (Staff/Admin + Customers)
        var users = new List<User>
        {
            new() { FullName = "Nguyễn Văn An", Email = "admin@henry.example", Phone = "0901 234 567", Cccd = "079090000001", DateOfBirth = new DateTime(1990, 5, 15), PasswordHash = "123456", Role = "ADMIN", StaffPosition = "Quản lý", HireDate = today.AddDays(-400), IsLocked = false, CreatedAt = today.AddDays(-400) },
            new() { FullName = "Lê Thị Bích", Email = "bich.le@henry.example", Phone = "0902 345 678", Cccd = "079092000002", DateOfBirth = new DateTime(1995, 8, 20), PasswordHash = "123456", Role = "STAFF", StaffPosition = "Lễ tân", HireDate = today.AddDays(-210), IsLocked = false, CreatedAt = today.AddDays(-210) },
            new() { FullName = "Trần Quốc Cường", Email = "cuong.tran@henry.example", Phone = "0903 456 789", Cccd = "079093000003", DateOfBirth = new DateTime(1993, 11, 10), PasswordHash = "123456", Role = "STAFF", StaffPosition = "Kế toán", HireDate = today.AddDays(-150), IsLocked = false, CreatedAt = today.AddDays(-150) },
            new() { FullName = "Phạm Thu Hà", Email = "ha.pham@henry.example", Phone = "0904 567 890", Cccd = "079096000004", DateOfBirth = new DateTime(1998, 2, 25), PasswordHash = "123456", Role = "STAFF", StaffPosition = "Buồng phòng", HireDate = today.AddDays(-60), IsLocked = true, CreatedAt = today.AddDays(-60) },

            new() { FullName = "Nguyễn Quốc Huy", Email = "quochuy@example.com", Phone = "0912 345 678", Cccd = "079201001234", DateOfBirth = new DateTime(2001, 3, 12), PasswordHash = "123456", Role = "CUSTOMER", IsLocked = false, CreatedAt = today.AddDays(-120) },
            new() { FullName = "Trần Thị Mai", Email = "maitran@example.com", Phone = "0987 654 321", Cccd = "079195004567", DateOfBirth = new DateTime(1995, 7, 19), PasswordHash = "123456", Role = "CUSTOMER", IsLocked = false, CreatedAt = today.AddDays(-80) },
            new() { FullName = "Lê Hoàng Nam", Email = "namle@example.com", Phone = "0903 111 222", Cccd = "079198007890", DateOfBirth = new DateTime(1998, 10, 5), PasswordHash = "123456", Role = "CUSTOMER", IsLocked = false, CreatedAt = today.AddDays(-45) },
            new() { FullName = "Phạm Minh Anh", Email = "minhanh@example.com", Phone = "0938 222 333", Cccd = "079200003210", DateOfBirth = new DateTime(2000, 1, 22), PasswordHash = "123456", Role = "CUSTOMER", IsLocked = false, CreatedAt = today.AddDays(-20) },
            new() { FullName = "Đỗ Thùy Linh", Email = "thuylinh@example.com", Phone = "0977 888 999", Cccd = "079197006543", DateOfBirth = new DateTime(1997, 12, 30), PasswordHash = "123456", Role = "CUSTOMER", IsLocked = true, CreatedAt = today.AddDays(-5) }
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        // 3. Room Types
        var rtDouble = new RoomType { Code = "double", Name = "Phòng đôi", Description = "Phòng đôi tiêu chuẩn với tầm nhìn thoáng đãng." };
        var rtStudio = new RoomType { Code = "studio", Name = "Studio", Description = "Không gian khép kín ấm cúng có gian bếp nhỏ." };
        var rtFamily = new RoomType { Code = "family", Name = "Phòng gia đình", Description = "Không gian rộng rãi với 2 giường lớn cho gia đình." };
        db.RoomTypes.AddRange(rtDouble, rtStudio, rtFamily);
        db.SaveChanges();

        // 4. Amenities
        var amWifi = new Amenity { Code = "wifi", Name = "Wi-Fi", Icon = "wifi" };
        var amAircon = new Amenity { Code = "aircon", Name = "Điều hòa", Icon = "aircon" };
        var amBalcony = new Amenity { Code = "balcony", Name = "Ban công riêng", Icon = "balcony" };
        var amKitchen = new Amenity { Code = "kitchen", Name = "Bếp nhỏ", Icon = "kitchen" };
        db.Amenities.AddRange(amWifi, amAircon, amBalcony, amKitchen);
        db.SaveChanges();

        // 5. Rooms (6 rooms from HenryData.rooms)
        var r41 = new Room { RoomCode = "41", Name = "Vista View 41", RoomTypeId = rtDouble.Id, AreaM2 = 35, BedInfo = "1 giường Queen", ViewDescription = "Cửa sổ nhìn thành phố", Description = "Cửa sổ nhìn thành phố. 1 giường Queen, 35 m².", MaxGuests = 4, AllowPets = false, PricePerNight = 1099000, Status = "ACTIVE" };
        var r42 = new Room { RoomCode = "42", Name = "Vista View 42", RoomTypeId = rtDouble.Id, AreaM2 = 40, BedInfo = "1 giường Queen", ViewDescription = "Ban công đón nắng", Description = "Ban công đón nắng. 1 giường Queen, 40 m².", MaxGuests = 4, AllowPets = true, PricePerNight = 1190000, Status = "ACTIVE" };
        var r51 = new Room { RoomCode = "51", Name = "Vista View 51", RoomTypeId = rtDouble.Id, AreaM2 = 35, BedInfo = "1 giường Queen", ViewDescription = "Thành phố về đêm", Description = "Thành phố về đêm. 1 giường Queen, 35 m².", MaxGuests = 4, AllowPets = false, PricePerNight = 1150000, Status = "ACTIVE" };
        var r52 = new Room { RoomCode = "52", Name = "Vista View 52", RoomTypeId = rtDouble.Id, AreaM2 = 40, BedInfo = "1 giường Queen", ViewDescription = "Ban công ngắm hoàng hôn", Description = "Ban công ngắm hoàng hôn. 1 giường Queen, 40 m².", MaxGuests = 4, AllowPets = true, PricePerNight = 1299000, Status = "ACTIVE" };
        var r71 = new Room { RoomCode = "71", Name = "Studio 71", RoomTypeId = rtStudio.Id, AreaM2 = 40, BedInfo = "1 giường King", ViewDescription = "Góc nhỏ bình yên", Description = "Góc nhỏ bình yên. 1 giường King, 40 m².", MaxGuests = 2, AllowPets = true, PricePerNight = 1399000, Status = "ACTIVE" };
        var r81 = new Room { RoomCode = "81", Name = "Family Suite 81", RoomTypeId = rtFamily.Id, AreaM2 = 69, BedInfo = "2 giường Queen", ViewDescription = "Không gian cho cả nhà", Description = "Không gian cho cả nhà. 2 giường Queen, 69 m².", MaxGuests = 6, AllowPets = false, PricePerNight = 2099000, Status = "ACTIVE" };
        db.Rooms.AddRange(r41, r42, r51, r52, r71, r81);
        db.SaveChanges();

        // 6. Room Images
        db.RoomImages.AddRange(
            new RoomImage { RoomId = r41.Id, ImageUrl = "photo-02.jpg", IsPrimary = true, DisplayOrder = 1 },
            new RoomImage { RoomId = r42.Id, ImageUrl = "photo-03.jpg", IsPrimary = true, DisplayOrder = 1 },
            new RoomImage { RoomId = r51.Id, ImageUrl = "photo-04.jpg", IsPrimary = true, DisplayOrder = 1 },
            new RoomImage { RoomId = r52.Id, ImageUrl = "photo-05.jpg", IsPrimary = true, DisplayOrder = 1 },
            new RoomImage { RoomId = r71.Id, ImageUrl = "photo-08.jpg", IsPrimary = true, DisplayOrder = 1 },
            new RoomImage { RoomId = r81.Id, ImageUrl = "photo-07.jpg", IsPrimary = true, DisplayOrder = 1 }
        );

        // 7. Room Amenities
        void AddAmenities(Room room, params Amenity[] list)
        {
            foreach (var a in list)
                db.RoomAmenities.Add(new RoomAmenity { RoomId = room.Id, AmenityId = a.Id });
        }
        AddAmenities(r41, amWifi, amAircon);
        AddAmenities(r42, amWifi, amAircon, amBalcony);
        AddAmenities(r51, amWifi, amAircon);
        AddAmenities(r52, amWifi, amAircon, amBalcony);
        AddAmenities(r71, amWifi, amAircon, amKitchen);
        AddAmenities(r81, amWifi, amAircon, amBalcony, amKitchen);
        db.SaveChanges();

        // 8. Bookings, BookingRooms, RoomNights, Payments (matching admin.js)
        var c1 = users[4]; // Nguyễn Quốc Huy
        var c2 = users[5]; // Trần Thị Mai
        var c3 = users[6]; // Lê Hoàng Nam
        var c4 = users[7]; // Phạm Minh Anh
        var c5 = users[8]; // Đỗ Thùy Linh
        var adminUser = users[0];

        int txSeq = 0;
        void SeedBooking(string code, User cust, Room room, int inDays, int outDays, byte adults, byte children, byte infants, byte pets, string option, string status, int createdDaysAgo)
        {
            var checkIn = today.AddDays(inDays);
            var checkOut = today.AddDays(outDays);
            var nights = Math.Max(1, (int)(checkOut - checkIn).TotalDays);
            var total = nights * room.PricePerNight;
            var depAmt = option == "DEPOSIT" ? Math.Ceiling(total * 0.3m) : 0m;
            var reqPay = option == "DEPOSIT" ? depAmt : total;
            var paid = status == "PENDING_PAYMENT" ? 0m : reqPay;
            var refund = status == "CANCELLED" ? "WAITING_REFUND" : "NONE";

            var bk = new Booking
            {
                BookingCode = code,
                UserId = cust.Id,
                GuestName = cust.FullName,
                GuestPhone = cust.Phone,
                GuestEmail = cust.Email,
                GuestCccd = cust.Cccd,
                GuestDob = cust.DateOfBirth,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                Adults = adults,
                Children = children,
                Infants = infants,
                Pets = pets,
                TotalAmount = total,
                PaymentOption = option,
                DepositPercent = 30,
                DepositAmount = depAmt,
                PaidAmount = paid,
                Status = status,
                RefundStatus = refund,
                CancelledAt = status == "CANCELLED" ? today : null,
                CreatedAt = today.AddDays(createdDaysAgo).AddHours(9).AddMinutes(30),
                UpdatedAt = today
            };
            db.Bookings.Add(bk);
            db.SaveChanges();

            db.BookingRooms.Add(new BookingRoom
            {
                BookingId = bk.Id,
                RoomId = room.Id,
                PricePerNight = room.PricePerNight
            });

            if (status != "CANCELLED")
            {
                for (var d = checkIn; d < checkOut; d = d.AddDays(1))
                {
                    db.RoomNights.Add(new RoomNight
                    {
                        RoomId = room.Id,
                        StayDate = d,
                        BookingId = bk.Id
                    });
                }
            }

            txSeq++;
            db.Payments.Add(new Payment
            {
                PaymentCode = $"GD-{txSeq}",
                BookingId = bk.Id,
                Amount = reqPay,
                Type = option == "DEPOSIT" ? "DEPOSIT" : "FULL",
                Method = "BANK_TRANSFER",
                Status = status == "PENDING_PAYMENT" ? "PENDING" : "CONFIRMED",
                ConfirmedBy = status == "PENDING_PAYMENT" ? null : adminUser.Id,
                ConfirmedAt = status == "PENDING_PAYMENT" ? null : bk.CreatedAt.AddMinutes(45),
                CreatedAt = bk.CreatedAt
            });
            db.SaveChanges();
        }

        SeedBooking("HN-270001", c1, r42, 1, 3, 2, 0, 0, 0, "DEPOSIT", "PENDING_PAYMENT", 0);
        SeedBooking("HN-270002", c2, r41, -2, 1, 2, 1, 0, 0, "FULL", "CHECKED_IN", -6);
        SeedBooking("HN-270003", c3, r52, 5, 7, 2, 0, 1, 1, "DEPOSIT", "CONFIRMED", -3);
        SeedBooking("HN-270004", c4, r71, -8, -6, 2, 0, 0, 0, "FULL", "COMPLETED", -14);
        SeedBooking("HN-270005", c1, r81, 9, 12, 4, 2, 0, 0, "DEPOSIT", "CANCELLED", -4);
        SeedBooking("HN-270006", c5, r51, 2, 4, 2, 0, 0, 0, "FULL", "PENDING_PAYMENT", 0);
        SeedBooking("HN-270007", c2, r42, 12, 14, 2, 0, 0, 0, "FULL", "CONFIRMED", -1);

        // Save 1 favorite room for customer c1
        db.SavedRooms.Add(new SavedRoom { UserId = c1.Id, RoomId = r42.Id });
        db.SaveChanges();
    }
}
