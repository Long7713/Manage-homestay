using HenryHomestay.BLL;
using HenryHomestay.DAL;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Initialize and seed shared database (SQL Server / Shared Local DB)
DbInitializer.Initialize();

var keysDir = new DirectoryInfo(Path.Combine(DatabaseConfigManager.GetSolutionRoot(), ".keys"));
keysDir.Create();
builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDir);

// Dependency Injection for Business Logic Services (BLL)
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SystemService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
