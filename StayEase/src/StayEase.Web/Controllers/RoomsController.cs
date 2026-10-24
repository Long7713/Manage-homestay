using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Services;

namespace StayEase.Web.Controllers;

public sealed class RoomsController(StayEaseDbContext db,BookingService bookingService):Controller
{
    public async Task<IActionResult> Index(DateOnly? checkIn,DateOnly? checkOut,int adults=2,int children=0,string? q=null)
    {
        var rooms=db.RoomTypes.AsNoTracking().Where(r=>r.IsActive);
        if(!string.IsNullOrWhiteSpace(q)) rooms=rooms.Where(r=>r.Name.Contains(q)||r.Description.Contains(q));
        rooms=rooms.Where(r=>r.Adults>=adults && r.Children>=children);
        var result=await rooms.OrderBy(r=>r.PricePerNight).ToListAsync();
        if(checkIn is not null && checkOut is not null && BookingService.ValidateDates(checkIn.Value,checkOut.Value))
            ViewBag.Counts=await bookingService.AvailableCountsAsync(checkIn.Value,checkOut.Value);
        ViewBag.CheckIn=checkIn; ViewBag.CheckOut=checkOut;ViewBag.Adults=adults;ViewBag.Children=children; ViewBag.Q=q;
        return View(result);
    }
    public async Task<IActionResult> Details(int id,DateOnly? checkIn,DateOnly? checkOut)
    {
        var room=await db.RoomTypes.AsNoTracking().FirstOrDefaultAsync(r=>r.Id==id && r.IsActive);
        if(room is null)return NotFound();
        ViewBag.CheckIn=checkIn; ViewBag.CheckOut=checkOut;
        ViewBag.DepositPercent=(await db.BusinessSettings.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==1))?.DepositPercent??50;
        ViewBag.OtherRooms=await db.RoomTypes.AsNoTracking().Where(r=>r.IsActive && r.Id!=id).ToListAsync();
        return View(room);
    }
}
