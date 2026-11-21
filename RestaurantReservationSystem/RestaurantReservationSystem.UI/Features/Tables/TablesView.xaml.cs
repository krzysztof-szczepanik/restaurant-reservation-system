using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.Tables.Dialogs;

namespace RestaurantReservationSystem.UI.Features.Tables
{
	public partial class TablesView : UserControl
	{

		public static ObservableCollection<Table> MockTables = new ObservableCollection<Table>();

		public TablesView()
		{
			InitializeComponent();
			Loaded += (_, _) => TablesDataGrid.ItemsSource = MockTables;
		}

		private MainWindow? GetMainWindow() => Window.GetWindow(this) as MainWindow;

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;

			var dialog = new AddEditTableDialog();
			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);
			dialog.SaveRequested += (_, _) => {
				MockTables.Add(new Table
				{
					Number = int.TryParse(dialog.TableNumber, out int n) ? n : 0,
					Label = dialog.TableLabel ?? "",
					Zone = dialog.TableZone ?? "",
					Seats = int.TryParse(dialog.TableSeats, out int s) ? s : 0
				});
				mainWindow.DialogHost.Close(dialog, true);
			};
			mainWindow.DialogHost.Show(dialog);
		}

		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			if (TablesDataGrid.SelectedItem is not Table selectedTable) return;
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;

			var dialog = new AddEditTableDialog(
				selectedTable.Number.ToString() ?? "",
				selectedTable.Label ?? "",
				selectedTable.Zone ?? "",
				selectedTable.Seats.ToString() ?? "");

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);
			dialog.SaveRequested += (_, _) => {
				selectedTable.Number = int.TryParse(dialog.TableNumber, out int n) ? n : 0;
				selectedTable.Label = dialog.TableLabel ?? "";
				selectedTable.Zone = dialog.TableZone ?? "";
				selectedTable.Seats = int.TryParse(dialog.TableSeats, out int s) ? s : 0;
				TablesDataGrid.Items.Refresh();
				mainWindow.DialogHost.Close(dialog, true);
			};
			mainWindow.DialogHost.Show(dialog);
		}

		private void DeleteTab_Click(object sender, RoutedEventArgs e)
		{
			if (TablesDataGrid.SelectedItem is not Table selectedTable) return;
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;

			var deleteDialog = new DeleteTablesDialog();
			deleteDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(deleteDialog, false);
			deleteDialog.DeleteRequested += (_, _) => {
				MockTables.Remove(selectedTable);
				mainWindow.DialogHost.Close(deleteDialog, true);
			};
			mainWindow.DialogHost.Show(deleteDialog);
		}
	}
}