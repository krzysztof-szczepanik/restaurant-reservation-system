using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class ChangeStatusDialog : Window
	{
		public ChangeStatusDialog()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}