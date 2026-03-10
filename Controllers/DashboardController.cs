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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
                return RedirectToAction("Login", "Account");

            var dashboard = new VDashboard
            {
                TotalCitizens   = await _db.Citoyens.CountAsync(),
                CinDelivered    = await _db.CinRequests.CountAsync(r => r.Status == "Approved"),
                PendingRequests = await _db.CinRequests.CountAsync(r => r.Status == "Pending"),
                TotalVotes      = await _db.Votes.CountAsync()
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
                        FirstName  = g.Key.PrenomCandidat,
                        LastName   = g.Key.NomCandidat,
                        TotalVotes = g.Count()
                    })
                    .OrderByDescending(r => r.TotalVotes)
                    .ToListAsync();

                ViewBag.Resultats = resultats;
                ViewBag.ElectionTitre = electionActive.TitreElection;
            }

            return View(dashboard);
        }
    }
}