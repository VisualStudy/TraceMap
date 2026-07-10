using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraceMap.Data;
using TraceMap.Models;
using TraceMap.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase("TraceMapDb");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. " +
                "Set ConnectionStrings__DefaultConnection in Azure Web App Configuration.");

        options.UseSqlServer(connectionString);
    }
});

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC 페이지에서 인증되지 않은 사용자가 [Authorize] 페이지에 접근하면
// 401 Unauthorized를 그대로 반환하지 않고 로그인 페이지로 이동하도록 설정합니다.
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ITracePageService, TracePageService>();
builder.Services.AddScoped<ITracePlaceService, TracePlaceService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();

builder.Services.Configure<PlacePhotoStorageOptions>(
    builder.Configuration.GetSection("PlacePhotoStorage"));

builder.Services.AddScoped<IPlacePhotoStorageService, PlacePhotoStorageService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    var seeder = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/identity")
    .MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();