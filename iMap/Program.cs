using iMap.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using iMap.ViewModels;
using System.Configuration;
using GoogleMaps.AspNetCore;
using iMap.Hubs;
using Microsoft.AspNetCore.SignalR;
using iMap;
using iMap.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using iMap.Helpers;
using AutoMapper;
using iMap.Extensions;
using iMap.Helper;
using iMap.Mappings;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore).AddRazorRuntimeCompilation();
builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));


//builder.Services.AddAuthentication().AddFacebook(options =>
//    {
//        options.AppId = builder.Configuration["App:FacebookAppId"];
//        options.AppSecret = builder.Configuration["App:FacebookAppSecret"];
//    });

//builder.Services.AddAuthentication().AddGoogle(options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//});





builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abc�defg�hi�jklmn�opqrs�t�uvwxyzABC�DEF�GHI�JKLMNO�PQRS�TU�VWXYZ0123456789-._@+/ ";
})
        .AddEntityFrameworkStores<AppIdentityDbContext>()
        .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//builder.Services.AddAutoMapper(typeof(ApplicationBuilder));
builder.Services.AddAutoMapper(cfg =>
{
}, typeof(UserProfile));
builder.Services.AddTransient<IFileValidator, FileValidator>();
builder.Services.AddTransient<IUploadHelper, UploadHelper>();
builder.Services.AddRazorPages();
builder.Services.AddSignalR(cfg => cfg.EnableDetailedErrors = true);



builder.Services.AddCors(options =>
{
    options.AddPolicy(
         "AllowOrigin",
         builder =>
builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});






CookieBuilder cookieBuilder = new CookieBuilder();
cookieBuilder.Name = "User";
cookieBuilder.HttpOnly = true;
cookieBuilder.SameSite = SameSiteMode.Lax;
cookieBuilder.SecurePolicy = CookieSecurePolicy.None;

builder.Services.ConfigureApplicationCookie(Options =>
{
    Options.LoginPath = new PathString("/Home/Login");
    Options.LogoutPath = new PathString("/Member/LogOut");
    Options.Cookie = cookieBuilder;
    Options.SlidingExpiration = true;
    Options.ExpireTimeSpan = TimeSpan.FromDays(14);

});






var app = builder.Build();
app.MigrateDatabase();

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

    app.UseDeveloperExceptionPage();


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}"
    );


app.UseCors(x =>
{
    x.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});



app.Run();
