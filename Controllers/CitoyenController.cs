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

            // Récupérer le citoyen existant pour garder son token
            var existing = await _db.Citoyens.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.NomCitoyen    = model.NomCitoyen;
            existing.PrenomCitoyen = model.PrenomCitoyen;
            existing.CIN           = model.CIN;
            existing.DateNaissance = model.DateNaissance;
            existing.Address       = model.Address;
            existing.FokontanyId   = model.FokontanyId;
            existing.UtilisateurId = model.UtilisateurId;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Citoyen mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create()
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");

            // Seul un Citoyen peut accéder à cette page
            var role = HttpContext.Session.GetString("role");
            if (role != "Utilisateur")
                return RedirectToAction("Index", "Home");

            // Vérifier qu'il n'a pas déjà un profil citoyen
            var userId = HttpContext.Session.GetInt32("userId");
            var dejaLie = await _db.Citoyens.AnyAsync(c => c.UtilisateurId == userId);
            if (dejaLie)
            {
                TempData["Error"] = "Vous avez déjà un profil citoyen.";
                return RedirectToAction("Index");
            }

            ViewBag.Fokontanys = await _db.Fokontany.ToListAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Citoyen model)
        {
            if (!EstConnecte()) return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("role");
            if (role != "Utilisateur")
                return RedirectToAction("Index", "Home");

            // Récupérer l'userId depuis la session automatiquement
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Forcer l'UtilisateurId depuis la session
            model.UtilisateurId = userId.Value;

            // Générer le token automatiquement
            model.QRCodeToken = Guid.NewGuid().ToString();

            // Retirer les champs auto du ModelState
            ModelState.Remove("QRCodeToken");
            ModelState.Remove("UtilisateurId");
            ModelState.Remove("Utilisateur");

            if (!ModelState.IsValid)
            {
                ViewBag.Fokontanys = await _db.Fokontany.ToListAsync();
                return View(model);
            }

            _db.Citoyens.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Profil citoyen créé avec succès !";
            return RedirectToAction("Index");
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

        public async Task<IActionResult> FixTokens()
        {
            var citoyens = _db.Citoyens
                .Where(c => c.QRCodeToken == null || c.QRCodeToken == "")
                .ToList();

            foreach (var c in citoyens)
                c.QRCodeToken = Guid.NewGuid().ToString();

            await _db.SaveChangesAsync();
            return Content($"{citoyens.Count} tokens générés avec succès !");
        }
    }
}
