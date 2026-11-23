using RestaurantReservationSystem.UI.Features.Settings.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Settings
{
	public partial class SettingsView : UserControl
	{

		// CONSTRUCTORS:
		// W przyszłości wstrzyknąć serwis resetujący bazę danych
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

			dialog.ResetRequested += (_, _) => {
				// TODO backend: Zresetowanie danych aplikacji w bazie danych
				mainWindow.DialogHost.Close(dialog, true);
			};

			mainWindow.DialogHost.Show(dialog);
		}
	}
}