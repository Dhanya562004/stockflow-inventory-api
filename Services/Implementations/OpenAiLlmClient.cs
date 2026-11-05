using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InventoryManagementSystemAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystemAPI.Services.Implementations
{
    public class OpenAiLlmClient : ILlmClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiLlmClient>? _logger;

        public OpenAiLlmClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiLlmClient>? logger = null)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateSummaryAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["LLM:ApiKey"]
                         ?? _configuration["OpenAI:ApiKey"]
                         ?? Environment.GetEnvironmentVariable("LLM_API_KEY")
                         ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("LLM API key is not configured.");
            }

            var model = _configuration["LLM:Model"]
                        ?? Environment.GetEnvironmentVariable("LLM_MODEL")
                        ?? "gpt-4o-mini";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are an inventory management assistant. Provide concise, actionable reorder recommendations based on low-stock reports." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 300,
                temperature = 0.3
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
            httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("LLM API call failed with status code {StatusCode}: {Response}", response.StatusCode, responseContent);
                throw new HttpRequestException($"LLM provider returned status code {response.StatusCode}: {responseContent}");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            var messageContent = choices[0].GetProperty("message").GetProperty("content").GetString();
            return messageContent?.Trim() ?? string.Empty;
        }
    }
}
