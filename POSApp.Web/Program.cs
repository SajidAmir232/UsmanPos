using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using POSApp.Data;
using POSApp.Data.Services;
using POSApp.Web;
using POSApp.Web.Components;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
    db.Database.Migrate();

    var existingDefaultAdmin = db.Users.FirstOrDefault(u =>
        u.Guid == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
        (u.Username == "admin" && u.Email == "admin@posapp.local"));

    if (existingDefaultAdmin != null)
    {
        existingDefaultAdmin.Username = "admin3";
        existingDefaultAdmin.Email = "admin3@posapp.local";
        existingDefaultAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin34");
        existingDefaultAdmin.UpdatedAtUtc = DateTime.UtcNow;
        existingDefaultAdmin.IsSynced = false;
        db.Users.Update(existingDefaultAdmin);
        db.SaveChanges();
    }

    MockDataService.SeedAll(db);
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
