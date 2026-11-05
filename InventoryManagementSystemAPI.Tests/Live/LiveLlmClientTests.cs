using InventoryManagementSystemAPI.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InventoryManagementSystemAPI.Tests.Live
{
    public class LiveLlmClientTests
    {
        [Fact(Skip = "LLM_API_KEY environment variable is not configured; skipping live LLM test.")]
        [Trait("Category", "Live")]
        public async Task GenerateSummaryAsync_LiveApiCall_ReturnsActionableRecommendation()
        {
            var apiKey = Environment.GetEnvironmentVariable("LLM_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return;
            }

            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "LLM:ApiKey", apiKey },
                { "LLM:Model", "gpt-4o-mini" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            using var httpClient = new HttpClient();
            var client = new OpenAiLlmClient(httpClient, configuration);

            var prompt = "Low stock alert: Product 'Wireless Mouse' (SKU: WM-01) has 2 units remaining (Reorder Level: 15). Suggest purchase quantity.";
            var response = await client.GenerateSummaryAsync(prompt);

            Assert.False(string.IsNullOrWhiteSpace(response));
        }
    }
}
