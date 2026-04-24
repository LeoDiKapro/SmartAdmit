using AdmissionsPortal.Data;
using AdmissionsPortal.Models;
using AdmissionsPortal.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=admissions.db"));

// Identity (auth + roles) 
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password rules for development 
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Redirect unauthenticated users 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddHttpClient<ClaudeService>();

var app = builder.Build();

// Seed admin user on first run 
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userMgr = services.GetRequiredService<UserManager<AppUser>>();
    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Create roles if they don't exist
    foreach (var role in new[] { "Admin", "Student", "UniversityRep" })
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }

    // Create default admin account if it doesn't exist
    var adminEmail = "admin@admissions.com";
    if (await userMgr.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };
        await userMgr.CreateAsync(admin, "Admin123!");
        await userMgr.AddToRoleAsync(admin, "Admin");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
