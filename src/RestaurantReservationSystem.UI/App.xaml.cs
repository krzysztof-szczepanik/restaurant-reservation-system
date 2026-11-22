using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Application.Services;
using RestaurantReservationSystem.Infrastructure.Persistence;

namespace RestaurantReservationSystem.UI
{
	public partial class App : System.Windows.Application
	{

		// FIELDS:
		public static IServiceProvider ServiceProvider { get; private set; } = null!;


		// METHODS:
		protected override void OnStartup(StartupEventArgs e)
		{
			string connectionString;

			try
			{
				connectionString = ConnectionStringProvider.GetConnectionString();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to initialize the database.\n\n{ex.Message}",
					"Database Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);

				Shutdown(-1);
				return;
			}

			var services = new ServiceCollection();

			// Database:
			services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

			// Services:
			services.AddScoped<IConfigurationService, ConfigurationService>();
			services.AddScoped<ITableService, TableService>();
			services.AddScoped<IReservationService, ReservationService>();
			services.AddScoped<IApplicationResetService, ApplicationResetService>();

			ServiceProvider = services.BuildServiceProvider();

			try
			{
				using var scope = ServiceProvider.CreateScope();
				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

				var databasePath = dbContext.Database.GetDbConnection().DataSource;

				if (File.Exists(databasePath))
				{
					dbContext.Database.Migrate();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to initialize the database.\n\n{ex.Message}",
					"Database Initialization Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);

				Shutdown(-1);
				return;
			}
			EventManager.RegisterClassHandler(
				typeof(Button),
				Button.ClickEvent,
				new RoutedEventHandler((sender, _) =>
				{
					if (sender is Button { Name: "ClearSearchButton", TemplatedParent: TextBox textBox })
					{
						textBox.Clear();
						textBox.Focus();
					}
				}));
			base.OnStartup(e);
		}
	}
}