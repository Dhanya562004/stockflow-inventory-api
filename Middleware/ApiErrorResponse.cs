namespace InventoryManagementSystemAPI.Middleware
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; init; }

        public string Message { get; init; } = string.Empty;

        public IDictionary<string, string[]>? Errors { get; init; }
    }
}
