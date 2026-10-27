using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs
{
	public partial class CannotDeleteTableDialog : Window
	{
		public CannotDeleteTableDialog()
		{
			InitializeComponent();
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}