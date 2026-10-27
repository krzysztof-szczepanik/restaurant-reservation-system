using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class AddResDial : Window
	{
		public AddResDial()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void SelectTables_Click(object sender, RoutedEventArgs e)
		{
			var selectTablesDialog = new SelectTablesDialog();
			selectTablesDialog.Owner = this;
			selectTablesDialog.ShowDialog();
		}

		private void AddNote_Click(object sender, RoutedEventArgs e)
		{
			var addNoteDialog = new AddNoteDialog();
			addNoteDialog.Owner = this;
			addNoteDialog.ShowDialog();
		}
	}
}