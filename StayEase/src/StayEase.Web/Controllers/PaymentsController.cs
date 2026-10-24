using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Models;

namespace StayEase.Web.Controllers;

[Authorize]
public sealed class PaymentsController(StayEaseDbContext db,UserManager<ApplicationUser> users,IConfiguration cfg):Controller
{
    [HttpGet] public async Task<IActionResult> Checkout(int id)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.AsNoTracking().Include(b=>b.RoomUnit).ThenInclude(r=>r.RoomType)
            .Include(b=>b.Payment).FirstOrDefaultAsync(b=>b.Id==id && b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(booking.Status is not (BookingStatus.PendingPayment or BookingStatus.AwaitingVerification)
           || booking.HoldExpiresAtUtc<=DateTime.UtcNow)return RedirectToAction("Result",new{id});
        var bank=cfg["VietQr:BankId"];
        var account=cfg["VietQr:AccountNumber"];
        var owner=cfg["VietQr:AccountName"];
        ViewBag.Bank=bank;
        ViewBag.Account=account;
        ViewBag.AccountName=owner;
        if(!string.IsNullOrWhiteSpace(bank)&&!string.IsNullOrWhiteSpace(account)&&!string.IsNullOrWhiteSpace(owner))
        {
            var amount=Convert.ToInt64(booking.Payment?.Amount??0);
            ViewBag.QrUrl=$"https://img.vietqr.io/image/{Uri.EscapeDataString(bank)}-{Uri.EscapeDataString(account)}-compact2.png?amount={amount}&addInfo={Uri.EscapeDataString(booking.Reference)}&accountName={Uri.EscapeDataString(owner)}";
        }
        return View(booking);
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectAmount(int id,string mode="Deposit")
    {
        if(mode is not ("Deposit" or "Full"))return BadRequest();
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.Include(b=>b.Payment).FirstOrDefaultAsync(b=>b.Id==id&&b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(booking.Status!=BookingStatus.PendingPayment || booking.HoldExpiresAtUtc<=DateTime.UtcNow || booking.Payment is null)
            return RedirectToAction("Result",new{id});
        booking.Payment.PaymentMode=mode;
        booking.Payment.Amount=mode=="Full"?booking.TotalPrice:
            decimal.Round(booking.TotalPrice*booking.DepositPercentAtBooking/100m,0);
        await db.SaveChangesAsync();
        return RedirectToAction("Checkout",new{id});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> ReportTransfer(int id)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.Include(b=>b.Payment).FirstOrDefaultAsync(b=>b.Id==id&&b.UserId==user.Id);
        if(booking is null)return NotFound();
        if(string.IsNullOrWhiteSpace(cfg["VietQr:BankId"]) || string.IsNullOrWhiteSpace(cfg["VietQr:AccountNumber"]) || string.IsNullOrWhiteSpace(cfg["VietQr:AccountName"]))
            return BadRequest("Homestay chưa cấu hình tài khoản VietQR.");
        if(booking.Status==BookingStatus.PendingPayment && booking.HoldExpiresAtUtc>DateTime.UtcNow && booking.Payment is not null)
        {
            booking.Status=BookingStatus.AwaitingVerification;
            booking.Payment.Status=PaymentStatus.AwaitingVerification;
            booking.Payment.CustomerReportedAtUtc=DateTime.UtcNow;
            // Extend the hold for staff to reconcile; expires after 24 hours if not verified.
            booking.HoldExpiresAtUtc=DateTime.UtcNow.AddHours(24);
            await db.SaveChangesAsync();
            TempData["Notice"]="Đã ghi nhận thông báo chuyển khoản; chưa xác nhận đã nhận tiền. Admin sẽ đối soát.";
        }
        return RedirectToAction("Result",new{id});
    }
    [HttpGet] public async Task<IActionResult> Result(int id)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var booking=await db.Bookings.AsNoTracking().Include(b=>b.RoomUnit).ThenInclude(r=>r.RoomType)
            .Include(b=>b.Payment).FirstOrDefaultAsync(b=>b.Id==id&&b.UserId==user.Id);
        return booking is null?NotFound():View(booking);
    }
}
