using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Data;
using StayEase.Web.Models;
using StayEase.Web.ViewModels;

namespace StayEase.Web.Controllers;

public sealed class AccountController(UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn,
    StayEaseDbContext db):Controller
{
    [HttpGet,AllowAnonymous] public IActionResult Login(string? returnUrl=null)
    {
        ViewBag.ReturnUrl=returnUrl;return View(new LoginVm());
    }
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm,string? returnUrl=null)
    {
        ViewBag.ReturnUrl=returnUrl;
        if(!ModelState.IsValid)return View(vm);
        var user=await users.FindByEmailAsync(vm.Email.Trim());
        if(user is null){ModelState.AddModelError("","Email hoặc mật khẩu chưa đúng.");return View(vm);}
        var result=await signIn.PasswordSignInAsync(user,vm.Password,vm.RememberMe,lockoutOnFailure:true);
        if(!result.Succeeded){ModelState.AddModelError("",result.IsLockedOut?"Tài khoản tạm bị khóa. Hãy thử lại sau.":"Email hoặc mật khẩu chưa đúng.");return View(vm);}
        return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:Url.Action("Index","Home")!);
    }
    [HttpGet,AllowAnonymous] public IActionResult Register()=>View(new RegisterVm());
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if(!ModelState.IsValid)return View(vm);
        var user=new ApplicationUser {UserName=vm.Email.Trim(),Email=vm.Email.Trim(),
            FullName=vm.FullName.Trim(),PhoneNumber=vm.Phone.Trim()};
        var result=await users.CreateAsync(user,vm.Password);
        if(!result.Succeeded)
        {
            foreach(var error in result.Errors)ModelState.AddModelError("",error.Description);
            return View(vm);
        }
        await users.AddToRoleAsync(user,"Customer");
        await signIn.SignInAsync(user,isPersistent:false);
        return RedirectToAction("Profile");
    }
    [Authorize,HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();return RedirectToAction("Index","Home");
    }
    [Authorize,HttpGet] public async Task<IActionResult> Profile()
    {
        var user=await users.GetUserAsync(User);
        if(user is null)return Challenge();
        ViewBag.Email=user.Email;
        ViewBag.Bookings=await db.Bookings.AsNoTracking().Include(b=>b.RoomUnit).ThenInclude(u=>u.RoomType)
            .Where(b=>b.UserId==user.Id).OrderByDescending(b=>b.CreatedAtUtc).Take(5).ToListAsync();
        ViewBag.Favorites=await db.Favorites.CountAsync(f=>f.UserId==user.Id);
        return View(new ProfileVm {FullName=user.FullName,Email=user.Email??"",Phone=user.PhoneNumber,CitizenId=user.CitizenId,DateOfBirth=user.DateOfBirth});
    }
    [Authorize,HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVm vm)
    {
        if(!ModelState.IsValid)
        {
            var user0=await users.GetUserAsync(User);
            ViewBag.Email=user0?.Email;
            ViewBag.Bookings=new List<Booking>();
            ViewBag.Favorites=0;
            return View(vm);
        }
        var user=await users.GetUserAsync(User);
        if(user is null)return Challenge();
        var newEmail=vm.Email.Trim();
        if(!string.Equals(newEmail,user.Email,StringComparison.OrdinalIgnoreCase))
        {
            if(string.IsNullOrWhiteSpace(vm.CurrentPasswordForEmail) ||
               !await users.CheckPasswordAsync(user,vm.CurrentPasswordForEmail))
            {
                ModelState.AddModelError(nameof(vm.CurrentPasswordForEmail),"Nhập mật khẩu hiện tại để đổi email.");
                ViewBag.Email=user.Email;ViewBag.Bookings=new List<Booking>();ViewBag.Favorites=0;
                return View(vm);
            }
            var collision=await users.FindByEmailAsync(newEmail);
            if(collision is not null && collision.Id!=user.Id)
            {
                ModelState.AddModelError(nameof(vm.Email),"Email đã được sử dụng.");
                ViewBag.Email=user.Email;ViewBag.Bookings=new List<Booking>();ViewBag.Favorites=0;
                return View(vm);
            }
            var emailResult=await users.SetEmailAsync(user,newEmail);
            if(!emailResult.Succeeded) {TempData["Notice"]="Không thể đổi email.";return RedirectToAction("Profile");}
            var usernameResult=await users.SetUserNameAsync(user,newEmail);
            if(!usernameResult.Succeeded){TempData["Notice"]="Cập nhật tên đăng nhập thất bại. Liên hệ quản trị.";return RedirectToAction("Profile");}
            // EmailConfirmed is false until an email-verification service is configured.
            user.EmailConfirmed=false;
        }
        user.FullName=vm.FullName.Trim();user.PhoneNumber=vm.Phone?.Trim();
        user.CitizenId=string.IsNullOrWhiteSpace(vm.CitizenId)?null:vm.CitizenId.Trim();
        user.DateOfBirth=vm.DateOfBirth;
        var result=await users.UpdateAsync(user);
        if(result.Succeeded)await signIn.RefreshSignInAsync(user);
        TempData["Notice"]=result.Succeeded?"Đã cập nhật hồ sơ.":"Không thể lưu thông tin.";
        return RedirectToAction("Profile");
    }
}
