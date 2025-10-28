using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;

namespace PatoPrimordialAPI.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
    public DbSet<Drone> Drones => Set<Drone>();
    public DbSet<PontoReferencia> PontosReferencia => Set<PontoReferencia>();
    public DbSet<Pato> Patos => Set<Pato>();
    public DbSet<AnalisePato> AnalisesPatos => Set<AnalisePato>();
    public DbSet<ParametroAnalise> ParametrosAnalise => Set<ParametroAnalise>();
    public DbSet<Avistamento> Avistamentos => Set<Avistamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Fabricante>(entity =>
        {
            entity.ToTable("fabricantes");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Nome).IsRequired().HasMaxLength(200);
            entity.Property(f => f.Marca).HasMaxLength(200);
            entity.Property(f => f.PaisOrigem).HasMaxLength(120);
        });

        modelBuilder.Entity<Drone>(entity =>
        {
            entity.ToTable("drones");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.NumeroSerie).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Status).IsRequired().HasMaxLength(100);
            entity.Property(d => d.PrecisaoNominalMinCm).HasColumnType("numeric(18,4)");
            entity.Property(d => d.PrecisaoNominalMaxM).HasColumnType("numeric(18,4)");
            entity.HasIndex(d => d.NumeroSerie).IsUnique();
            entity.HasOne(d => d.Fabricante)
                .WithMany(f => f.Drones)
                .HasForeignKey(d => d.FabricanteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PontoReferencia>(entity =>
        {
            entity.ToTable("pontos_referencia");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(200);
            entity.Property(p => p.RaioMetros).HasColumnType("double precision");
        });

        modelBuilder.Entity<Pato>(entity =>
        {
            entity.ToTable("patos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Codigo).IsRequired().HasMaxLength(200);
            entity.Property(p => p.AlturaCm).HasColumnType("numeric(18,4)");
            entity.Property(p => p.PesoG).HasColumnType("numeric(18,4)");
            entity.Property(p => p.PrecisaoM).HasColumnType("numeric(18,4)");
            entity.Property(p => p.Estado).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Cidade).HasMaxLength(200);
            entity.Property(p => p.Pais).HasMaxLength(120);
            entity.Property(p => p.PoderNome).HasMaxLength(200);
            entity.Property(p => p.PoderTagsCsv).HasMaxLength(500);
            entity.Property(p => p.CriadoEm).HasColumnType("timestamp with time zone");
            entity.Property(p => p.AtualizadoEm).HasColumnType("timestamp with time zone");
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.HasOne(p => p.PontoReferencia)
                .WithMany(pr => pr.Patos)
                .HasForeignKey(p => p.PontoReferenciaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Avistamento>(entity =>
        {
            entity.ToTable("avistamentos");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AlturaValor).HasColumnType("numeric(18,4)");
            entity.Property(a => a.PesoValor).HasColumnType("numeric(18,4)");
            entity.Property(a => a.AlturaCm).HasColumnType("numeric(18,4)");
            entity.Property(a => a.PesoG).HasColumnType("numeric(18,4)");
            entity.Property(a => a.PrecisaoValor).HasColumnType("numeric(18,4)");
            entity.Property(a => a.PrecisaoM).HasColumnType("numeric(18,4)");
            entity.Property(a => a.Confianca).HasColumnType("double precision");
            entity.Property(a => a.Cidade).HasMaxLength(200);
            entity.Property(a => a.Pais).HasMaxLength(120);
            entity.Property(a => a.EstadoPato).HasMaxLength(50);
            entity.Property(a => a.CriadoEm).HasColumnType("timestamp with time zone");
            entity.HasIndex(a => a.CriadoEm);
            entity.HasOne(a => a.Drone)
                .WithMany(d => d.Avistamentos)
                .HasForeignKey(a => a.DroneId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Pato)
                .WithMany(p => p.Avistamentos)
                .HasForeignKey(a => a.PatoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParametroAnalise>(entity =>
        {
            entity.ToTable("parametros_analise");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Chave).IsRequired().HasMaxLength(200);
            entity.Property(p => p.ValorNum).HasColumnType("double precision");
            entity.Property(p => p.ValorTxt).HasMaxLength(500);
            entity.Property(p => p.AtualizadoEm).HasColumnType("timestamp with time zone");
            entity.HasIndex(p => p.Chave).IsUnique();
        });

        modelBuilder.Entity<AnalisePato>(entity =>
        {
            entity.ToTable("analise_pato");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.CustoTransporte).HasColumnType("double precision");
            entity.Property(a => a.Prioridade).HasColumnType("double precision");
            entity.Property(a => a.DistKm).HasColumnType("double precision");
            entity.Property(a => a.ClassePrioridade).IsRequired().HasMaxLength(10);
            entity.Property(a => a.ClasseRisco).IsRequired().HasMaxLength(10);
            entity.Property(a => a.CalculadoEm).HasColumnType("timestamp with time zone");
            entity.HasIndex(a => a.PatoId).IsUnique();
            entity.HasOne(a => a.Pato)
                .WithOne(p => p.Analise)
                .HasForeignKey<AnalisePato>(a => a.PatoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
