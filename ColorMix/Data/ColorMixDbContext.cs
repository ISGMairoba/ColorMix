using Microsoft.EntityFrameworkCore;
using ColorMix.Data.Entities;

namespace ColorMix.Data
{
    public class ColorMixDbContext : DbContext
    {
        public DbSet<ColorEntity> Colors { get; set; }
        public DbSet<PaletteVariantEntity> PaletteVariants { get; set; }
        public DbSet<PaletteComponentEntity> PaletteComponents { get; set; }

        public ColorMixDbContext(DbContextOptions<ColorMixDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ColorEntity
            modelBuilder.Entity<ColorEntity>(entity =>
            {
                entity.ToTable("Colors");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ColorName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HexValue).IsRequired().HasMaxLength(7);
                entity.HasIndex(e => e.ColorName);
            });

            // Configure PaletteVariantEntity
            modelBuilder.Entity<PaletteVariantEntity>(entity =>
            {
                entity.ToTable("PaletteVariants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.HexColor).IsRequired();
            });

            // Configure PaletteComponentEntity
            modelBuilder.Entity<PaletteComponentEntity>(entity =>
            {
                entity.ToTable("PaletteComponents");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.PaletteVariant)
                      .WithMany(p => p.Components)
                      .HasForeignKey(e => e.PaletteVariantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public async Task InitializeDatabaseAsync()
        {
            await Database.EnsureCreatedAsync();
        }
    }
}
