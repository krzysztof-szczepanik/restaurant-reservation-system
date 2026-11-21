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

			TodayDataGrid.ItemsSource = null;
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

				ReservationStatus previousStatus = reservation.Status;

				// Zatrzymujemy nasłuchiwanie zdarzenia, aby uniknąć zapętlenia przy programowej zmianie statusu
				comboBox.SelectionChanged -= StatusComboBox_SelectionChanged;

				if (newStatusString == "Arrived" || newStatusString == "Expected" || newStatusString == "Late")
				{
					if (Enum.TryParse<ReservationStatus>(newStatusString, out var parsed))
					{
						reservation.Status = parsed;
					}
					comboBox.SelectionChanged += StatusComboBox_SelectionChanged;
				}
				else if (newStatusString == "Cancelled" || newStatusString == "NoShow" || newStatusString == "No-show")
				{
					var dialog = new RestaurantReservationSystem.UI.Features.Today.Dialogs.ChangeStatusDialog(newStatusString);

					dialog.CancelRequested += (_, _) => {
						// Przywracamy stary status w przypadku kliknięcia Cancel
						foreach (ComboBoxItem item in comboBox.Items)
						{
							if (item.Content.ToString() == previousStatus.ToString())
							{
								comboBox.SelectedItem = item;
								break;
							}
						}

						mainWindow.DialogHost.Close(dialog, false);
						comboBox.SelectionChanged += StatusComboBox_SelectionChanged;
					};

					dialog.ConfirmRequested += (_, _) => {
						string parseString = newStatusString.Replace("-", "");
						if (Enum.TryParse<ReservationStatus>(parseString, out var parsed))
						{
							reservation.Status = parsed;
						}

						// Opcjonalne odświeżenie całej siatki, aby wymusić przepięcie triggerów XAML (jeśli potrzebne)
						RefreshTodayList();

						mainWindow.DialogHost.Close(dialog, true);
						comboBox.SelectionChanged += StatusComboBox_SelectionChanged;
					};

					mainWindow.DialogHost.Show(dialog);
				}
				else if (newStatusString == "Unlock")
				{
					var dialog = new RestaurantReservationSystem.UI.Features.Reservations.Dialogs.UnlockReservationDialog();

					dialog.CancelRequested += (_, _) => {
						foreach (ComboBoxItem item in comboBox.Items)
						{
							if (item.Content.ToString() == previousStatus.ToString())
							{
								comboBox.SelectedItem = item;
								break;
							}
						}

						mainWindow.DialogHost.Close(dialog, false);
						comboBox.SelectionChanged += StatusComboBox_SelectionChanged;
					};

					dialog.ConfirmRequested += (_, _) => {
						mainWindow.DialogHost.Close(dialog, true);
						mainWindow.MainContentArea.Content = new RestaurantReservationSystem.UI.Features.Reservations.ReservationsView();

						var editDialog = new RestaurantReservationSystem.UI.Features.Reservations.Dialogs.AddEditReservationDialog(isEditMode: true);
						editDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(editDialog, false);
						editDialog.SaveRequested += (_, _) => mainWindow.DialogHost.Close(editDialog, true);
						mainWindow.DialogHost.Show(editDialog);

						comboBox.SelectionChanged += StatusComboBox_SelectionChanged;
					};

					mainWindow.DialogHost.Show(dialog);
				}
				else
				{
					comboBox.SelectionChanged += StatusComboBox_SelectionChanged;
				}
			}
		}
	}
}