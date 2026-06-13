using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Webbanhang.Models;
using Webbanhang.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Kết nối database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.UseCompatibilityLevel(120)
    ));

// Identity đăng nhập / đăng ký
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;

    // Tắt khóa tài khoản tự động khi nhập sai nhiều lần
    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.MaxFailedAccessAttempts = 1000;
})
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Cấu hình đường dẫn đăng nhập / đăng xuất / từ chối truy cập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Email sender chống lỗi Register
builder.Services.AddTransient<IEmailSender, Webbanhang.Helpers.NoOpEmailSender>();

// Repository
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

// Session giỏ hàng
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC + Razor Pages cho Identity UI
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed role + tài khoản mẫu
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    await SeedIdentityDataAsync(scope.ServiceProvider);
    await SeedProductDataAsync(scope.ServiceProvider);
}

app.Run();

static async Task SeedIdentityDataAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[]
    {
        SD.Role_Admin,
        SD.Role_User,
        SD.Role_Customer,
        SD.Role_Company,
        SD.Role_Employee
    })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    await CreateDemoUserAsync(
        userManager,
        "admin@htpfood.vn",
        "Admin123",
        SD.Role_Admin,
        "Quản trị viên HTP Food"
    );

    await CreateDemoUserAsync(
        userManager,
        "user@htpfood.vn",
        "User123",
        SD.Role_User,
        "Người dùng HTP Food"
    );

    await CreateDemoUserAsync(
        userManager,
        "customer@htpfood.vn",
        "Customer123",
        SD.Role_Customer,
        "Khách hàng HTP Food"
    );

    await CreateDemoUserAsync(
        userManager,
        "company@htpfood.vn",
        "Company123",
        SD.Role_Company,
        "Công ty HTP Food"
    );

    await CreateDemoUserAsync(
        userManager,
        "employee@htpfood.vn",
        "Employee123",
        SD.Role_Employee,
        "Nhân viên HTP Food"
    );
}

static async Task CreateDemoUserAsync(
    UserManager<ApplicationUser> userManager,
    string email,
    string password,
    string role,
    string fullName)
{
    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            Address = "",
            Age = ""
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description))
            );
        }
    }

    if (!await userManager.IsInRoleAsync(user, role))
    {
        await userManager.AddToRoleAsync(user, role);
    }
}
static async Task SeedProductDataAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();

    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Name = "Trái cây nhập khẩu" },
            new Category { Name = "Trái cây Việt Nam" },
            new Category { Name = "Combo trái cây" }
        );

        await context.SaveChangesAsync();
    }

    if (!context.Products.Any())
    {
        var imported = context.Categories.FirstOrDefault(c => c.Name == "Trái cây nhập khẩu");
        var vietnam = context.Categories.FirstOrDefault(c => c.Name == "Trái cây Việt Nam");
        var combo = context.Categories.FirstOrDefault(c => c.Name == "Combo trái cây");

        context.Products.AddRange(
            new Product
            {
                Name = "Táo Envy New Zealand",
                Price = 120000,
                Description = "Táo Envy giòn ngọt, nhập khẩu New Zealand.",
                ImageUrl = "/images/khay-thap-cam.jpg",
                CategoryId = imported?.Id ?? 1
            },
            new Product
            {
                Name = "Nho mẫu đơn Hàn Quốc",
                Price = 350000,
                Description = "Nho mẫu đơn cao cấp, vị ngọt thanh, mọng nước.",
                ImageUrl = "/images/khay-thap-cam.jpg",
                CategoryId = imported?.Id ?? 1
            },
            new Product
            {
                Name = "Cam sành Việt Nam",
                Price = 45000,
                Description = "Cam sành tươi, nhiều nước, giàu vitamin C.",
                ImageUrl = "/images/khay-thap-cam.jpg",
                CategoryId = vietnam?.Id ?? 2
            },
            new Product
            {
                Name = "Xoài cát Hòa Lộc",
                Price = 85000,
                Description = "Xoài cát Hòa Lộc thơm ngon, chín tự nhiên.",
                ImageUrl = "/images/khay-thap-cam.jpg",
                CategoryId = vietnam?.Id ?? 2
            },
            new Product
            {
                Name = "Combo trái cây gia đình",
                Price = 299000,
                Description = "Combo gồm nhiều loại trái cây tươi phù hợp cho gia đình.",
                ImageUrl = "/images/khay-thap-cam.jpg",
                CategoryId = combo?.Id ?? 3
            }
        );

        await context.SaveChangesAsync();
    }
}
