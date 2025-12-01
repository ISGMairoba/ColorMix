using Microsoft.EntityFrameworkCore;
using ColorMix.Data.Entities;

namespace ColorMix.Data
{
    public class ColorMixDbContext : DbContext
    {
        public DbSet<ColorEntity> Colors { get; set; }
        public DbSet<PaletteVariantEntity> PaletteVariants { get; set; }
        public DbSet<PaletteComponentEntity> PaletteComponents { get; set; }
        public DbSet<SavedPaletteEntity> SavedPalettes { get; set; }
        public DbSet<SavedPaletteColorEntity> SavedPaletteColors { get; set; }

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

            // Configure SavedPaletteEntity
            modelBuilder.Entity<SavedPaletteEntity>(entity =>
            {
                entity.ToTable("SavedPalettes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.HasMany(e => e.Colors)
                      .WithOne(c => c.SavedPalette)
                      .HasForeignKey(c => c.SavedPaletteId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Variants)
                      .WithOne(v => v.SavedPalette)
                      .HasForeignKey(v => v.SavedPaletteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SavedPaletteColorEntity
            modelBuilder.Entity<SavedPaletteColorEntity>(entity =>
            {
                entity.ToTable("SavedPaletteColors");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ColorName).IsRequired();
                entity.Property(e => e.HexValue).IsRequired();
            });
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Check if database can be opened and has correct schema
                bool canConnect = await Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Try to check if new tables exist
                    try
                    {
                        // This will throw if tables don't exist
                        await SavedPalettes.AnyAsync();
                        await SavedPaletteColors.AnyAsync();
                    }
                    catch
                    {
                        // New tables don't exist, need to recreate database
                        System.Diagnostics.Debug.WriteLine("New tables not found, recreating database...");
                        await Database.EnsureDeletedAsync();
                        await Database.EnsureCreatedAsync();
                        System.Diagnostics.Debug.WriteLine("Database recreated successfully");
                    }
                }
                else
                {
                    // Database doesn't exist, create it
                    await Database.EnsureCreatedAsync();
                    System.Diagnostics.Debug.WriteLine("Database created successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Last resort: delete and recreate
                try
                {
                    await Database.EnsureDeletedAsync();
                    await Database.EnsureCreatedAsync();
                    System.Diagnostics.Debug.WriteLine("Database force-recreated after error");
                }
                catch (Exception recreateEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to recreate database: {recreateEx.Message}");
                    throw;
                }
            }
        }
    }
}
