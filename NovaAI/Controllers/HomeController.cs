using Microsoft.AspNetCore.Mvc;

namespace NovaAI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}