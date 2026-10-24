using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<InitialConfiguration> InitialConfigurations => Set<InitialConfiguration>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.Tables)
                .WithMany(t => t.Reservations)
                .UsingEntity(j => j.ToTable("ReservationTables"));

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>();

            modelBuilder.Entity<InitialConfiguration>()
                .Property(c => c.TableIdentifierOrder)
                .HasConversion<string>();
        }
    }
}
