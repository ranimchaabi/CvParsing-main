using Microsoft.EntityFrameworkCore;
using CvParsing.Models;

namespace CvParsing.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Candidat> Candidats { get; set; }
    public DbSet<Cv> Cvs { get; set; }

    public DbSet<OffreEmploi> OffresEmploi { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidat>()
            .HasOne(c => c.Utilisateur)
            .WithOne()
            .HasForeignKey<Candidat>(c => c.id);

        modelBuilder.Entity<Cv>()
            .HasOne(c => c.Candidat)
            .WithMany()
            .HasForeignKey(c => c.id_candidat)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasIndex(t => t.TokenHashHex);
            e.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}