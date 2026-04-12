using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers
{
    public class CinRequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CinRequestController(ApplicationDbContext db) => _db = db;

        private bool EstAdmin() =>
            HttpContext.Session.GetString("role") == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!EstAdmin()) return RedirectToAction("Login", "Account");

            var requests = await _db.CinRequests
                .Include(r => r.Citoyen)
                .OrderBy(r => r.Status)
                .ThenByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> Approuver(int id)
        {
            if (!EstAdmin()) return RedirectToAction("Login", "Account");

            var request = await _db.CinRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = "Approuvé";
                await _db.SaveChangesAsync();
                TempData["Success"] = "CIN approuvée avec succès !";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Refuser(int id)
        {
            if (!EstAdmin()) return RedirectToAction("Login", "Account");

            var request = await _db.CinRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = "Refusé";
                await _db.SaveChangesAsync();
                TempData["Error"] = "CIN refusée.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}