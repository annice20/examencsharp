using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers.API
{
    [Route("api/citoyens")]
    [ApiController]
    public class CitoyenApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CitoyenApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET api/citoyens
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var citoyens = await _db.Citoyens
                .Include(c => c.Fokontany)
                .Select(c => new {
                    c.Id, c.NomCitoyen, c.PrenomCitoyen,
                    c.CIN, c.Address, c.DateNaissance,
                    c.FokontanyId,
                    fokontany = c.Fokontany!.NomFokontany
                }).ToListAsync();
            return Ok(citoyens);
        }

        // GET api/citoyens/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _db.Citoyens
                .Include(c => c.Fokontany)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (c == null) return NotFound(new { message = "Citoyen introuvable" });
            return Ok(new {
                c.Id, c.NomCitoyen, c.PrenomCitoyen,
                c.CIN, c.Address, c.FokontanyId,
                fokontany = c.Fokontany?.NomFokontany
            });
        }

        // POST api/citoyens
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Citoyen citoyen)
        {
            citoyen.QRCodeToken = Guid.NewGuid().ToString();
            _db.Citoyens.Add(citoyen);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Citoyen créé", id = citoyen.Id });
        }

        // PUT api/citoyens/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Citoyen citoyen)
        {
            var existing = await _db.Citoyens.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Citoyen introuvable" });

            existing.NomCitoyen    = citoyen.NomCitoyen;
            existing.PrenomCitoyen = citoyen.PrenomCitoyen;
            existing.CIN           = citoyen.CIN;
            existing.Address       = citoyen.Address;
            existing.FokontanyId   = citoyen.FokontanyId;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Citoyen mis à jour" });
        }

        // DELETE api/citoyens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Citoyens.FindAsync(id);
            if (c == null) return NotFound(new { message = "Citoyen introuvable" });
            _db.Citoyens.Remove(c);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Citoyen supprimé" });
        }
    }
}