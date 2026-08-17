using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using POSApp.Data;
using POSApp.Data.Services;
using POSApp.Web;
using POSApp.Web.Components;
using POSApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var supabaseSection = builder.Configuration.GetSection("Supabase");
var supabaseEnabled = supabaseSection.GetValue<bool>("Enabled", false);
if (supabaseEnabled)
{
    var supabaseConnectionString = supabaseSection.GetValue<string>("ConnectionString");
    if (!string.IsNullOrWhiteSpace(supabaseConnectionString))
    {
        Environment.SetEnvironmentVariable("SUPABASE_CONNECTION_STRING", supabaseConnectionString);
    }

    Environment.SetEnvironmentVariable("USE_SUPABASE", "true");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<LocalDbContext>();
builder.Services.AddScoped<WebAuthService>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<PurchaseInvoiceService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<CustomerPaymentService>();
builder.Services.AddScoped<SupplierPaymentService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InventoryManagementService>();
builder.Services.AddScoped<InvoiceReturnService>();
builder.Services.AddScoped<ImeiService>();
builder.Services.AddScoped<RepairJobService>();
builder.Services.AddScoped<CustomerLedgerService>();
builder.Services.AddScoped<OfflineSyncService>();
builder.Services.AddSingleton<KeyboardShortcutService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var runtimeDb = new LocalDbContext();
    var useSupabase = string.Equals(Environment.GetEnvironmentVariable("USE_SUPABASE"), "true", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING"));

    if (useSupabase)
    {
        try
        {
            runtimeDb.Database.Migrate();
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Supabase/Postgres migration failed. Falling back to local SQLite database.");
            Environment.SetEnvironmentVariable("USE_SUPABASE", "false");
            runtimeDb.Dispose();

            runtimeDb = new LocalDbContext();
            runtimeDb.Database.Migrate();
        }
    }
    else
    {
        runtimeDb.Database.Migrate();
    }

    var existingDefaultAdmin = runtimeDb.Users.FirstOrDefault(u =>
        u.Guid == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
        (u.Username == "admin" && u.Email == "admin@posapp.local"));

    if (existingDefaultAdmin != null)
    {
        existingDefaultAdmin.Username = "admin3";
        existingDefaultAdmin.Email = "admin3@posapp.local";
        existingDefaultAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin34");
        existingDefaultAdmin.UpdatedAtUtc = DateTime.UtcNow;
        existingDefaultAdmin.IsSynced = false;
        runtimeDb.Users.Update(existingDefaultAdmin);
        runtimeDb.SaveChanges();
    }

    MockDataService.SeedAll(runtimeDb);
    runtimeDb.Dispose();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
