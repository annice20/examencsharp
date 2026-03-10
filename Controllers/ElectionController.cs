using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers
{
    public class ElectionController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ElectionController(ApplicationDbContext db) => _db = db;

        private bool EstConnecte() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("user"));

        public async Task<IActionResult> Index()
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            return View(await _db.Elections.Include(e => e.Fokontany).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            ViewBag.Fokontanys = await _db.Fokontany.ToListAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Election model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Fokontanys = await _db.Fokontany.ToListAsync();
                return View(model);
            }
            _db.Elections.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Élection créée.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            var election = await _db.Elections.FindAsync(id);
            if (election != null)
            {
                election.IsActive = !election.IsActive;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Resultats(int id)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            var election = await _db.Elections.Include(e => e.Fokontany)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (election == null) return NotFound();

            var candidats = await _db.Candidats.Where(c => c.ElectionId == id).ToListAsync();
            var resultats = await _db.Votes
                .Where(v => v.ElectionId == id)
                .GroupBy(v => v.CandidatId)
                .Select(g => new { CandidatId = g.Key, Total = g.Count() })
                .ToListAsync();

            ViewBag.Election = election;
            ViewBag.Resultats = resultats;
            return View(candidats);
        }
    }
}