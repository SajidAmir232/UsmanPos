using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services
{
    public class InvoiceService
    {
        private readonly ProductService _productService = new();

        public List<Invoice> GetAll()
        {
            using var db = new LocalDbContext();
            return db.Invoices.Include(i => i.Items)
                               .Where(i => !i.IsDeleted)
                               .OrderByDescending(i => i.InvoiceDate)
                               .ToList();
        }

        // Creates a full invoice + items in one transaction and reduces stock for each item.
        public Invoice Create(Invoice invoice, string deviceId)
        {
            using var db = new LocalDbContext();
            using var tx = db.Database.BeginTransaction();

            invoice.InvoiceNumber = GenerateInvoiceNumber(db);
            invoice.CreatedByDeviceId = deviceId;
            invoice.UpdatedAtUtc = DateTime.UtcNow;
            invoice.IsSynced = false;

            db.Invoices.Add(invoice);

            foreach (var item in invoice.Items)
            {
                item.InvoiceGuid = invoice.Guid;
                db.InvoiceItems.Add(item);

                var product = db.Products.FirstOrDefault(p => p.Guid == item.ProductGuid);
                if (product != null)
                {
                    product.Quantity -= (int)item.Quantity;
                    product.IsSynced = false;
                    product.UpdatedAtUtc = DateTime.UtcNow;
                    db.Products.Update(product);
                }
            }

            db.SaveChanges();
            tx.Commit();
            return invoice;
        }

        private string GenerateInvoiceNumber(LocalDbContext db)
        {
            // Prefix with device-friendly date so two offline devices rarely clash,
            // final de-duplication safety net is the Guid anyway.
            var count = db.Invoices.Count() + 1;
            return $"INV-{DateTime.Now:yyyyMMdd}-{count:D4}";
        }
    }
}
