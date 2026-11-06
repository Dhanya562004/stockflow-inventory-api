using System.Diagnostics;
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
        private readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(15);

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
                    new { role = "system", content = "You are an inventory management assistant. Provide concise, actionable reorder recommendations based on low-stock reports adhering strictly to requested JSON schemas." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 400,
                temperature = 0.2
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            const int maxRetries = 1;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
            {
                var stopwatch = Stopwatch.StartNew();
                string outcome = "Failed";

                using var timeoutCts = new CancellationTokenSource(_requestTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                try
                {
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
                    httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(httpRequest, linkedCts.Token);
                    var responseContent = await response.Content.ReadAsStringAsync(linkedCts.Token);

                    if (!response.IsSuccessStatusCode)
                    {
                        var statusCode = (int)response.StatusCode;
                        outcome = $"HttpError_{statusCode}";
                        throw new HttpRequestException($"LLM provider returned status code {response.StatusCode}");
                    }

                    using var doc = JsonDocument.Parse(responseContent);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() == 0)
                    {
                        outcome = "EmptyChoices";
                        return string.Empty;
                    }

                    var messageContent = choices[0].GetProperty("message").GetProperty("content").GetString();
                    outcome = "Success";
                    stopwatch.Stop();
                    _logger?.LogInformation("LLM call attempt {Attempt} finished in {LatencyMs}ms with outcome: {Outcome}", attempt, stopwatch.ElapsedMilliseconds, outcome);

                    return messageContent?.Trim() ?? string.Empty;
                }
                catch (Exception ex) when (attempt <= maxRetries && !cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Stop();
                    lastException = ex;
                    _logger?.LogWarning("LLM call attempt {Attempt} failed in {LatencyMs}ms with outcome: {Outcome}. Retrying once...", attempt, stopwatch.ElapsedMilliseconds, outcome);
                }
                catch (Exception)
                {
                    stopwatch.Stop();
                    _logger?.LogError("LLM call attempt {Attempt} failed in {LatencyMs}ms with outcome: {Outcome}", attempt, stopwatch.ElapsedMilliseconds, outcome);
                    throw;
                }
            }

            throw lastException ?? new InvalidOperationException("LLM request failed after retries.");
        }
    }
}
