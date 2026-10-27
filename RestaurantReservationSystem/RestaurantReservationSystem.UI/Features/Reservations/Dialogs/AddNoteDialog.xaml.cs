using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class AddNoteDialog : Window
	{
		public AddNoteDialog()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}