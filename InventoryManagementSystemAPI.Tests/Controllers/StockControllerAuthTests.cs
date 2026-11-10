using InventoryManagementSystemAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Controllers
{
    public class StockControllerAuthTests
    {
        [Fact]
        public void AdjustStock_HasAuthorizeAttributeWithAdminOrManagerRole()
        {
            var method = typeof(StockController).GetMethod(nameof(StockController.AdjustStock));
            Assert.NotNull(method);

            var authAttribute = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authAttribute);
            Assert.Equal("Admin,Manager", authAttribute.Roles);
        }

        [Fact]
        public void AdjustWarehouseStock_HasAuthorizeAttributeWithAdminOrManagerRole()
        {
            var method = typeof(StockController).GetMethod(nameof(StockController.AdjustWarehouseStock));
            Assert.NotNull(method);

            var authAttribute = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authAttribute);
            Assert.Equal("Admin,Manager", authAttribute.Roles);
        }

        [Fact]
        public void Index_HasAllowAnonymousAttribute()
        {
            var method = typeof(StockController).GetMethod(nameof(StockController.Index));
            Assert.NotNull(method);

            var allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>();
            Assert.NotNull(allowAnonymous);
        }

        [Fact]
        public void ReportsController_HasAuthorizeAttribute()
        {
            var authAttribute = typeof(ReportsController).GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authAttribute);
        }
    }
}
