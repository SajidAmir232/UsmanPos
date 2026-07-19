using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class PurchaseInvoiceService
{
    private readonly ProductService _productService = new();

    public List<PurchaseInvoice> GetAll()
    {
        using var db = new LocalDbContext();
        return db.PurchaseInvoices.Include(pi => pi.Items)
                                   .Where(pi => !pi.IsDeleted)
                                   .OrderByDescending(pi => pi.PurchaseDate)
                                   .ToList();
    }

    public PurchaseInvoice? GetByGuid(Guid guid)
    {
        using var db = new LocalDbContext();
        return db.PurchaseInvoices.Include(pi => pi.Items)
                                  .FirstOrDefault(pi => pi.Guid == guid && !pi.IsDeleted);
    }

    public PurchaseInvoice Create(PurchaseInvoice purchaseInvoice, string deviceId)
    {
        using var db = new LocalDbContext();
        using var tx = db.Database.BeginTransaction();

        purchaseInvoice.PurchaseNumber = GeneratePurchaseNumber(db);
        purchaseInvoice.UpdatedAtUtc = DateTime.UtcNow;
        purchaseInvoice.IsSynced = false;

        db.PurchaseInvoices.Add(purchaseInvoice);
        db.SaveChanges();

        foreach (var item in purchaseInvoice.Items)
        {
            item.Guid = item.Guid == Guid.Empty ? Guid.NewGuid() : item.Guid;
            item.IsSynced = false;
            item.UpdatedAtUtc = DateTime.UtcNow;

            var product = db.Products.FirstOrDefault(p => p.Guid == item.ProductGuid);
            if (product != null)
            {
                product.Quantity += (int)item.Quantity;
                product.IsSynced = false;
                product.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        db.SaveChanges();
        tx.Commit();
        return purchaseInvoice;
    }

    public PurchaseInvoice Update(PurchaseInvoice purchaseInvoice)
    {
        using var db = new LocalDbContext();
        purchaseInvoice.UpdatedAtUtc = DateTime.UtcNow;
        purchaseInvoice.IsSynced = false;

        db.PurchaseInvoices.Update(purchaseInvoice);
        db.SaveChanges();
        return purchaseInvoice;
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var purchaseInvoice = db.PurchaseInvoices.Find(id);
        if (purchaseInvoice == null) return;

        purchaseInvoice.IsDeleted = true;
        purchaseInvoice.IsSynced = false;
        purchaseInvoice.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    private string GeneratePurchaseNumber(LocalDbContext db)
    {
        var count = db.PurchaseInvoices.Count() + 1;
        return $"PUR-{DateTime.Now:yyyyMMdd}-{count:D4}";
    }
}
