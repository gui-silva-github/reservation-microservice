using Microsoft.EntityFrameworkCore;
using Reservation.SpacesService.DAL.Entities;

namespace Reservation.SpacesService.DAL.Context
{
    public class SpacesDbContext : DbContext
    {
        public SpacesDbContext(DbContextOptions<SpacesDbContext> options) : base(options)
        {
        }

        public DbSet<Space> Spaces => Set<Space>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Space>(entity =>
            {
                entity.ToTable("Spaces");
                entity.HasKey(space => space.Id);
                entity.Property(space => space.Name).HasMaxLength(200).IsRequired();
                entity.Property(space => space.Location).HasMaxLength(200).IsRequired();
                entity.Property(space => space.Description).HasMaxLength(2000);
                entity.Property(space => space.PricePerHour).HasPrecision(18, 2);
                entity.Property(space => space.IsActive).HasDefaultValue(true);
            });
        }
    }
}