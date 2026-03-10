using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers
{
    public class CitoyenController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CitoyenController(ApplicationDbContext db) => _db = db;

        private bool EstConnecte() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("user"));

        public async Task<IActionResult> Index(string search)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");

            var query = _db.Citoyens.Include(c => c.Fokontany).AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(c =>
                    c.NomCitoyen.Contains(search) ||
                    c.PrenomCitoyen.Contains(search) ||
                    c.CIN.Contains(search));

            ViewBag.Search = search;
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            var citoyen = await _db.Citoyens.Include(c => c.Fokontany)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (citoyen == null) return NotFound();
            return View(citoyen);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            var citoyen = await _db.Citoyens.FindAsync(id);
            if (citoyen == null) return NotFound();
            ViewBag.Fokontanys = await _db.Fokontany.ToListAsync();
            return View(citoyen);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Citoyen model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Fokontanys = await _db.Fokontany.ToListAsync();
                return View(model);
            }
            _db.Update(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Citoyen mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");
            var citoyen = await _db.Citoyens.FindAsync(id);
            if (citoyen != null)
            {
                _db.Citoyens.Remove(citoyen);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}