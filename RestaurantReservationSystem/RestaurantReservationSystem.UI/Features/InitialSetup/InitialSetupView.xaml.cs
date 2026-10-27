using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.InitialSetup
{
	public partial class InitialSetupView : UserControl
	{
		public InitialSetupView()
		{
			InitializeComponent();
		}

		private void Submit_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Your restaurant profile has been successfully created.", "Setup Completed", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}