using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;

namespace HelpDesk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Chamado> Chamados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações opcionais para as tabelas
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}