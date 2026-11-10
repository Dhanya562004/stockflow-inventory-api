using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantStockLevelLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockLevels_ProductId_LocationId_Lookup",
                table: "StockLevels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockLevels_ProductId_LocationId_Lookup",
                table: "StockLevels",
                columns: new[] { "ProductId", "LocationId" });
        }
    }
}
