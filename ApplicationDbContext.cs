using Microsoft.EntityFrameworkCore;
using examencsharp.Models;

namespace examencsharp
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}