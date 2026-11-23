using System;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Today.Dialogs
{
	public partial class ChangeStatusDialog : UserControl
	{
		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;

		public ChangeStatusDialog(string status)
		{
			InitializeComponent();
			MessageText.Text = $"Are you sure you want to change the status to {status}?";
		}
		private void Cancel_Click(object sender, RoutedEventArgs e) => CancelRequested?.Invoke(this, EventArgs.Empty);
		private void Confirm_Click(object sender, RoutedEventArgs e) => ConfirmRequested?.Invoke(this, EventArgs.Empty);
	}
}