using Microsoft.AspNetCore.WebSockets;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var urls = builder.Configuration.GetSection("Urls").Get<string[]>();
builder.WebHost.UseUrls(urls);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Accept_Dev_Frontend", builder => builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins("http://localhost:5173")
            .AllowCredentials());
});

builder.Services.AddWebSockets(x => { });
builder.Services.AddSignalR();

var app = builder.Build();


app.UseWebSockets();
app.UseRouting();

app.UseCors("Accept_Dev_Frontend");

app.UseEndpoints(x => x.MapHub<PartyHub>("/hub").RequireCors("Accept_Dev_Frontend"));

app.Run();