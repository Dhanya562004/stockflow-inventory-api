using InventoryManagementSystemAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InventoryManagementSystemAPI.Data.Seed
{
    public class AppDataSeeder
    {
        private readonly AppDbContext _context;

        public AppDataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync() 
        {
            await SeedCategoriesAsync();
            await SeedSuppliersAsync();
            await SeedProductsAsync();
            await SeedStockLevelsAsync();
            await SeedPurchaseOrdersAsync();
            await SeedPurchaseOrderItemsAsync();
            await UpdatePurchaseOrderTotalsAsync();
        }

        private async Task SeedCategoriesAsync() 
        {
            if (await _context.Categories.AnyAsync()) 
            {
                return;
            }

            var categories = new List<Category>
            {
                new()
                {
                    Name = "Electronics",
                    Description = "Electronic devices and accessories"
                },

                new()
                {
                    Name = "Office Supplies",
                    Description = "General office supplies"
                },

                new()
                {
                    Name = "Furniture",
                    Description = "Office furniture"
                },

                new()
                {
                    Name = "Networking",
                    Description = "Networking equipment"
                },

                new()
                {
                    Name = "Storage",
                    Description = "Storage devices"
                },

                new()
                {
                    Name = "Accessories",
                    Description = "Computer accessories"
                }
            };

            await _context.Categories.AddRangeAsync(categories);

            await _context.SaveChangesAsync();
        }

        private async Task SeedSuppliersAsync() 
        {
            if (await _context.Suppliers.AnyAsync())
                return;

            var suppliers = new List<Supplier>
            {
                new() 
                {
                    CompanyName = "Tom's Furniture",
                    ContactName = "Tom Smith",
                    Email = "tsmith@tomsfurniture.com",
                    PhoneNumber = "414-555-1234",
                    Address = "123 Furniture Street",
                    City = "Milwaukee",
                    State = "WI",
                    ZipCode = "53207"
                },

                new()
                {
                    CompanyName = "Dan's Electronics Store",
                    ContactName = "Dan Doe",
                    Email = "dd@danselectronics.com",
                    PhoneNumber = "214-555-1234",
                    Address = "4556 South Tech Way",
                    City = "Arlington",
                    State = "TX",
                    ZipCode = "22201"
                },

                new()
                {
                    CompanyName = "Office Department Store",
                    ContactName = "Fred Heath",
                    Email = "fheath@officedepartment.com",
                    PhoneNumber = "914-555-1234",
                    Address = "789 South Lexington Drive",
                    City = "Yonkers",
                    State = "NY",
                    ZipCode = "10710"
                }

            };

            await _context.Suppliers.AddRangeAsync(suppliers);

            await _context.SaveChangesAsync();
        }

        private async Task SeedProductsAsync() 
        {
            if (await _context.Products.AnyAsync())
                return;

            var categories = await _context.Categories.ToListAsync();
            var suppliers = await _context.Suppliers.ToListAsync();

            var electronics = categories.Single(c => c.Name == "Electronics");
            var officeSupplies = categories.Single(c => c.Name == "Office Supplies");
            var furniture = categories.Single(c => c.Name == "Furniture");
            var networking = categories.Single(c => c.Name == "Networking");
            var storage = categories.Single(c => c.Name == "Storage");
            var accessories = categories.Single(c => c.Name == "Accessories");

            var tomFurniture = suppliers.Single(s => s.CompanyName == "Tom's Furniture");
            var danElectronics = suppliers.Single(s => s.CompanyName == "Dan's Electronics Store");
            var officeStore = suppliers.Single(s => s.CompanyName == "Office Department Store");

            var products = new List<Product>
            {
                new()
                {
                    Name = "Dell Latitude 5550",
                    Description = "15-inch business laptop",
                    SKU = "DL-5550",
                    Barcode = "100000000001",
                    Price = 1199.99m,
                    Cost = 899.99m,
                    QuantityInStock = 15,
                    ReorderLevel = 5,
                    IsActive = true,
                    CategoryId = electronics.Id,
                    SupplierId = danElectronics.Id
                },

                new()
                {
                    Name = "Logitech MX Master 3S",
                    Description = "Wireless productivity mouse",
                    SKU = "LOG-MX3S",
                    Barcode = "100000000002",
                    Price = 99.99m,
                    Cost = 69.99m,
                    QuantityInStock = 40,
                    ReorderLevel = 10,
                    IsActive = true,
                    CategoryId = accessories.Id,
                    SupplierId = danElectronics.Id
                },

                new()
                {
                    Name = "Executive Office Chair",
                    Description = "Ergonomic leather office chair",
                    SKU = "CHR-001",
                    Barcode = "100000000003",
                    Price = 349.99m,
                    Cost = 249.99m,
                    QuantityInStock = 12,
                    ReorderLevel = 4,
                    IsActive = true,
                    CategoryId = furniture.Id,
                    SupplierId = tomFurniture.Id
                },

                new()
                {
                    Name = "Cat6 Ethernet Cable",
                    Description = "25 ft networking cable",
                    SKU = "CAT6-25",
                    Barcode = "100000000004",
                    Price = 14.99m,
                    Cost = 7.99m,
                    QuantityInStock = 100,
                    ReorderLevel = 25,
                    IsActive = true,
                    CategoryId = networking.Id,
                    SupplierId = officeStore.Id
                }
            };

            await _context.Products.AddRangeAsync(products);

            await _context.SaveChangesAsync();
        }

        private async Task SeedPurchaseOrdersAsync() 
        {
            if (await _context.PurchaseOrders.AnyAsync())
                return;

            var suppliers = await _context.Suppliers.ToListAsync();

            var tomFurniture = suppliers.Single(s => s.CompanyName == "Tom's Furniture");
            var danElectronics = suppliers.Single(s => s.CompanyName == "Dan's Electronics Store");
            var officeStore = suppliers.Single(s => s.CompanyName == "Office Department Store");

            var purchaseOrders = new List<PurchaseOrder> 
            {
                new() 
                {
                    OrderNumber = "PO-2026-0001",
                    OrderDate = DateTime.UtcNow.AddDays(-14),
                    Status = Enums.Status.COMPLETED,
                    TotalAmount = 8749.90m,
                    SupplierId = danElectronics.Id
                },

                new()
                {
                    OrderNumber = "PO-2026-0002",
                    OrderDate = DateTime.UtcNow.AddDays(-10),
                    Status = Enums.Status.APPROVED,
                    TotalAmount = 4199.88m,
                    SupplierId = tomFurniture.Id
                },

                new()
                {
                    OrderNumber = "PO-2026-0003",
                    OrderDate = DateTime.UtcNow.AddDays(-7),
                    Status = Enums.Status.PENDING,
                    TotalAmount = 1499.00m,
                    SupplierId = officeStore.Id
                },

                new()
                {
                    OrderNumber = "PO-2026-0004",
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    Status = Enums.Status.CANCELLED,
                    TotalAmount =249.95m,
                    SupplierId = danElectronics.Id
                },

                new()
                {
                    OrderNumber = "PO-2026-0005",
                    OrderDate = DateTime.UtcNow.AddDays(-2),
                    Status = Enums.Status.REJECTED,
                    TotalAmount = 999.99m,
                    SupplierId = officeStore.Id
                }
            };

            await _context.PurchaseOrders.AddRangeAsync(purchaseOrders);

            await _context.SaveChangesAsync();
        }

        private async Task SeedPurchaseOrderItemsAsync() 
        {
            if (await _context.PurchaseOrderItems.AnyAsync())
                return;

            var purchaseOrders = await _context.PurchaseOrders.ToListAsync();
            var products = await _context.Products.ToListAsync();

            var po1 = purchaseOrders.Single(po => po.OrderNumber == "PO-2026-0001");
            var po2 = purchaseOrders.Single(po => po.OrderNumber == "PO-2026-0002");
            var po3 = purchaseOrders.Single(po => po.OrderNumber == "PO-2026-0003");
            var po4 = purchaseOrders.Single(po => po.OrderNumber == "PO-2026-0004");
            var po5 = purchaseOrders.Single(po => po.OrderNumber == "PO-2026-0005");

            var laptop = products.Single(p => p.SKU == "DL-5550");
            var mouse = products.Single(p => p.SKU == "LOG-MX3S");
            var chair = products.Single(p => p.SKU == "CHR-001");
            var cable = products.Single(p => p.SKU == "CAT6-25");

            var purchaseOrderItems = new List<PurchaseOrderItem>
            {
                new()
                {
                    PurchaseOrderId = po1.Id,
                    ProductId = laptop.Id,
                    Quantity = 5,
                    Cost = laptop.Cost
                },

                new()
                {
                    PurchaseOrderId = po1.Id,
                    ProductId = mouse.Id,
                    Quantity = 10,
                    Cost = mouse.Cost
                },

                new()
                {
                    PurchaseOrderId = po2.Id,
                    ProductId = chair.Id,
                    Quantity = 8,
                    Cost = chair.Cost
                },

                new()
                {
                    PurchaseOrderId = po3.Id,
                    ProductId = cable.Id,
                    Quantity = 100,
                    Cost = cable.Cost
                },

                new()
                {
                    PurchaseOrderId = po4.Id,
                    ProductId = mouse.Id,
                    Quantity = 3,
                    Cost = mouse.Cost
                },

                new()
                {
                    PurchaseOrderId = po5.Id,
                    ProductId = laptop.Id,
                    Quantity = 2,
                    Cost = laptop.Cost
                }
            };

            await _context.PurchaseOrderItems.AddRangeAsync(purchaseOrderItems);

            await _context.SaveChangesAsync();
        }

        private async Task UpdatePurchaseOrderTotalsAsync() 
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.PurchaseOrderItems)
                .ToListAsync();

            foreach (var po in purchaseOrders) 
            {
                po.TotalAmount = po.PurchaseOrderItems.Sum(item =>
                        item.Quantity * item.Cost);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedStockLevelsAsync()
        {
            if (await _context.StockLevels.AnyAsync())
                return;

            var products = await _context.Products.ToListAsync();
            var stockLevels = new List<StockLevel>();

            foreach (var product in products)
            {
                var primaryQty = product.QuantityInStock * 2 / 3;
                var secondaryQty = product.QuantityInStock - primaryQty;

                stockLevels.Add(new StockLevel
                {
                    ProductId = product.Id,
                    LocationId = "WH-MAIN",
                    Quantity = primaryQty
                });

                if (secondaryQty > 0)
                {
                    stockLevels.Add(new StockLevel
                    {
                        ProductId = product.Id,
                        LocationId = "WH-SECONDARY",
                        Quantity = secondaryQty
                    });
                }
            }

            await _context.StockLevels.AddRangeAsync(stockLevels);
            await _context.SaveChangesAsync();
        }
    }
}
