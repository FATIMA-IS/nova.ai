using System.Text;
using System.Text.Json;
using NovaAI.Models;

namespace NovaAI.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;

        // ŞİFRENİ BURAYA YAPIŞTIRACAKSIN:
        private readonly string _apiKey = "apikey";

        public GeminiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(string Reply, string Intent)> AskAsync(string message, ChannelAnalysis? channel)
        {
            // URL'nin sonundaki boşlukları zorla temizleyen ve en stabil modeli çağıran kod:
            // gemini-2.0-flash-lite → daha yüksek free tier limiti
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={_apiKey.Trim()}";

            string systemPrompt = "Sen Nova AI adında zeki, fütüristik ve yardımsever bir sosyal medya asistanısın. Kısa, net ve emojilerle desteklenmiş cevaplar verirsin. ";
            if (channel != null)
            {
                systemPrompt += $"Şu an '{channel.ChannelName}' kanalını inceliyorsun. Kanalın {channel.Subscribers} abonesi ve {channel.VideoCount} videosu var. ";
            }

            var requestBody = new
            {
                contents = new[]   
                {
                    new { parts = new[] { new { text = systemPrompt + "\nKullanıcının Sorusu: " + message } } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return ($"API Hatası: {response.StatusCode} - {responseString}", "error");
                }

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    return (text ?? "Üzgünüm, anlamlı bir cevap üretemedim.", "chat");
                }

                return ("Gemini'den boş yanıt döndü.", "error");
            }
            catch (Exception ex)
            {
                return ($"Bağlantı Hatası: {ex.Message}", "error");
            }
        }
    }
}