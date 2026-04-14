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
            // ✅ Non connecté → Login
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
                return RedirectToAction("Login", "Account");

            // ✅ Utilisateur → redirigé vers son profil citoyen
            if (HttpContext.Session.GetString("role") == "Utilisateur")
                return RedirectToAction("Index", "Citoyen");

            // ✅ Pas Admin → Login
            if (HttpContext.Session.GetString("role") != "Admin")
                return RedirectToAction("Login", "Account");

            var dashboard = new VDashboard
            {
                total_citoyens   = await _db.Citoyens.CountAsync(),
                cin_delivered    = await _db.CinRequests.CountAsync(r => r.Status == "Approuvé"),
                pending_requests = await _db.CinRequests.CountAsync(r => r.Status == "En attente"),
                total_votes      = await _db.Votes.CountAsync()
            };

            var electionActive = await _db.Elections.FirstOrDefaultAsync(e => e.IsActive);
            if (electionActive != null)
            {
                var candidats = await _db.Candidats
                    .Where(c => c.ElectionId == electionActive.Id)
                    .ToListAsync();

                var resultats = new List<ResultatAvecVotants>();
                foreach (var candidat in candidats)
                {
                    var votants = await _db.Votes
                        .Where(v => v.CandidatId == candidat.Id && v.ElectionId == electionActive.Id)
                        .Select(v => new VotantInfo
                        {
                            NomPrenom = _db.Citoyens
                                .Where(c => c.UtilisateurId == v.CitoyenId)
                                .Select(c => c.NomCitoyen + " " + c.PrenomCitoyen)
                                .FirstOrDefault() ??
                                _db.Utilisateurs
                                .Where(u => u.Id == v.CitoyenId)
                                .Select(u => u.Email)
                                .FirstOrDefault() ?? "Utilisateur #" + v.CitoyenId,
                            DateVote = v.VoteDate
                        })
                        .ToListAsync();

                    resultats.Add(new ResultatAvecVotants
                    {
                        NomCandidat    = candidat.NomCandidat,
                        PrenomCandidat = candidat.PrenomCandidat,
                        TotalVotes     = votants.Count,
                        Votants        = votants
                    });
                }

                ViewBag.Resultats     = resultats.OrderByDescending(r => r.TotalVotes).ToList();
                ViewBag.ElectionTitre = electionActive.TitreElection;
                ViewBag.TotalVotes    = resultats.Sum(r => r.TotalVotes);
            }

            return View(dashboard);
        }
    }

    public class ResultatAvecVotants
    {
        public string NomCandidat    { get; set; } = "";
        public string PrenomCandidat { get; set; } = "";
        public int    TotalVotes     { get; set; }
        public List<VotantInfo> Votants { get; set; } = new();
    }

    public class VotantInfo
    {
        public string   NomPrenom { get; set; } = "";
        public DateTime DateVote  { get; set; }
    }
}