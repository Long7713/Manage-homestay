using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Models;
using StayEase.Web.Services;
using StayEase.Web.ViewModels;

namespace StayEase.Web.Controllers;

[Authorize]
public sealed class BookingsController(StayEaseDbContext db,BookingService service,UserManager<ApplicationUser> users):Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int roomTypeId,DateOnly? checkIn,DateOnly? checkOut,int adults=2,int children=0)
    {
        var room=await db.RoomTypes.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==roomTypeId && x.IsActive);
        if(room is null)return NotFound();
        var user=await users.GetUserAsync(User);
        ViewBag.Room=room;
        ViewBag.DepositPercent=(await db.BusinessSettings.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==1))?.DepositPercent??50;
        var today=DateOnly.FromDateTime(BookingService.PropertyNow);
        return View(new BookingVm {
            RoomTypeId=roomTypeId,CheckIn=checkIn??today.AddDays(1),CheckOut=checkOut??today.AddDays(3),
            Adults=adults,Children=children,CheckInHour=14,CheckOutHour=12,
            GuestName=user?.FullName??"",GuestEmail=user?.Email??"",GuestPhone=user?.PhoneNumber??"",
            GuestCitizenId=user?.CitizenId??""
        });
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingVm vm)
    {
        var room=await db.RoomTypes.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==vm.RoomTypeId && x.IsActive);
        if(room is null)return NotFound();
        ViewBag.Room=room;
        ViewBag.DepositPercent=(await db.BusinessSettings.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==1))?.DepositPercent??50;
        if(!ModelState.IsValid)return View(vm);
        var user=await users.GetUserAsync(User);
        if(user is null)return Challenge();
        try
        {
            var (booking,error)=await service.CreateAsync(vm,user.Id);
            if(booking is not null)return RedirectToAction("Checkout","Payments",new {id=booking.Id});
            ModelState.AddModelError("",error??"Không thể đặt phòng.");
        }
        catch(DbUpdateException){ModelState.AddModelError("","Phòng vừa có khách đặt. Vui lòng thử lại.");}
        return View(vm);
    }
    [HttpGet] public async Task<IActionResult> Mine(string tab="current")
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        ViewBag.Tab=tab is "completed" or "cancelled" ? tab : "current";
        var items=await db.Bookings.AsNoTracking().Include(b=>b.RoomUnit).ThenInclude(x=>x.RoomType)
            .Include(b=>b.Payment).Include(b=>b.RefundRequest)
            .Where(b=>b.UserId==user.Id).OrderByDescending(b=>b.CreatedAtUtc).ToListAsync();
        return View(items);
    }
    [HttpGet] public async Task<IActionResult> Cancel(int id)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.AsNoTracking().FirstOrDefaultAsync(b=>b.Id==id&&b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(!BookingService.CanSelfManage(booking)){TempData["Notice"]="Chỉ được hủy trước giờ nhận phòng tối thiểu 24 giờ.";return RedirectToAction("Mine");}
        ViewBag.Booking=booking;
        return View(new CancelBookingVm {BookingId=id,ContactName=booking.GuestName,
            ContactEmail=booking.GuestEmail,ContactPhone=booking.GuestPhone});
    }
    [HttpPost,ActionName("Cancel"),ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelPost(CancelBookingVm vm)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.Include(b=>b.Payment).FirstOrDefaultAsync(b=>b.Id==vm.BookingId && b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(!BookingService.CanSelfManage(booking)){TempData["Notice"]="Đơn đã quá hạn tự hủy (24 giờ trước check-in).";return RedirectToAction("Mine");}
        if(!ModelState.IsValid){ViewBag.Booking=booking;return View("Cancel",vm);}
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if(await db.RefundRequests.AnyAsync(r=>r.BookingId==booking.Id))return BadRequest("Đơn đã có yêu cầu hủy.");
        booking.Status=BookingStatus.Cancelled;booking.CancelledAtUtc=DateTime.UtcNow;
        var hasTransfer=booking.Payment?.Status is PaymentStatus.Succeeded or PaymentStatus.AwaitingVerification;
        db.RefundRequests.Add(new RefundRequest {
            BookingId=booking.Id,ContactName=vm.ContactName.Trim(),ContactEmail=vm.ContactEmail.Trim(),
            ContactPhone=vm.ContactPhone.Trim(),BankName=vm.BankName.Trim(),
            BankAccountHolder=vm.BankAccountHolder.Trim(),BankAccountNumber=vm.BankAccountNumber.Trim(),
            Amount=hasTransfer?booking.Payment!.Amount:0,
            Status=hasTransfer?RefundStatus.Pending:RefundStatus.NotRefunded
        });
        await db.SaveChangesAsync();await tx.CommitAsync();
        TempData["Notice"]=hasTransfer?"Đã nhận yêu cầu hủy. Hoàn cọc chờ nhân viên đối soát ngân hàng.":
            "Đã hủy đơn. Chưa ghi nhận khoản cọc để hoàn.";
        return RedirectToAction("Mine",new {tab="cancelled"});
    }
    [HttpGet] public async Task<IActionResult> Reschedule(int id)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.AsNoTracking().Include(x=>x.RoomUnit).ThenInclude(x=>x.RoomType)
            .FirstOrDefaultAsync(b=>b.Id==id&&b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(!BookingService.CanSelfManage(booking)){TempData["Notice"]="Chỉ được đổi lịch trước giờ nhận phòng ít nhất 24 giờ.";return RedirectToAction("Mine");}
        ViewBag.Booking=booking;
        return View(new RescheduleVm{BookingId=id,CheckIn=booking.CheckIn,CheckOut=booking.CheckOut,
            CheckInHour=booking.CheckInHour,CheckOutHour=booking.CheckOutHour});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Reschedule(RescheduleVm vm)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.AsNoTracking().Include(x=>x.RoomUnit).ThenInclude(x=>x.RoomType)
            .FirstOrDefaultAsync(b=>b.Id==vm.BookingId && b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(!ModelState.IsValid){ViewBag.Booking=booking;return View(vm);}
        var (ok,msg)=await service.RescheduleAsync(vm.BookingId,user.Id,vm);
        if(!ok){ModelState.AddModelError("",msg);ViewBag.Booking=booking;return View(vm);}
        TempData["Notice"]=msg;return RedirectToAction("Mine");
    }
}
