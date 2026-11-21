using RestaurantReservationSystem.Core.Enums;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class ChangeStatusDialog : UserControl
	{

		// EVENTS:
		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;


		// CONSTRUCTORS:
		public ChangeStatusDialog()
		{
			InitializeComponent();
		}


		// METHODS:
		public void SetTargetStatus(ReservationStatus targetStatus)
		{
			StatusQuestionText.Text = targetStatus == ReservationStatus.NoShow
				? "status to No-show?"
				: "status to Cancelled?";
		}


		// EVENT HANDLERS:
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			ConfirmRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}