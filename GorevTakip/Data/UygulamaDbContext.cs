using GorevTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace GorevTakip.Data
{
    public class UygulamaDbContext : DbContext
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gorev> Gorevler { get; set; } = null!;
    }
}
