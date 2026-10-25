using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Core.Repositories;
using RestaurantReservationSystem.Infrastructure.Persistence;
using RestaurantReservationSystem.Infrastructure.Repositories;
using System.IO;
using System.Windows;

namespace RestaurantReservationSystem.UI;

public partial class App : System.Windows.Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        string dbPath = ConnectionStringProvider.GetConnectionString();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(dbPath));

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(dbPath), ServiceLifetime.Scoped);

        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        ServiceProvider = services.BuildServiceProvider();

        try
        {
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }
        catch (Exception)
        {
            Shutdown(-1);
            throw;
        }
    }
}