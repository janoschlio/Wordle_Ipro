using System.Globalization;
using BlazorServerApp.Components;
using BlazorServerApp.Data;
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

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Database Context
builder.Services.AddDbContext<WordleDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("WordleDb")));

// Register Services
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<WordListService>();
builder.Services.AddScoped<WordleGameService>();

var app = builder.Build();

// Ensure database is created and the default word list is loaded
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

// Configure the HTTP request pipeline.
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
