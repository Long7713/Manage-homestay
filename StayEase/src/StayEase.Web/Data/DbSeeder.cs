using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayEase.Web.Models;

namespace StayEase.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, IHostEnvironment env)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StayEaseDbContext>();
        // Convenience for a new course project. Use proper EF migrations after the first schema change.
        await db.Database.EnsureCreatedAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { "Admin", "Customer" })
            if (!await roles.RoleExistsAsync(name)) await roles.CreateAsync(new IdentityRole(name));
        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (env.IsDevelopment() && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true,
                    FullName = config["SeedAdmin:FullName"] ?? "Quản trị viên" };
                var result = await users.CreateAsync(admin, password);
                if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ",result.Errors.Select(e=>e.Description)));
            }
            if (!await users.IsInRoleAsync(admin, "Admin")) await users.AddToRoleAsync(admin, "Admin");
        }
        if (await db.RoomTypes.AnyAsync()) return;
        var data = new[]
        {
            new RoomType { Name="The Pine Nest",Slug="the-pine-nest",Description="Phòng gỗ view rừng thông, 1 giường king lớn và ban công riêng. Thiết kế tối giản, ấm áp, dành cho những ngày thư giãn.", ImagePath="/images/pine-nest.webp",Features="Wi-Fi nhanh,Máy sưởi ấm,Máy pha trà,Nước nóng 24/7",AreaSqm=32,Adults=2,Children=1,PricePerNight=1450000m },
            new RoomType { Name="Valley Glass Suite",Slug="valley-glass-suite",Description="Suite kính panorama ngắm bình minh, phòng tắm với bồn ngâm hướng thung lũng. Trải nghiệm cao cấp giữa thiên nhiên.",ImagePath="/images/valley-suite.webp",Features="Bồn tắm view núi,Ban công 180°,Minibar miễn phí",AreaSqm=48,Adults=2,Children=1,PricePerNight=2200000m },
            new RoomType { Name="Attic Cozy Room",Slug="attic-cozy-room",Description="Áp mái ấm cúng, cửa sổ ngắm sao và lò sưởi nhỏ. Lý tưởng cho chuyến nghỉ dưỡng riêng tư.",ImagePath="/images/attic-cozy.webp",Features="Lò sưởi cổ điển,Cửa sổ trời,Loa Bluetooth",AreaSqm=28,Adults=2,Children=0,PricePerNight=1150000m },
            new RoomType { Name="Mộc Trà Villa",Slug="moc-tra-villa",Description="Biệt thự riêng giữa vườn xanh, hai phòng ngủ, bếp riêng và khu BBQ. Phù hợp gia đình hoặc nhóm bạn.",ImagePath="/images/moc-tra-villa.webp",Features="2 phòng ngủ,Bếp đầy đủ,Sân BBQ,Phòng khách riêng",AreaSqm=75,Adults=4,Children=2,PricePerNight=3200000m }
        };
        var prefix = new[] { "P", "V", "A", "M" };
        for (var i = 0; i < data.Length; i++)
        {
            for(var j=1;j<=3;j++) data[i].Units.Add(new RoomUnit { Code=$"{prefix[i]}{j:00}" });
        }
        db.RoomTypes.AddRange(data);
        await db.SaveChangesAsync();
    }
}
