using System.Data;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Models;
using StayEase.Web.ViewModels;

namespace StayEase.Web.Services;

public sealed class BookingService(StayEaseDbContext db)
{
    // All guest-facing clock times are property-local (Vietnam / UTC+7).
    public static DateTime PropertyNow => DateTime.UtcNow.AddHours(7);
    public static DateTime ToLocal(DateOnly date, int hour) => date.ToDateTime(new TimeOnly(hour, 0));
    public static bool ValidateDates(DateOnly checkIn, DateOnly checkOut) =>
        ValidateDates(checkIn, 14, checkOut, 12);
    public static bool ValidateDates(DateOnly checkIn, int inHour, DateOnly checkOut, int outHour)
    {
        if(inHour is < 0 or > 23 || outHour is < 0 or > 23) return false;
        return ToLocal(checkIn, inHour) > PropertyNow &&
               checkOut > checkIn &&
               ToLocal(checkOut, outHour) > ToLocal(checkIn, inHour) &&
               checkOut.DayNumber - checkIn.DayNumber <= 30;
    }
    public static bool CanSelfManage(Booking b) =>
        (b.Status is BookingStatus.PendingPayment or BookingStatus.AwaitingVerification or BookingStatus.Confirmed) &&
        ToLocal(b.CheckIn,b.CheckInHour) >= PropertyNow.AddHours(24);

    public async Task<Dictionary<int,int>> AvailableCountsAsync(DateOnly checkIn, DateOnly checkOut)
    {
        var now=DateTime.UtcNow;
        var units=await db.RoomUnits.AsNoTracking().Where(u=>u.IsActive&&u.RoomType.IsActive)
            .Select(u=>new {u.Id,u.RoomTypeId}).ToListAsync();
        // Search uses conservative day-based display; the final hour-based check is in CreateAsync.
        var blocked=await db.Bookings.AsNoTracking().Where(b=>b.CheckIn<checkOut && b.CheckOut>checkIn &&
            (b.Status==BookingStatus.Confirmed || b.Status==BookingStatus.CheckedIn ||
             (b.Status==BookingStatus.PendingPayment && b.HoldExpiresAtUtc>now) ||
             (b.Status==BookingStatus.AwaitingVerification && b.HoldExpiresAtUtc>now)))
            .Select(b=>b.RoomUnitId).ToListAsync();
        var blockedIds=blocked.ToHashSet();
        return units.Where(u=>!blockedIds.Contains(u.Id)).GroupBy(u=>u.RoomTypeId)
            .ToDictionary(g=>g.Key,g=>g.Count());
    }

    // Used by CreateAsync and RescheduleAsync; includes shared-day turnovers.
    private IQueryable<RoomUnit> FreeUnits(int roomTypeId,DateOnly start,int startHour,DateOnly end,int endHour,DateTime utcNow,int? exceptBookingId=null)
    {
        return db.RoomUnits.Where(u=>u.RoomTypeId==roomTypeId && u.IsActive &&
            !db.Bookings.Any(b=>b.RoomUnitId==u.Id && (exceptBookingId==null || b.Id!=exceptBookingId) &&
                (b.CheckIn<end || (b.CheckIn==end && b.CheckInHour<endHour)) &&
                (b.CheckOut>start || (b.CheckOut==start && b.CheckOutHour>startHour)) &&
                (b.Status==BookingStatus.Confirmed || b.Status==BookingStatus.CheckedIn ||
                 (b.Status==BookingStatus.PendingPayment && b.HoldExpiresAtUtc>utcNow) ||
                 (b.Status==BookingStatus.AwaitingVerification && b.HoldExpiresAtUtc>utcNow))));
    }

    public async Task<(Booking? Booking,string? Error)> CreateAsync(BookingVm vm,string userId)
    {
        if(!ValidateDates(vm.CheckIn,vm.CheckInHour,vm.CheckOut,vm.CheckOutHour))
            return (null,"Thời gian lưu trú không hợp lệ. Giờ phải tròn, tối đa 30 đêm.");
        if(vm.Adults<1 || vm.Children<0)return (null,"Số khách không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var room=await db.RoomTypes.FirstOrDefaultAsync(r=>r.Id==vm.RoomTypeId && r.IsActive);
        if(room is null)return (null,"Không tìm thấy hạng phòng.");
        if(vm.Adults>room.Adults || vm.Children>room.Children)return (null,"Số khách vượt sức chứa.");
        var now=DateTime.UtcNow;
        var unit=await FreeUnits(room.Id,vm.CheckIn,vm.CheckInHour,vm.CheckOut,vm.CheckOutHour,now)
            .OrderBy(u=>u.Id).FirstOrDefaultAsync();
        if(unit is null)return (null,"Không còn phòng trống theo ngày và giờ bạn chọn.");
        var settings=await db.BusinessSettings.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==1);
        var percent=settings?.DepositPercent??50;
        var nights=vm.CheckOut.DayNumber-vm.CheckIn.DayNumber;
        var booking=new Booking {
            Reference=$"SE-{now:yyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            UserId=userId,RoomUnitId=unit.Id,CheckIn=vm.CheckIn,CheckOut=vm.CheckOut,
            CheckInHour=vm.CheckInHour,CheckOutHour=vm.CheckOutHour,
            Adults=vm.Adults,Children=vm.Children,GuestName=vm.GuestName.Trim(),
            GuestEmail=vm.GuestEmail.Trim(),GuestPhone=vm.GuestPhone.Trim(),
            GuestCitizenId=vm.GuestCitizenId.Trim(),Note=vm.Note?.Trim()??"",
            TotalPrice=room.PricePerNight*nights,DepositPercentAtBooking=percent,
            Status=BookingStatus.PendingPayment,HoldExpiresAtUtc=now.AddMinutes(15)
        };
        db.Bookings.Add(booking);
        db.Payments.Add(new Payment {
            Booking=booking,Amount=decimal.Round(booking.TotalPrice*percent/100m,0),
            Status=PaymentStatus.Pending,Method="VietQR_BankTransfer"
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (booking,null);
    }

    public async Task<(bool Ok,string Message)> RescheduleAsync(int bookingId,string userId,RescheduleVm vm)
    {
        if(!ValidateDates(vm.CheckIn,vm.CheckInHour,vm.CheckOut,vm.CheckOutHour))
            return (false,"Ngày và giờ lưu trú mới không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var booking=await db.Bookings.FirstOrDefaultAsync(b=>b.Id==bookingId && b.UserId==userId);
        if(booking is null)return (false,"Không tìm thấy đơn của bạn.");
        if(!CanSelfManage(booking))return (false,"Chỉ được đổi lịch trước giờ nhận phòng tối thiểu 24 giờ.");
        if(vm.CheckOut.DayNumber-vm.CheckIn.DayNumber != booking.CheckOut.DayNumber-booking.CheckIn.DayNumber)
            return (false,"Đổi lịch trực tuyến phải giữ nguyên số đêm và hạng phòng. Liên hệ lễ tân để đổi số đêm.");
        var typeId=await db.RoomUnits.Where(u=>u.Id==booking.RoomUnitId).Select(u=>u.RoomTypeId).SingleAsync();
        var free=await FreeUnits(typeId,vm.CheckIn,vm.CheckInHour,vm.CheckOut,vm.CheckOutHour,DateTime.UtcNow,booking.Id)
            .Where(u=>u.Id==booking.RoomUnitId).AnyAsync();
        if(!free)
        {
            var replacement=await FreeUnits(typeId,vm.CheckIn,vm.CheckInHour,vm.CheckOut,vm.CheckOutHour,DateTime.UtcNow,booking.Id)
                .OrderBy(u=>u.Id).FirstOrDefaultAsync();
            if(replacement is null)return (false,"Hạng phòng không còn chỗ ở lịch mới.");
            booking.RoomUnitId=replacement.Id;
        }
        booking.CheckIn=vm.CheckIn;booking.CheckOut=vm.CheckOut;
        booking.CheckInHour=vm.CheckInHour;booking.CheckOutHour=vm.CheckOutHour;
        await db.SaveChangesAsync();await tx.CommitAsync();
        return (true,"Đã cập nhật lịch đặt phòng. Giá và số đêm không thay đổi.");
    }

    public async Task ExpireHoldsAsync(CancellationToken token=default)
    {
        var now=DateTime.UtcNow;
        var expired=await db.Bookings.Where(b=>
            (b.Status==BookingStatus.PendingPayment || b.Status==BookingStatus.AwaitingVerification) &&
            b.HoldExpiresAtUtc<=now).ToListAsync(token);
        foreach(var item in expired)item.Status=BookingStatus.Expired;
        if(expired.Count>0)await db.SaveChangesAsync(token);
    }
}

public sealed class BookingHoldWorker(IServiceScopeFactory scopes,ILogger<BookingHoldWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));
        while(!token.IsCancellationRequested)
        {
            try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<BookingService>().ExpireHoldsAsync(token);}
            catch(OperationCanceledException) when(token.IsCancellationRequested){break;}
            catch(Exception e){logger.LogError(e,"Cannot expire booking holds");}
            try{if(!await timer.WaitForNextTickAsync(token))break;}
            catch(OperationCanceledException){break;}
        }
    }
}
