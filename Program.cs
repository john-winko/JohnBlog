using JohnBlog.Data;
using JohnBlog.Models;
using JohnBlog.Services;
using JohnBlog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
builder.Services.AddScoped<IEmailService, EmailService>();

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
