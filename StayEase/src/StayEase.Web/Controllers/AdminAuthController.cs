using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StayEase.Web.Models;

namespace StayEase.Web.Controllers;
[IgnoreAntiforgeryToken]
[ApiController,Route("api/admin/auth")]
public sealed class AdminAuthController(UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn,
    IConfiguration configuration):ControllerBase
{
    public sealed record LoginRequest(string Email,string Password);
    [HttpPost("login"),AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest payload)
    {
        if(string.IsNullOrWhiteSpace(payload.Email)||string.IsNullOrWhiteSpace(payload.Password))return Unauthorized();
        var user=await users.FindByEmailAsync(payload.Email.Trim());
        if(user is null)return Unauthorized(new {message="Thông tin đăng nhập không đúng."});
        var check=await signIn.CheckPasswordSignInAsync(user,payload.Password,lockoutOnFailure:true);
        if(!check.Succeeded || !await users.IsInRoleAsync(user,"Admin"))
            return Unauthorized(new {message="Sai mật khẩu hoặc không có quyền quản trị."});
        var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var claims=new[] {new Claim(JwtRegisteredClaimNames.Sub,user.Id),new Claim(ClaimTypes.NameIdentifier,user.Id),
            new Claim(ClaimTypes.Name,user.FullName),new Claim(ClaimTypes.Role,"Admin")};
        var expiration=DateTime.UtcNow.AddHours(8);
        var jwt=new JwtSecurityToken(issuer:configuration["Jwt:Issuer"],audience:configuration["Jwt:Audience"],
            claims:claims,notBefore:DateTime.UtcNow,expires:expiration,
            signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));
        return Ok(new {token=new JwtSecurityTokenHandler().WriteToken(jwt),expiresAtUtc=expiration,
            name=user.FullName});
    }
}
