using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Models;
namespace StayEase.Web.Controllers;

[Authorize]
public sealed class FavoritesController(StayEaseDbContext db,UserManager<ApplicationUser> users):Controller
{
    [HttpGet] public async Task<IActionResult> Index()
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        return View(await db.Favorites.AsNoTracking().Include(x=>x.RoomType)
            .Where(x=>x.UserId==user.Id).ToListAsync());
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int roomTypeId)
    {
        var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        if(!await db.RoomTypes.AnyAsync(r=>r.Id==roomTypeId && r.IsActive))return NotFound();
        var fav=await db.Favorites.FirstOrDefaultAsync(x=>x.UserId==user.Id && x.RoomTypeId==roomTypeId);
        if(fav is not null)db.Favorites.Remove(fav);
        else db.Favorites.Add(new Favorite {UserId=user.Id,RoomTypeId=roomTypeId});
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
