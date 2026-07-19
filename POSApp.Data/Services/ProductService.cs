using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public class ProductService
{
    public List<Product> GetAll()
    {
        using var db = new LocalDbContext();
        return db.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
    }

    public Product? GetByGuid(Guid guid)
    {
        using var db = new LocalDbContext();
        return db.Products.FirstOrDefault(p => p.Guid == guid && !p.IsDeleted);
    }

    public Product Save(Product product)
    {
        using var db = new LocalDbContext();
        product.UpdatedAtUtc = DateTime.UtcNow;
        product.IsSynced = false; // mark dirty so SyncService pushes it next time

        if (product.Id == 0)
        {
            // Auto-generate SKU for new products if not already set
            if (string.IsNullOrWhiteSpace(product.Sku))
            {
                product.Sku = GenerateSku(db);
            }
            db.Products.Add(product);
        }
        else
        {
            db.Products.Update(product);
        }

        db.SaveChanges();
        return product;
    }

    private string GenerateSku(LocalDbContext db)
    {
        // Generate SKU pattern: PROD-YYYYMMDD-0001
        // This ensures uniqueness even across offline devices
        var datePrefix = DateTime.Now.ToString("yyyyMMdd");
        var existingCount = db.Products
            .Where(p => p.Sku != null && p.Sku.StartsWith("PROD-" + datePrefix))
            .Count();

        var sequenceNumber = existingCount + 1;
        return $"PROD-{datePrefix}-{sequenceNumber:D4}";
    }

    public void Delete(int id)
    {
        using var db = new LocalDbContext();
        var product = db.Products.Find(id);
        if (product == null) return;

        // Soft delete -> so the deletion also gets synced instead of just disappearing locally
        product.IsDeleted = true;
        product.IsSynced = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }

    public void AdjustStock(Guid productGuid, decimal quantityChange)
    {
        using var db = new LocalDbContext();
        var product = db.Products.FirstOrDefault(p => p.Guid == productGuid);
        if (product == null) return;

        product.Quantity += (int)quantityChange;
        product.IsSynced = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        db.SaveChanges();
    }
}
