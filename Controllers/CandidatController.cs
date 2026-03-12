using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers
{
    public class CandidatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CandidatController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Liste candidats
        public async Task<IActionResult> Index()
        {
            var candidates = await _context.VCandidates.ToListAsync();
            return View(candidates);
        }

        // Formulaire
        public IActionResult Create()
        {
            ViewBag.Fokontany = _context.Fokontany.ToList();
            ViewBag.Elections = _context.Elections.ToList();

            return View();
        }

        // Enregistrement
        [HttpPost]
        public async Task<IActionResult> Create(Candidat candidat, IFormFile photo)
        {
            if (photo != null)
            {
                string path = Path.Combine(_env.WebRootPath, "photos");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);

                string filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                candidat.Photo = "/photos/" + fileName;
            }

            candidat.Validated = false;

            _context.Candidats.Add(candidat);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Validation candidat
        public async Task<IActionResult> Validate(int id)
        {
            var candidate = await _context.Candidats.FindAsync(id);

            if (candidate == null)
                return NotFound();

            candidate.Validated = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}