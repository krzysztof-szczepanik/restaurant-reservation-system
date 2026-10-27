using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class FilterResDial : Window
	{
		public FilterResDial()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Apply_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
		}

		private void Clean_Click(object sender, RoutedEventArgs e)
		{
		}

		private void SelectZones_Click(object sender, RoutedEventArgs e)
		{
			var selectZonesDialog = new SelectZonesDialog();
			selectZonesDialog.Owner = this;
			selectZonesDialog.ShowDialog();
		}

		private void SelectTables_Click(object sender, RoutedEventArgs e)
		{
			var selectTablesDialog = new SelectTablesDialog();
			selectTablesDialog.Owner = this;
			selectTablesDialog.ShowDialog();
		}
	}
}