using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp.Controllers.API
{
    [Route("api/elections")]
    [ApiController]
    public class ElectionApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ElectionApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET api/elections
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var elections = await _db.Elections
                .Include(e => e.Fokontany)
                .Select(e => new {
                    e.Id, e.TitreElection, e.DateElection,
                    e.IsActive, e.FokontanyId,
                    fokontany = e.Fokontany!.NomFokontany
                }).ToListAsync();
            return Ok(elections);
        }

        // GET api/elections/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var election = await _db.Elections
                .Include(e => e.Fokontany)
                .FirstOrDefaultAsync(e => e.IsActive);

            if (election == null)
                return NotFound(new { message = "Aucune élection active" });

            return Ok(new {
                election.Id, election.TitreElection,
                election.DateElection, election.IsActive,
                fokontany = election.Fokontany?.NomFokontany
            });
        }

        // GET api/elections/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _db.Elections.FindAsync(id);
            if (e == null) return NotFound(new { message = "Election introuvable" });
            return Ok(e);
        }

        // POST api/elections
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Election election)
        {
            _db.Elections.Add(election);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Election créée", id = election.Id });
        }

        // PUT api/elections/5/toggle
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var election = await _db.Elections.FindAsync(id);
            if (election == null) return NotFound(new { message = "Election introuvable" });

            election.IsActive = !election.IsActive;
            await _db.SaveChangesAsync();
            return Ok(new {
                message = election.IsActive ? "Election activée" : "Election désactivée",
                isActive = election.IsActive
            });
        }

        // DELETE api/elections/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _db.Elections.FindAsync(id);
            if (e == null) return NotFound(new { message = "Election introuvable" });
            _db.Elections.Remove(e);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Election supprimée" });
        }
    }
}