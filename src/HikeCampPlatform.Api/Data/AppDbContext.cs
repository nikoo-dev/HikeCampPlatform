using Microsoft.EntityFrameworkCore;
using HikeCampPlatform.Api.Models;

namespace HikeCampPlatform.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourRoutePoint> TourRoutePoints => Set<TourRoutePoint>();
    public DbSet<TourDeparture> TourDepartures => Set<TourDeparture>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Operator>()
            .HasIndex(o => o.Email)
            .IsUnique();

        modelBuilder.Entity<Tour>()
            .HasOne(t => t.Operator)
            .WithMany()
            .HasForeignKey(t => t.OperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TourRoutePoint>()
            .HasOne(rp => rp.Tour)
            .WithMany(t => t.RoutePoints)
            .HasForeignKey(rp => rp.TourId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TourDeparture>()
            .HasOne(td => td.Tour)
            .WithMany(t => t.Departures)
            .HasForeignKey(td => td.TourId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.TourDeparture)
            .WithMany(td => td.Bookings)
            .HasForeignKey(b => b.TourDepartureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}