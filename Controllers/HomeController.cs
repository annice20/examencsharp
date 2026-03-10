using Microsoft.AspNetCore.Mvc;

namespace examencsharp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Vérifier si connecté via Session
            var userEmail = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}