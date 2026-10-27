using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs
{
	public partial class AddTableDialog : Window
	{
		public AddTableDialog()
		{
			InitializeComponent();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}