using InventoryManagementSystemAPI.Models.Identity;

namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string> roles);
    }
}
