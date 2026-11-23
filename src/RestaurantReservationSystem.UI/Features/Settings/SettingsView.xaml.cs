using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.UI.Features.Settings.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Settings
{
	public partial class SettingsView : UserControl
	{

		// CONSTRUCTORS:
		public SettingsView()
		{
			InitializeComponent();
		}


		// EVENT HANDLERS:
		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;

			if (mainWindow == null)
			{
				return;
			}

			var dialog = new ApplicationResetDialog();

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.ResetRequested += async (_, _) =>
			{
				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var resetService = scope.ServiceProvider.GetRequiredService<IApplicationResetService>();

					await resetService.ResetAsync();

					mainWindow.DialogHost.Close(dialog, true);
					mainWindow.ShowInitialSetup();
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"Failed to reset application.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}
	}
}