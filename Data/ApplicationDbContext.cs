using Libri.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Libro> Libri { get; set; }
    public DbSet<Autore> Autori { get; set; }
    public DbSet<Recensione> Recensioni { get; set; }
    public DbSet<LibroAutore> LibroAutori { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurazione relazione molti-a-molti
        modelBuilder.Entity<LibroAutore>()
            .HasKey(la => new { la.LibroId, la.AutoreId });

        modelBuilder.Entity<LibroAutore>()
            .HasOne(la => la.Libro)
            .WithMany(l => l.Autori)
            .HasForeignKey(la => la.LibroId);

        modelBuilder.Entity<LibroAutore>()
            .HasOne(la => la.Autore)
            .WithMany(a => a.Libri)
            .HasForeignKey(la => la.AutoreId);
    }
}