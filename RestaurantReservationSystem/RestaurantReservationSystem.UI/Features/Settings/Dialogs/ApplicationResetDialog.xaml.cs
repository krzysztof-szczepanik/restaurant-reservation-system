using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Settings.Dialogs
{
	public partial class ApplicationResetDialog : Window
	{
		public ApplicationResetDialog()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}