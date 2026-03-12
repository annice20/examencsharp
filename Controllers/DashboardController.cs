using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models.Views;

namespace examencsharp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            // Vérifier session
            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
                //return RedirectToAction("Login", "Account");

            var dashboard = new VDashboard
            {
                total_citoyens   = await _db.Citoyens.CountAsync(),
                cin_delivered    = await _db.CinRequests.CountAsync(r => r.Status == "Approved"),
                pending_requests = await _db.CinRequests.CountAsync(r => r.Status == "Pending"),
                total_votes      = await _db.Votes.CountAsync()
            };

            var electionActive = await _db.Elections.FirstOrDefaultAsync(e => e.IsActive);
            if (electionActive != null)
            {
                var resultats = await _db.Votes
                    .Where(v => v.ElectionId == electionActive.Id)
                    .Include(v => v.Candidat)
                    .GroupBy(v => new { v.Candidat!.NomCandidat, v.Candidat.PrenomCandidat })
                    .Select(g => new VResult
                    {
                        NomCandidat  = g.Key.NomCandidat,
                        PrenomCandidat   = g.Key.PrenomCandidat,
                        total_votes = g.Count()
                    })
                    .OrderByDescending(r => r.total_votes)
                    .ToListAsync();

                ViewBag.Resultats = resultats;
                ViewBag.ElectionTitre = electionActive.TitreElection;
            }

            return View(dashboard);
        }
    }
}