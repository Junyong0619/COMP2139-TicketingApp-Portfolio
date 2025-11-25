using GBC_Ticketing.Data;
using GBC_Ticketing.Models;
using GBC_Ticketing.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logsDirectory = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "logs");
Directory.CreateDirectory(logsDirectory);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(Path.Combine(logsDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7);
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddRazorPages();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error/500");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
        name: "admin-dashboard",
        pattern: "Dashboard/{action=Index}/{id?}",
        defaults: new { area = "Admin", controller = "Home" })
    .WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Search}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

// ✅ Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    context.Database.Migrate();
    await SeedIdentityDataAsync(roleManager, userManager);

    // Seed categories if they don't exist
    if (!context.Categories.Any())
    {
        var categories = new List<Category>
        {
            new Category { Name = "Music", Description = "Concerts, festivals, and musical performances" },
            new Category { Name = "Sports", Description = "Sporting events and competitions" },
            new Category { Name = "Entertainment", Description = "Shows, comedy, and entertainment events" },
            new Category { Name = "Education", Description = "Workshops, conferences, and educational events" },
            new Category { Name = "Food & Drink", Description = "Food festivals, wine tastings, and culinary events" },
            new Category { Name = "Arts & Culture", Description = "Art exhibitions, theater, and cultural events" }
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();
    }

    // Seed events if they don't exist
    if (!context.Events.Any())
    {
        var musicCategory = context.Categories.FirstOrDefault(c => c.Name == "Music");
        var entertainmentCategory = context.Categories.FirstOrDefault(c => c.Name == "Entertainment");
        var educationCategory = context.Categories.FirstOrDefault(c => c.Name == "Education");
        var foodCategory = context.Categories.FirstOrDefault(c => c.Name == "Food & Drink");
        var sportsCategory = context.Categories.FirstOrDefault(c => c.Name == "Sports");
        var artsCategory = context.Categories.FirstOrDefault(c => c.Name == "Arts & Culture");

        var events = new List<Event>
        {
            new Event
            {
                Title = "Hope for Tomorrow – Charity Concert",
                Description = "A special evening of live music, uplifting stories, and community spirit. All proceeds support local children's education and healthcare initiatives.",
                CategoryId = musicCategory?.CategoryId ?? 1,
                StartAt = DateTime.SpecifyKind(new DateTime(2026, 1, 15, 18, 30, 0), DateTimeKind.Utc),
                Price = 25.00m,
                AvailableTickets = 200,
                Address = "Casa Loma Campus – Auditorium A",
                ImagePath = "https://images.unsplash.com/photo-1507874457470-272b3c8d8ee2"
            },
            new Event
            {
                Title = "Toronto Startup Night 2025",
                Description = "A high-energy showcase for founders, builders, and early adopters. Expect lightning pitches, live product demos, and candid fireside chats with investors.",
                CategoryId = entertainmentCategory?.CategoryId ?? 3,
                StartAt = DateTime.SpecifyKind(new DateTime(2026, 1, 28, 19, 0, 0), DateTimeKind.Utc),
                Price = 15.00m,
                AvailableTickets = 150,
                Address = "St. James Campus – Event Hall",
                ImagePath = "https://images.unsplash.com/photo-1518770660439-4636190af475"
            },
            new Event
            {
                Title = "Career Connect – Job Fair 2025",
                Description = "Meet recruiters from top employers across tech, finance, design, and public service.",
                CategoryId = educationCategory?.CategoryId ?? 4,
                StartAt = DateTime.SpecifyKind(new DateTime(2026, 2, 5, 13, 0, 0), DateTimeKind.Utc),
                Price = 0.00m,
                AvailableTickets = 500,
                Address = "Waterfront Campus – Learning Commons",
                ImagePath = "https://images.unsplash.com/photo-1504384308090-c894fdcc538d"
            },
            new Event
            {
                Title = "Morning Boost – Free Breakfast Meetup",
                Description = "Start the day with fresh coffee, pastries, and good people.",
                CategoryId = foodCategory?.CategoryId ?? 5,
                StartAt = DateTime.SpecifyKind(new DateTime(2026, 2, 10, 8, 30, 0), DateTimeKind.Utc),
                Price = 0.00m,
                AvailableTickets = 120,
                Address = "Casa Loma Campus – Cafeteria",
                ImagePath = "https://images.unsplash.com/photo-1504754524776-8f4f37790ca0"
            },
            new Event
            {
                Title = "Community Soccer Tournament 2025",
                Description = "A friendly yet competitive soccer tournament that brings together students, alumni, and local players.",
                CategoryId = sportsCategory?.CategoryId ?? 2,
                StartAt = DateTime.SpecifyKind(new DateTime(2026, 2, 20, 10, 0, 0), DateTimeKind.Utc),
                Price = 5.00m,
                AvailableTickets = 300,
                Address = "St. James Campus – Outdoor Field",
                ImagePath = "https://images.unsplash.com/photo-1508609349937-5ec4ae374ebf"
            },
            new Event
            {
                Title = "Autumn Arts Festival – Toronto Edition",
                Description = "Celebrate creativity at this vibrant arts and culture festival featuring student exhibitions, live painting, and music performances.",
                CategoryId = artsCategory?.CategoryId ?? 6,
                StartAt = DateTime.SpecifyKind(new DateTime(2026, 3, 5, 11, 0, 0), DateTimeKind.Utc),
                Price = 10.00m,
                AvailableTickets = 400,
                Address = "Waterfront Campus – Atrium",
                ImagePath = "https://images.unsplash.com/photo-1497032628192-86f99bcd76bc"
            }
        };

        context.Events.AddRange(events);
        context.SaveChanges();
    }
}

app.Run();

async Task SeedIdentityDataAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    var roles = new[] { "Admin", "Organizer", "Attendee" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var defaultUsers = new List<(string Email, string FullName, string Role, string Phone, DateTime DateOfBirth)>
    {
        ("admin@gbc-ticketing.com", "Admin User", "Admin", "111-111-1111", DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc)),
        ("organizer@gbc-ticketing.com", "Organizer User", "Organizer", "222-222-2222", DateTime.SpecifyKind(new DateTime(1992, 2, 2), DateTimeKind.Utc)),
        ("attendee@gbc-ticketing.com", "Attendee User", "Attendee", "333-333-3333", DateTime.SpecifyKind(new DateTime(1994, 3, 3), DateTimeKind.Utc))
    };

    const string defaultPassword = "Password123!";

    foreach (var (email, fullName, role, phone, dob) in defaultUsers)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            existingUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                PhoneNumber = phone,
                DateOfBirth = dob,
                ProfilePictureUrl = null
            };

            var createResult = await userManager.CreateAsync(existingUser, defaultPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(existingUser, role))
        {
            await userManager.AddToRoleAsync(existingUser, role);
        }
    }
}
