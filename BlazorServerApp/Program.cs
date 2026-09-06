using System.Globalization;
using BlazorServerApp.Components;
using BlazorServerApp.Data;
using BlazorServerApp.Services.Player;
using BlazorServerApp.Services.Statistics;
using BlazorServerApp.Services.Wordle;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var culture = new CultureInfo("de-CH");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<WordleDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("WordleDb")));

builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<WordListService>();
builder.Services.AddScoped<WordleGameService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WordleDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var wordListService = scope.ServiceProvider.GetRequiredService<WordListService>();
    var seeded = await wordListService.EnsureSeededAsync(app.Environment.ContentRootPath);

    if (seeded > 0)
    {
        app.Logger.LogInformation("Default word list loaded: {Count} words", seeded);
    }
}

// Muss vor UseHttpsRedirection stehen: sonst sieht die Redirect-Middleware das
// Schema "http" und erzeugt hinter dem Proxy eine Endlosschleife.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
