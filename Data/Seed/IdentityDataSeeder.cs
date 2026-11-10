using InventoryManagementSystemAPI.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystemAPI.Data.Seed
{
    public class IdentityDataSeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public IdentityDataSeeder(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            string[] roles = { "Admin", "User", "Manager" };

            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                var result = await _roleManager.CreateAsync(
                    new ApplicationRole { Name = role });

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': {errors}");
                }
            }
        }
    }
}