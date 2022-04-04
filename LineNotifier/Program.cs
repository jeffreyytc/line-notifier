using LineNotifier.Models.Db;
using LineNotifier.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<LineNotifierDbContext>(options => options.UseInMemoryDatabase("LineNotifier"));
builder.Services.AddSingleton<LoginApiHelper, LoginApiHelper>();
builder.Services.AddSingleton<NotifyApiHelper, NotifyApiHelper>();

var app = builder.Build();

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

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
