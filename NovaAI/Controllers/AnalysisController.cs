using Microsoft.AspNetCore.Mvc;
using NovaAI.Models;
using NovaAI.Services;

namespace NovaAI.Controllers
{
    public class AnalysisController : Controller
    {
        private readonly YouTubeService _youtube;
        private readonly GeminiService _gemini;

        public AnalysisController(YouTubeService youtube, GeminiService gemini)
        {
            _youtube = youtube;
            _gemini = gemini;
        }

        // ── Dashboard Sayfası ──
        public IActionResult Dashboard()
        {
            var channel = TempData["ChannelData"] as string;
            if (string.IsNullOrEmpty(channel))
                return RedirectToAction("Index", "Home");

            var analysis = System.Text.Json.JsonSerializer
                .Deserialize<ChannelAnalysis>(channel);

            return View(analysis);
        }

        // ── Kanal Analiz Et ──
        [HttpPost]
        public async Task<IActionResult> Analyze([FromForm] string channelUrl)
        {
            try
            {
                var channelId = await _youtube.ResolveChannelIdAsync(channelUrl);
                var analysis = await _youtube.GetChannelAnalysisAsync(channelId);

                TempData["ChannelData"] = System.Text.Json.JsonSerializer
                    .Serialize(analysis);

                return RedirectToAction("Dashboard");
            }
            // AnalysisController.cs içindeki 49. satır civarındaki catch bloğunu bununla değiştir:
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message + (ex.InnerException != null ? " Detay: " + ex.InnerException.Message : "");
                return RedirectToAction("Index", "Home");
            }
        }

        // ── Chat (Gemini) ──
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                ChannelAnalysis? channel = null;

                var cached = TempData.Peek("ChannelData") as string;
                if (cached != null)
                    channel = System.Text.Json.JsonSerializer
                        .Deserialize<ChannelAnalysis>(cached);

                var (reply, intent) = await _gemini.AskAsync(
                    request.Message, channel);

                return Ok(new ChatResponse
                {
                    Reply = reply,
                    Intent = intent,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ChatResponse
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}