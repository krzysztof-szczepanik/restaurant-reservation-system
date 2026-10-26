using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Core.Repositories;
using RestaurantReservationSystem.Infrastructure.Persistence;
using RestaurantReservationSystem.Infrastructure.Repositories;
using System.Windows;

namespace RestaurantReservationSystem.UI;

public partial class App : System.Windows.Application {
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        string connectionString;

        try {
            connectionString = ConnectionStringProvider.GetConnectionString();
        } catch (Exception ex) {
            MessageBox.Show(
                $"Failed to initialize the database.\n\n{ex.Message}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
            return;
        }

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Scoped);

        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        ServiceProvider = services.BuildServiceProvider();

        try {
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        } catch (Exception ex) {
            MessageBox.Show(
                $"Failed to create the database directory.\n\n{ex.Message}",
                "Database Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
            return;
        }

        base.OnStartup(e);
    }
}