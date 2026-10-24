using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StayEase.Web.Data;
using StayEase.Web.Models;
using StayEase.Web.Services;

var builder=WebApplication.CreateBuilder(args);
var connection=builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection");
var jwtKey=builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Thiếu Jwt:Key (biến môi trường hoặc user secrets)");
if(Encoding.UTF8.GetByteCount(jwtKey)<32) throw new InvalidOperationException("Jwt:Key phải tối thiểu 32 bytes");

builder.Services.AddDbContext<StayEaseDbContext>(o=>o.UseSqlServer(connection));
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(o=>
{
    o.Password.RequiredLength=8;
    o.Password.RequireDigit=true;
    o.Password.RequireUppercase=true;
    o.Password.RequireLowercase=true;
    o.Password.RequireNonAlphanumeric=true;
    o.User.RequireUniqueEmail=true;
    o.Lockout.MaxFailedAccessAttempts=5;
    o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(10);
}).AddEntityFrameworkStores<StayEaseDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o=>
{
    o.LoginPath="/Account/Login";
    o.AccessDeniedPath="/Account/Login";
    o.Cookie.HttpOnly=true;
    o.Cookie.SameSite=SameSiteMode.Lax;
    o.SlidingExpiration=true;
});
builder.Services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,o=>
{
    o.TokenValidationParameters=new TokenValidationParameters
    {
        ValidateIssuer=true,ValidIssuer=builder.Configuration["Jwt:Issuer"],
        ValidateAudience=true,ValidAudience=builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddControllersWithViews(o=>o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()))
    .AddJsonOptions(o=>o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<BookingService>();
builder.Services.AddHostedService<BookingHoldWorker>();
var app=builder.Build();
await DbSeeder.SeedAsync(app.Services,app.Configuration,app.Environment);
if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();
