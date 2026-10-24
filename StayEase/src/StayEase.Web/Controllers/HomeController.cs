using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
namespace StayEase.Web.Controllers;

public sealed class HomeController(StayEaseDbContext db):Controller
{
    public async Task<IActionResult> Index() => View(await db.RoomTypes.AsNoTracking().Include(x=>x.Units)
        .Where(x=>x.IsActive).OrderBy(x=>x.PricePerNight).ToListAsync());
    public IActionResult Error()=>View();
}
