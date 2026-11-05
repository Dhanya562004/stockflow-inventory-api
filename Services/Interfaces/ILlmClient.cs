namespace InventoryManagementSystemAPI.Services.Interfaces
{
    public interface ILlmClient
    {
        Task<string> GenerateSummaryAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
