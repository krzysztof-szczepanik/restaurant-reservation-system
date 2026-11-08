using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.UI.Features.Reservations;

namespace RestaurantReservationSystem.UI.Features.Today
{
	public partial class TodayView : UserControl
	{

		public TodayView()
		{
			InitializeComponent();
			Loaded += TodayView_Loaded;
		}

		private void TodayView_Loaded(object sender, RoutedEventArgs e)
		{
			if (CurrentDateText != null)
			{
				CurrentDateText.Text = DateTime.Now.ToString("dd.MM.yyyy");
			}
			RefreshTodayList();
		}

		private void RefreshTodayList()
		{
			var dzisiejszeRezerwacje = ReservationsView.MockReservations
				.Where(r => r.Date == DateOnly.FromDateTime(DateTime.Today)).ToList();

			TodayDataGrid.ItemsSource = dzisiejszeRezerwacje;
		}

		private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
			{
				var newStatusString = selectedItem.Content.ToString() ?? "";
				var reservation = comboBox.DataContext as Reservation;
				var mainWindow = Window.GetWindow(this) as MainWindow;

				if (reservation == null || mainWindow == null) return;

				if (newStatusString == "Arrived")
				{
					reservation.Status = ReservationStatus.Arrived;
					comboBox.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0FDF4"));
					comboBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#16A34A"));
					RefreshTodayList();
				}
				else if (newStatusString == "Cancelled" || newStatusString == "NoShow" || newStatusString == "No-show")
				{
					var dialog = new RestaurantReservationSystem.UI.Features.Today.Dialogs.ChangeStatusDialog(newStatusString);
					dialog.CancelRequested += (_, _) => {
						mainWindow.DialogHost.Close(dialog, false);
					};
					dialog.ConfirmRequested += (_, _) => {
						if (Enum.TryParse<ReservationStatus>(newStatusString.Replace("-", ""), out var parsed))
						{
							reservation.Status = parsed;
						}
						RefreshTodayList();
						mainWindow.DialogHost.Close(dialog, true);
					};
					mainWindow.DialogHost.Show(dialog);
				}
				else if (newStatusString == "Unlock")
				{
					var dialog = new RestaurantReservationSystem.UI.Features.Reservations.Dialogs.UnlockReservationDialog();
					dialog.CancelRequested += (_, _) => {
						mainWindow.DialogHost.Close(dialog, false);
					};
					dialog.ConfirmRequested += (_, _) => {
						mainWindow.DialogHost.Close(dialog, true);

						mainWindow.MainContentArea.Content = new ReservationsView();

						var editDialog = new RestaurantReservationSystem.UI.Features.Reservations.Dialogs.AddEditReservationDialog(isEditMode: true);
						editDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(editDialog, false);
						editDialog.SaveRequested += (_, _) => mainWindow.DialogHost.Close(editDialog, true);
						mainWindow.DialogHost.Show(editDialog);
					};
					mainWindow.DialogHost.Show(dialog);
				}
				else
				{
					if (Enum.TryParse<ReservationStatus>(newStatusString.Replace("-", ""), out var parsed))
					{
						reservation.Status = parsed;
					}

					comboBox.ClearValue(ComboBox.BackgroundProperty);
					comboBox.ClearValue(ComboBox.ForegroundProperty);

					RefreshTodayList();
				}
			}
		}

		private ComboBoxItem? GetItemForStatus(ReservationStatus status)
		{
			return null;
		}

		private void TodayDataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			RefreshTodayList();
		}
	}
}