using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DAOs;
using OverLut.Models.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<OverLutContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatConnection")));

builder.Services.AddDbContext<OverLutStorageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StorageConnection")));
builder.Services.AddScoped<OverLutContext>();


// DAO Scoped

builder.Services.AddScoped<AttachmentDAO>();
builder.Services.AddScoped<ChannelDAO>();
builder.Services.AddScoped<ChannelMemberDAO>();
builder.Services.AddScoped<MessageDAO>();
builder.Services.AddScoped<ReadReceiptDAO>();
builder.Services.AddScoped<UserDAO>();

//Repository Scoped
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opption =>
    {
        opption.LoginPath = "/Login";
    }
    );

var app = builder.Build();
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await ChatWebSocketHandler.Handle(context, webSocket, app.Services);
        }
        else { context.Response.StatusCode = StatusCodes.Status400BadRequest; }
    }
    else { await next(); }
});

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
