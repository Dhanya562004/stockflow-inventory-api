using InventoryManagementSystemAPI.Services.Interfaces;

namespace InventoryManagementSystemAPI.Tests.Fakes
{
    public class FakeLlmClient : ILlmClient
    {
        private readonly string? _fixedResponse;
        private readonly Exception? _exceptionToThrow;

        public FakeLlmClient(string fixedResponse)
        {
            _fixedResponse = fixedResponse;
        }

        public FakeLlmClient(Exception exceptionToThrow)
        {
            _exceptionToThrow = exceptionToThrow;
        }

        public Task<string> GenerateSummaryAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            }

            return Task.FromResult(_fixedResponse ?? string.Empty);
        }
    }
}
