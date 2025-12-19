using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using NutrientOptimizer.Core.Data;
using NutrientOptimizer.Web.Components;
using NutrientOptimizer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Add DbContext
builder.Services.AddDbContext<NutrientDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<DatabaseInitializationService>();
builder.Services.AddScoped<SaltDatabaseService>();
builder.Services.AddScoped<SelectedSubstancesService>();

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();
    try
    {
        dbInitService.InitializeDatabaseAsync().GetAwaiter().GetResult();
        Console.WriteLine("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR during database initialization: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
