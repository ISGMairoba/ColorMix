/// <summary>
/// This file defines the database context for the ColorMix application.
/// The DbContext is Entity Framework's way of connecting to and interacting with the database.
/// It represents a session with the database and can be used to query and save data.
/// </summary>
using Microsoft.EntityFrameworkCore;
using ColorMix.Data.Entities;

namespace ColorMix.Data
{
    /// <summary>
    /// The main database context for ColorMix.
    /// This class inherits from DbContext (part of Entity Framework) and defines:
    /// - Which tables exist in the database (DbSet properties)
    /// - How tables are related to each other (OnModelCreating)
    /// - Database initialization logic
    /// </summary>
    public class ColorMixDbContext : DbContext
    {
        /// <summary>
        /// Colors table - Stores individual colors created by the user.
        /// Each DbSet represents a table in the database.
        /// </summary>
        public DbSet<ColorEntity> Colors { get; set; }
        
        /// <summary>
        /// PaletteVariants table - Stores color mix variants (different combinations of colors).
        /// A variant is a specific color created by mixing other colors in certain proportions.
        /// </summary>
        public DbSet<PaletteVariantEntity> PaletteVariants { get; set; }
        
        /// <summary>
        /// PaletteComponents table - Stores the individual color components that make up a variant.
        /// For example, if a variant is made of 2 parts red + 1 part blue, there would be 2 components.
        /// </summary>
        public DbSet<PaletteComponentEntity> PaletteComponents { get; set; }
        
        /// <summary>
        /// SavedPalettes table - Stores collections of colors saved by the user.
        /// A palette is a group of related colors.
        /// </summary>
        public DbSet<SavedPaletteEntity> SavedPalettes { get; set; }
        
        /// <summary>
        /// SavedPaletteColors table - Stores the individual colors that belong to a saved palette.
        /// This creates a many-to-many relationship between palettes and colors.
        /// </summary>
        public DbSet<SavedPaletteColorEntity> SavedPaletteColors { get; set; }

        /// <summary>
        /// Constructor - Creates a new database context with the specified configuration.
        /// The options parameter is injected by the dependency injection system and contains
        /// things like the database connection string.
        /// </summary>
        /// <param name="options">Configuration options for the database context</param>
        public ColorMixDbContext(DbContextOptions<ColorMixDbContext> options)
            : base(options)  // Pass options to the base DbContext class
        {
        }

        /// <summary>
        /// Configures the database model (tables, relationships, constraints).
        /// This method is called by Entity Framework when creating the database schema.
        /// It defines how entities map to tables and how they relate to each other.
        /// </summary>
        /// <param name="modelBuilder">The builder used to configure the database model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ColorEntity (the Colors table)
            modelBuilder.Entity<ColorEntity>(entity =>
            {
                entity.ToTable("Colors");  // Table name in the database
                entity.HasKey(e => e.Id);  // Id is the primary key
                entity.Property(e => e.ColorName).IsRequired().HasMaxLength(100);  // ColorName is required, max 100 characters
                entity.Property(e => e.HexValue).IsRequired().HasMaxLength(7);  // HexValue is required (e.g., "#FF5733")
                entity.HasIndex(e => e.ColorName);  // Create an index on ColorName for faster searches
            });

            // Configure PaletteVariantEntity (the PaletteVariants table)
            modelBuilder.Entity<PaletteVariantEntity>(entity =>
            {
                entity.ToTable("PaletteVariants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();  // Variant must have a name
                entity.Property(e => e.HexColor).IsRequired();  // Variant must have a color value
            });

            // Configure PaletteComponentEntity (the PaletteComponents table)
            modelBuilder.Entity<PaletteComponentEntity>(entity =>
            {
                entity.ToTable("PaletteComponents");
                entity.HasKey(e => e.Id);
                
                // Define relationship: A component belongs to one variant, a variant can have many components
                entity.HasOne(e => e.PaletteVariant)  // Each component has one parent variant
                      .WithMany(p => p.Components)     // Each variant has many components
                      .HasForeignKey(e => e.PaletteVariantId)  // Foreign key linking to the variant
                      .OnDelete(DeleteBehavior.Cascade);  // When variant is deleted, delete its components too
            });

            // Configure SavedPaletteEntity (the SavedPalettes table)
            modelBuilder.Entity<SavedPaletteEntity>(entity =>
            {
                entity.ToTable("SavedPalettes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();  // Palette must have a name
                
                // Define relationship: A palette has many colors
                entity.HasMany(e => e.Colors)  // A palette has many colors
                      .WithOne(c => c.SavedPalette)  // Each color belongs to one palette
                      .HasForeignKey(c => c.SavedPaletteId)  // Foreign key
                      .OnDelete(DeleteBehavior.Cascade);  // Delete colors when palette is deleted
                      
                // Define relationship: A palette has many variants
                entity.HasMany(e => e.Variants)  // A palette has many variants
                      .WithOne(v => v.SavedPalette)  // Each variant belongs to one palette
                      .HasForeignKey(v => v.SavedPaletteId)  // Foreign key
                      .OnDelete(DeleteBehavior.Cascade);  // Delete variants when palette is deleted
            });

            // Configure SavedPaletteColorEntity (the SavedPaletteColors table)
            modelBuilder.Entity<SavedPaletteColorEntity>(entity =>
            {
                entity.ToTable("SavedPaletteColors");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ColorName).IsRequired();
                entity.Property(e => e.HexValue).IsRequired();
            });
        }

        /// <summary>
        /// Initializes the database, creating it if it doesn't exist or updating it if needed.
        /// This method handles schema changes and ensures the database is in a working state.
        /// It's called automatically when the app starts (see MauiProgram.cs).
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Check if we can connect to the database
                bool canConnect = await Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Database exists, but check if it has the new tables we need
                    try
                    {
                        // Try to query the new tables
                        // This will throw an exception if the tables don't exist
                        await SavedPalettes.AnyAsync();
                        await SavedPaletteColors.AnyAsync();
                        // If we get here, the database schema is up to date
                    }
                    catch
                    {
                        // New tables don't exist, need to recreate database with new schema
                        // NOTE: This will delete all existing data!
                        System.Diagnostics.Debug.WriteLine("New tables not found, recreating database...");
                        await Database.EnsureDeletedAsync();  // Delete the old database
                        await Database.EnsureCreatedAsync();  // Create new database with current schema
                        System.Diagnostics.Debug.WriteLine("Database recreated successfully");
                    }
                }
                else
                {
                    // Database doesn't exist at all, create it
                    await Database.EnsureCreatedAsync();
                    System.Diagnostics.Debug.WriteLine("Database created successfully");
                }
            }
            catch (Exception ex)
            {
                // Something went wrong, log the error
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Last resort: delete and recreate the database
                // This ensures the app doesn't crash due to database issues
                try
                {
                    await Database.EnsureDeletedAsync();
                    await Database.EnsureCreatedAsync();
                    System.Diagnostics.Debug.WriteLine("Database force-recreated after error");
                }
                catch (Exception recreateEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to recreate database: {recreateEx.Message}");
                    throw;  // Re-throw if we can't even recreate the database
                }
            }
        }
    }
}
