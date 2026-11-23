using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class ChangeStatusDialog : UserControl
	{

		// EVENTS:
		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;

		public string TargetStatus { get; }

		public string StatusDisplayText => TargetStatus switch
		{
			"No-show" => "No show?",
			_ => $"{TargetStatus}?"
		};

		// CONSTRUCTORS:
		public ChangeStatusDialog(string targetStatus = "Cancelled")
		{
			InitializeComponent();
			TargetStatus = targetStatus;
			DataContext = this;
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