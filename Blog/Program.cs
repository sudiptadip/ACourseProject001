using Blog.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Blog.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Blog.DataAccess.Repository.IRepository;
using Blog.DataAccess.Repository;
using Blog.Utility.Service.IService;
using Blog.Utility.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging().LogTo(Console.WriteLine));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IUniteOfWork, UniteOfWork>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<ViewRenderingService>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
   // app.UseExceptionHandler("/Home/Error");
    app.UseExceptionHandler("Customer/Error/PageNotFound");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Customer/Error/PageNotFound");

// Handle 404 errors
app.UseStatusCodePagesWithReExecute("/Customer/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
