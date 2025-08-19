using IUT_ACM_Solution.Data;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System;
using IUT_ACM_Solution.Services;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IEventManagement, EventManagement>();
builder.Services.AddScoped<ITeamManagement, TeamManagement>();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password Settings 
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout Settings 
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User Settings
    options.User.RequireUniqueEmail = true;

    //Signin Settings
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Error/Forbidden";
});

builder.Services.AddAuthentication()
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]??"";
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]??"";
        options.CallbackPath = "/signin-github";
        options.Scope.Add("user:email");
    });

builder.Services.Configure<SmtpSetting>(
    builder.Configuration.GetSection("SmtpSetting"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    SeedData.SeedAdminUserAsync(userManager, roleManager).GetAwaiter().GetResult();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
