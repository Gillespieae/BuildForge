using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Role + Admin seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminEmail = "admin@buildforge.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
        await userManager.CreateAsync(user, "Admin123!");
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

// Test component seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    if (!context.PcComponents.Any())
    {
        context.PcComponents.AddRange(
            new PcComponent
            {
                Name = "Ryzen 5 5600X",
                Brand = "AMD",
                ComponentType = "CPU",
                Price = 199.99m,
                SocketType = "AM4",
                Wattage = 65,
                StockQuantity = 10,
                IsActive = true
            },
            new PcComponent
            {
                Name = "Core i5-12600K",
                Brand = "Intel",
                ComponentType = "CPU",
                Price = 249.99m,
                SocketType = "LGA1700",
                Wattage = 125,
                StockQuantity = 8,
                IsActive = true
            },
            new PcComponent
            {
                Name = "RTX 3060",
                Brand = "NVIDIA",
                ComponentType = "GPU",
                Price = 329.99m,
                Wattage = 170,
                StockQuantity = 5,
                IsActive = true
            },
            new PcComponent
            {
                Name = "Corsair 16GB DDR4",
                Brand = "Corsair",
                ComponentType = "RAM",
                Price = 79.99m,
                CapacityGB = 16,
                StockQuantity = 15,
                IsActive = true
            },
            new PcComponent
            {
                Name = "ASUS B550 Motherboard",
                Brand = "ASUS",
                ComponentType = "Motherboard",
                Price = 149.99m,
                SocketType = "AM4",
                FormFactor = "ATX",
                StockQuantity = 6,
                IsActive = true
            }
        );

        context.SaveChanges();
    }
}

app.Run();
