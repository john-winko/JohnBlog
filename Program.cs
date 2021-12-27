using JohnBlog.Data;
using JohnBlog.Models;
using JohnBlog.Services;
using JohnBlog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Try remote environment vars
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = string.IsNullOrEmpty(databaseUrl) ? 
    builder.Configuration.GetConnectionString("DefaultConnection") : 
    DataService.BuildConnectionString(databaseUrl);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()  
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// register services
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<SlugService>();
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection(MailSettings.JSONName));
builder.Services.AddScoped<IEmailSender, EmailService>();

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(Directory.GetCurrentDirectory() + "/Log.txt")
    .MinimumLevel.Information()
    .CreateLogger();
Log.Verbose("Created logger");
Log.CloseAndFlush();

var app = builder.Build();

// Seed database if necessary although this can be accomplished through the ApplicationDBContext
await app.Services
    .CreateScope()
    .ServiceProvider
    .GetRequiredService<DataService>()
    .SeedDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

// Post.Slug routing
app.MapControllerRoute(
    "SlugRoute",
    "Posts/UrlFriendly/{slug}",
    new {controller="Posts", action="Details"});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
