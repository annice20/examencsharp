using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers
{
    public class VoteController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly QRCodeService _qrService;

        public VoteController(ApplicationDbContext db, QRCodeService qrService)
        {
            _db = db;
            _qrService = qrService;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var election = await _db.Elections
                .Include(e => e.Fokontany)
                .FirstOrDefaultAsync(e => e.IsActive);

            if (election == null)
                return View("AucuneElection");

            var userId = HttpContext.Session.GetInt32("userId");
            var citoyen = await _db.Citoyens
                .FirstOrDefaultAsync(c => c.UtilisateurId == userId);

            if (citoyen == null)
                return View("NonEligible");

            var dejaVote = await _db.Votes
                .AnyAsync(v => v.CitoyenId == citoyen.Id && v.ElectionId == election.Id);

            if (dejaVote)
                return View("DejaVote");

            var candidats = await _db.Candidats
                .Where(c => c.ElectionId == election.Id)
                .ToListAsync();

            ViewBag.Election = election;
            return View(candidats);
        }

        // Générer QR Code avec URL complète
        public IActionResult GenerateQR(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            var citoyen = _db.Citoyens.Find(id);
            if (citoyen == null) return NotFound();

            // Seul le citoyen concerné peut générer son QR
            if (role == "Citoyen" && citoyen.UtilisateurId != userId)
                return Forbid();

            // Admin ne peut pas générer de QR
            if (role == "Admin")
                return Forbid();

            var baseUrl = GetLocalBaseUrl();
            var qrBytes = _qrService.GenerateQRCode(citoyen.QRCodeToken, baseUrl);
            return File(qrBytes, "image/png");
        }

        // Méthode helper — détecte l'IP locale automatiquement
        private string GetLocalBaseUrl()
        {
            var host = Request.Host.Host;
            var port = Request.Host.Port ?? 5207;
            var scheme = Request.Scheme;

            // Si localhost → remplacer par l'IP locale réelle
            if (host == "localhost" || host == "127.0.0.1")
            {
                host = System.Net.NetworkInformation.NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                            && n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault() ?? "localhost";
            }

            return $"{scheme}://{host}:{port}";
        }

        // Page scan
        public async Task<IActionResult> Scan()
        {
            var election = await _db.Elections
                .FirstOrDefaultAsync(e => e.IsActive);

            ViewBag.ElectionId = election?.Id;
            ViewBag.Candidats = await _db.Candidats
                .Where(c => c.ElectionId == election.Id)
                .ToListAsync();

            return View();
        }

        // API Vérification GET /Vote/Verify/{qrCode}
        [HttpGet]
        public async Task<IActionResult> Verify(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Content("TOKEN VIDE");

            var citoyen = await _db.Citoyens
                .Include(c => c.Fokontany)
                .FirstOrDefaultAsync(c => c.QRCodeToken == id);

            if (citoyen == null)
                return Content($"INTROUVABLE: {id}");

            if (citoyen.ADejaVote)
                return Content("DEJA VOTE");

            // Passer explicitement le nom de la vue
            return View("Verify", citoyen);
        }

        // Vérification POST (scan manuel)
        [HttpPost]
        public IActionResult Verifier(string token)
        {
            var citoyen = _db.Citoyens
                .FirstOrDefault(c => c.QRCodeToken == token);

            if (citoyen == null)
                return Content("Citoyen introuvable");

            if (citoyen.ADejaVote)
                return Content("Déjà voté");

            return Content($"Bienvenue {citoyen.NomCitoyen}");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Voter(int candidatId, int electionId, string token)
        {
            var userEmail = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var citoyen = _db.Citoyens
                .FirstOrDefault(c => c.QRCodeToken == token);

            if (citoyen == null)
            {
                ViewBag.Error = "Votre compte n'est pas lié à un citoyen.";
                return RedirectToAction("Index");
            }

            var dejaVote = await _db.Votes
                .AnyAsync(v => v.CitoyenId == citoyen.Id && v.ElectionId == electionId);
            if (dejaVote)
                return View("DejaVote");

            // Marquer comme ayant voté
            citoyen.ADejaVote = true;

            _db.Votes.Add(new Vote
            {
                CitoyenId  = citoyen.Id,
                CandidatId = candidatId,
                ElectionId = electionId,
                VoteDate   = DateTime.UtcNow,
                QRCode     = Guid.NewGuid().ToString()
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            return View();
        }
    }
}