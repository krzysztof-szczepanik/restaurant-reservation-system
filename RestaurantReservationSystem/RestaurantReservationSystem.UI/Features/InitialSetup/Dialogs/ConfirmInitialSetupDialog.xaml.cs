using System.Windows;

namespace RestaurantReservationSystem.UI.Features.InitialSetup.Dialogs
{
	public partial class ConfirmInitialSetupDialog : Window
	{
		public ConfirmInitialSetupDialog()
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