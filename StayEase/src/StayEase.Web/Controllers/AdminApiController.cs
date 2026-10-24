using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Models;

namespace StayEase.Web.Controllers;
[IgnoreAntiforgeryToken]
[ApiController,Route("api/admin")]
[Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme,Roles="Admin")]
public sealed class AdminApiController(StayEaseDbContext db,UserManager<ApplicationUser> users):ControllerBase
{
    public sealed record RoomInput(string Name,string Slug,string Description,string ImagePath,string Features,
        int AreaSqm,int Adults,int Children,decimal PricePerNight,int InitialUnits,bool IsActive);
    public sealed record StatusInput(BookingStatus Status);
    public sealed record RoleInput(string Role);
    public sealed record DepositSettingsInput(int DepositPercent);
    public sealed record RefundDecisionInput(string Decision);

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var from=DateTime.UtcNow.AddDays(-30);
        var bookings=await db.Bookings.AsNoTracking().ToListAsync();
        var payments=await db.Payments.AsNoTracking().Where(p=>p.Status==PaymentStatus.Succeeded
            && p.PaidAtUtc>=from).ToListAsync();
        return Ok(new {
            totalRooms=await db.RoomUnits.CountAsync(u=>u.IsActive),
            roomTypes=await db.RoomTypes.CountAsync(r=>r.IsActive),
            customers=await db.Users.CountAsync(),
            totalBookings=bookings.Count,
            pending=bookings.Count(b=>b.Status==BookingStatus.PendingPayment),
            confirmed=bookings.Count(b=>b.Status==BookingStatus.Confirmed),
            checkedIn=bookings.Count(b=>b.Status==BookingStatus.CheckedIn),
            revenue30Days=payments.Sum(p=>p.Amount),
            recent=await db.Bookings.AsNoTracking().OrderByDescending(b=>b.CreatedAtUtc)
                .Take(8).Select(b=>new {b.Reference,b.GuestName,b.TotalPrice,b.Status,b.CheckIn,b.CheckOut}).ToListAsync()
        });
    }

    [HttpGet("roomtypes")]
    public async Task<IActionResult> RoomTypes()=>Ok(await db.RoomTypes.AsNoTracking()
        .Select(r=>new {r.Id,r.Name,r.Slug,r.Description,r.ImagePath,r.Features,r.AreaSqm,r.Adults,r.Children,
            r.PricePerNight,r.IsActive,UnitCount=r.Units.Count,ActiveUnits=r.Units.Count(u=>u.IsActive)})
        .OrderBy(r=>r.Id).ToListAsync());

    [HttpPost("roomtypes")]
    public async Task<IActionResult> CreateRoom(RoomInput input)
    {
        var validation=ValidateRoom(input);
        if(validation is not null)return BadRequest(new {message=validation});
        if(await db.RoomTypes.AnyAsync(r=>r.Slug==input.Slug))return Conflict(new {message="Slug đã tồn tại."});
        var entity=new RoomType();MapRoom(entity,input);
        for(var i=0;i<input.InitialUnits;i++)entity.Units.Add(new RoomUnit {Code=$"U-{Guid.NewGuid().ToString("N")[..10]}"});
        db.RoomTypes.Add(entity);await db.SaveChangesAsync();
        return CreatedAtAction(nameof(RoomTypes),new {id=entity.Id},new {entity.Id});
    }

    [HttpPut("roomtypes/{id:int}")]
    public async Task<IActionResult> UpdateRoom(int id,RoomInput input)
    {
        var validation=ValidateRoom(input);
        if(validation is not null)return BadRequest(new {message=validation});
        var entity=await db.RoomTypes.FindAsync(id);
        if(entity is null)return NotFound();
        if(await db.RoomTypes.AnyAsync(r=>r.Slug==input.Slug && r.Id!=id))
            return Conflict(new {message="Slug đã tồn tại."});
        MapRoom(entity,input);await db.SaveChangesAsync();return NoContent();
    }

    [HttpPost("roomtypes/{id:int}/units")]
    public async Task<IActionResult> AddUnits(int id,[FromQuery]int count=1)
    {
        if(count<1||count>20)return BadRequest(new {message="Số phòng thêm phải từ 1 đến 20."});
        if(!await db.RoomTypes.AnyAsync(r=>r.Id==id))return NotFound();
        for(int i=0;i<count;i++)db.RoomUnits.Add(new RoomUnit {RoomTypeId=id,Code=$"U-{Guid.NewGuid().ToString("N")[..10]}"});
        await db.SaveChangesAsync();return Ok(new {added=count});
    }

    [HttpDelete("roomtypes/{id:int}")]
    public async Task<IActionResult> ArchiveRoom(int id)
    {
        var room=await db.RoomTypes.FindAsync(id);if(room is null)return NotFound();
        room.IsActive=false;await db.SaveChangesAsync();return NoContent();
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings(string? search=null)
    {
        var query=db.Bookings.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(search))query=query.Where(b=>b.Reference.Contains(search)||b.GuestName.Contains(search));
        return Ok(await query.OrderByDescending(b=>b.CreatedAtUtc).Take(500)
            .Select(b=>new {b.Id,b.Reference,b.GuestName,b.GuestEmail,b.GuestPhone,
                Room=b.RoomUnit.RoomType.Name,Unit=b.RoomUnit.Code,b.CheckIn,b.CheckOut,
                b.CheckInHour,b.CheckOutHour,b.TotalPrice,b.Status,b.CreatedAtUtc,b.Note}).ToListAsync());
    }

    [HttpPatch("bookings/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id,StatusInput input)
    {
        var booking=await db.Bookings.Include(b=>b.Payment).FirstOrDefaultAsync(b=>b.Id==id);
        if(booking is null)return NotFound();
        var valid=(booking.Status,input.Status) switch {
            (BookingStatus.PendingPayment,BookingStatus.Cancelled)=>true,
            (BookingStatus.AwaitingVerification,BookingStatus.Cancelled)=>true,
            (BookingStatus.Confirmed,BookingStatus.CheckedIn)=>true,
            (BookingStatus.Confirmed,BookingStatus.Cancelled)=>true,
            (BookingStatus.CheckedIn,BookingStatus.CheckedOut)=>true,
            _=>false
        };
        if(!valid)return BadRequest(new {message="Chuyển trạng thái không hợp lệ."});
        booking.Status=input.Status;
        await db.SaveChangesAsync();return NoContent();
    }

    [HttpGet("customers")]
    public async Task<IActionResult> Customers()=>Ok(await db.Users.AsNoTracking()
        .OrderByDescending(u=>u.CreatedAtUtc)
        .Select(u=>new {u.Id,u.FullName,u.Email,u.PhoneNumber,u.CreatedAtUtc,
            BookingCount=db.Bookings.Count(b=>b.UserId==u.Id)}).ToListAsync());

    [HttpGet("payments")]
    public async Task<IActionResult> Payments()=>Ok(await db.Payments.AsNoTracking()
        .OrderByDescending(p=>p.Id).Take(500)
        .Select(p=>new {p.Id,p.BookingId,p.Booking.Reference,p.Booking.GuestName,p.Amount,p.Status,p.Method,p.PaymentMode,p.PaidAtUtc})
        .ToListAsync());

    [HttpGet("reports")]
    public async Task<IActionResult> Reports(DateOnly? from,DateOnly? to)
    {
        var start=from??DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end=to??DateOnly.FromDateTime(DateTime.UtcNow);
        if(end<start)return BadRequest(new {message="Khoảng ngày không hợp lệ"});
        var result=await db.Bookings.AsNoTracking().Where(b=>b.CheckIn>=start && b.CheckIn<=end)
            .GroupBy(b=>b.CheckIn)
            .Select(g=>new {Date=g.Key,Bookings=g.Count(),Confirmed=g.Count(b=>b.Status==BookingStatus.Confirmed),
                GrossAmount=g.Sum(b=>b.TotalPrice)}).OrderBy(x=>x.Date).ToListAsync();
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users()=>Ok(await db.Users.AsNoTracking()
        .Select(u=>new {u.Id,u.FullName,u.Email,u.PhoneNumber,u.LockoutEnd,
            IsAdmin=db.UserRoles.Any(ur=>ur.UserId==u.Id && db.Roles.Any(r=>r.Id==ur.RoleId && r.Name=="Admin"))})
        .ToListAsync());

    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> UpdateRole(string id,RoleInput input)
    {
        if(input.Role!="Admin" && input.Role!="Customer")return BadRequest(new {message="Vai trò không hợp lệ"});
        var user=await users.FindByIdAsync(id);if(user is null)return NotFound();
        if(User.FindFirstValue(ClaimTypes.NameIdentifier)==id && input.Role!="Admin")
            return BadRequest(new {message="Không thể tự bỏ quyền quản trị."});
        var current=await users.GetRolesAsync(user);
        var remove=await users.RemoveFromRolesAsync(user,current);
        if(!remove.Succeeded)return BadRequest(new {message="Không thể đổi quyền."});
        var add=await users.AddToRoleAsync(user,input.Role);
        if(!add.Succeeded)return BadRequest(new {message="Không thể gán quyền."});
        return NoContent();
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var setting=await db.BusinessSettings.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==1);
        return Ok(new {depositPercent=setting?.DepositPercent??50});
    }

    [HttpPut("settings/deposit")]
    public async Task<IActionResult> UpdateDeposit(DepositSettingsInput input)
    {
        if(input.DepositPercent is < 1 or > 100)
            return BadRequest(new {message="Tỷ lệ cọc phải từ 1% đến 100%."});
        var setting=await db.BusinessSettings.FirstOrDefaultAsync(s=>s.Id==1);
        if(setting is null){setting=new BusinessSettings{Id=1};db.BusinessSettings.Add(setting);}
        setting.DepositPercent=input.DepositPercent;
        await db.SaveChangesAsync();
        // Amount on bookings created before this update is intentionally unchanged.
        return Ok(new {depositPercent=setting.DepositPercent});
    }

    [HttpPost("payments/{id:int}/confirm")]
    public async Task<IActionResult> ConfirmTransfer(int id)
    {
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var payment=await db.Payments.Include(p=>p.Booking).FirstOrDefaultAsync(p=>p.Id==id);
        if(payment is null)return NotFound();
        if(payment.Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingVerification) ||
           payment.Booking.Status is not (BookingStatus.PendingPayment or BookingStatus.AwaitingVerification) ||
           payment.Booking.HoldExpiresAtUtc<=DateTime.UtcNow)
            return BadRequest(new {message="Đơn đã hết hạn hoặc đã được xử lý; không thể xác nhận."});
        payment.Status=PaymentStatus.Succeeded;
        payment.PaidAtUtc=DateTime.UtcNow;
        payment.ConfirmedByUserId=User.FindFirstValue(ClaimTypes.NameIdentifier);
        payment.Booking.Status=BookingStatus.Confirmed;
        payment.Booking.HoldExpiresAtUtc=null;
        await db.SaveChangesAsync();await tx.CommitAsync();
        return Ok(new {message="Admin đã xác nhận tiền chuyển khoản thực nhận."});
    }

    [HttpGet("refunds")]
    public async Task<IActionResult> Refunds()=>Ok(await db.RefundRequests.AsNoTracking()
        .OrderByDescending(r=>r.RequestedAtUtc).Take(500)
        .Select(r=>new {r.Id,r.Booking.Reference,r.ContactName,r.ContactEmail,r.ContactPhone,
            r.BankName,r.BankAccountHolder,r.BankAccountNumber,r.Amount,r.Status,r.RequestedAtUtc,r.ProcessedAtUtc})
        .ToListAsync());

    [HttpPost("refunds/{id:int}/decision")]
    public async Task<IActionResult> DecideRefund(int id,RefundDecisionInput input)
    {
        if(input.Decision is not ("Refunded" or "NotRefunded"))return BadRequest(new {message="Quyết định không hợp lệ."});
        var request=await db.RefundRequests.Include(r=>r.Booking).ThenInclude(b=>b.Payment)
            .FirstOrDefaultAsync(r=>r.Id==id);
        if(request is null)return NotFound();
        if(request.Status!=RefundStatus.Pending)return BadRequest(new {message="Yêu cầu đã xử lý."});
        // Staff must actually transfer the refund at the bank before selecting 'Refunded'.
        request.Status=input.Decision=="Refunded"?RefundStatus.Refunded:RefundStatus.NotRefunded;
        request.ProcessedAtUtc=DateTime.UtcNow;
        if(request.Status==RefundStatus.Refunded && request.Booking.Payment?.Status==PaymentStatus.Succeeded)
            request.Booking.Payment.Status=PaymentStatus.Refunded;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static string? ValidateRoom(RoomInput x)
    {
        if(string.IsNullOrWhiteSpace(x.Name)||x.Name.Length>120)return "Tên phòng không hợp lệ";
        if(string.IsNullOrWhiteSpace(x.Slug)||x.Slug.Length>120||!System.Text.RegularExpressions.Regex.IsMatch(x.Slug,"^[a-z0-9-]+$"))return "Slug chỉ gồm a-z, 0-9 và dấu gạch ngang";
        if(x.PricePerNight<=0||x.AreaSqm<=0||x.Adults<1||x.Children<0||x.InitialUnits<0||x.InitialUnits>20)return "Thông số phòng không hợp lệ";
        if((x.Description?.Length??0)>2000||(x.ImagePath?.Length??0)>260||(x.Features?.Length??0)>200)return "Nội dung vượt độ dài cho phép";
        return null;
    }
    private static void MapRoom(RoomType r,RoomInput x)
    {
        r.Name=x.Name.Trim();r.Slug=x.Slug.Trim();r.Description=x.Description?.Trim()??"";
        r.ImagePath=x.ImagePath?.Trim()??"";r.Features=x.Features?.Trim()??"";
        r.AreaSqm=x.AreaSqm;r.Adults=x.Adults;r.Children=x.Children;
        r.PricePerNight=x.PricePerNight;r.IsActive=x.IsActive;
    }
}
