using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CandidatApiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET api/candidatapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var candidates = await _context.VCandidates.ToListAsync();
            return Ok(candidates);
        }

        // GET api/candidatapi/form-data
        public class OptionDto
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
        }

        // GET api/candidatapi/form-data
        [HttpGet("form-data")]
        public IActionResult GetFormData()
        {
            var fokontany = _context.Fokontany
                .Select(f => new OptionDto { Id = f.Id, Nom = f.NomFokontany })
                .ToList();

            var elections = _context.Elections
                .Select(e => new OptionDto { Id = e.Id, Nom = e.TitreElection })
                .ToList();

            return Ok(new { fokontany, elections });
        }

        // POST api/candidatapi
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CandidatCreateDto dto)
        {
            var candidat = new Candidat
            {
                NomCandidat        = dto.Nom,
                PrenomCandidat     = dto.Prenom,
                Programme          = dto.Programme,
                FokontanyId        = dto.FokontanyId,
                ElectionId         = dto.ElectionId,
                Validated          = false
            };

            if (dto.Photo != null)
            {
                string path = Path.Combine(_env.WebRootPath, "photos");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Photo.FileName);
                string filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                candidat.Photo = "/photos/" + fileName;
            }

            _context.Candidats.Add(candidat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = candidat.Id }, candidat);
        }

        // PUT api/candidatapi/{id}/validate
        [HttpPut("{id}/validate")]
        public async Task<IActionResult> Validate(int id)
        {
            var candidate = await _context.Candidats.FindAsync(id);
            if (candidate == null)
                return NotFound(new { message = "Candidat non trouvé" });

            candidate.Validated = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Candidat validé avec succès" });
        }
    }

    // DTO pour la création (multipart/form-data)
    public class CandidatCreateDto
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Programme { get; set; } = string.Empty;
        public int FokontanyId { get; set; }
        public int ElectionId { get; set; }
        public IFormFile? Photo { get; set; }
    }
}