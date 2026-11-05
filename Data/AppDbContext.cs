using InventoryManagementSystemAPI.Models;
using InventoryManagementSystemAPI.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystemAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //DbSets
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<StockLevel> StockLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity
            builder.Entity<ApplicationUser>()
                .Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Entity<ApplicationUser>()
                .Property(u => u.LastName)
                .HasMaxLength(100);

            // Category -> Products
            builder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supplier -> Products
            builder.Entity<Supplier>()
                .HasMany(s => s.Products)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supplier -> Purchase Orders
            builder.Entity<Supplier>()
                .HasMany(s => s.PurchaseOrders)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Purchase Order -> Items 
            builder.Entity<PurchaseOrder>()
                .HasMany(p => p.PurchaseOrderItems)
                .WithOne(i => i.PurchaseOrder)
                .HasForeignKey(i => i.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> Purchase Order Items 
            builder.Entity<Product>()
                .HasMany(p => p.PurchaseOrderItems)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal Precision
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.Cost)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            builder.Entity<PurchaseOrder>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<PurchaseOrderItem>()
                .Property(i => i.Cost)
                .HasPrecision(18, 2);

            // Other Constraints 
            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.Barcode)
                .IsUnique();

            // StockLevel Constraints
            builder.Entity<StockLevel>()
                .HasOne(s => s.Product)
                .WithMany(p => p.StockLevels)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockLevel>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            builder.Entity<StockLevel>()
                .HasIndex(s => new { s.ProductId, s.LocationId }, "IX_StockLevels_ProductId_LocationId_Unique")
                .IsUnique();

            builder.Entity<StockLevel>()
                .HasIndex(s => new { s.ProductId, s.LocationId }, "IX_StockLevels_ProductId_LocationId_Lookup");
        }
    }
}
