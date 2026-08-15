using System.Net;
using System.Net.Http.Json;
using Health.Models;

namespace Health.Services
{
    public class GeminiService
    {
        private const string ApiKey = ApiConstants.ApiKey;

        private const string Endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={ApiKey}";

        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }


        public async Task<string> GetResponseAsync(string userText)
        {
            var requestBody = new GeminiRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts = new List<Part> { new Part { Text = userText } }
                    }
                },
                SafetySettings = new List<SafetySetting>
                {
                    new SafetySetting { Category = "HARM_CATEGORY_HARASSMENT", Threshold = "BLOCK_NONE" },
                    new SafetySetting { Category = "HARM_CATEGORY_HATE_SPEECH", Threshold = "BLOCK_NONE" },
                }
            };

            int maxRetries = 3;  
            int delay = 1000;      

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(Endpoint, requestBody);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                        var candidate = result?.Candidates?.FirstOrDefault();
                        if (candidate?.FinishReason == "SAFETY")
                        {
                            return "Ответ заблокирован системой безопасности Google.";
                        }

                        return candidate?.Content?.Parts?.FirstOrDefault()?.Text
                               ?? "Модель вернула пустой ответ.";
                    }

                  
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == (HttpStatusCode)429)
                    {
                        if (i == maxRetries - 1)
                            return $"Сервер перегружен (503). Попробуйте позже.";

                        await Task.Delay(delay * (i + 1));
                        continue;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Ошибка API {(int)response.StatusCode}: {errorContent}";
                }
                catch (TaskCanceledException)
                {
                    return "Превышено время ожидания запроса.";
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1) return $"Сбой соединения: {ex.Message}";
                    await Task.Delay(delay);
                }
            }

            return "Неизвестная ошибка.";
        }
    }
}
