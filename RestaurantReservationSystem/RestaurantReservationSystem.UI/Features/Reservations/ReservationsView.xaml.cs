using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.Reservations.Dialogs;

namespace RestaurantReservationSystem.UI.Features.Reservations
{
	public partial class ReservationsView : UserControl
	{

		public static ObservableCollection<Reservation> MockReservations = new ObservableCollection<Reservation>();
		private ReservationFilterCriteria _activeCriteria = new();

		public bool HasActiveFilters
		{
			get => FilterButton.Tag is true;
			set => FilterButton.Tag = value;
		}

		public ReservationsView()
		{
			InitializeComponent();
			SearchBox.TextChanged += SearchBox_TextChanged;
			Loaded += (_, _) => ApplySearchAndFilter();
		}

		private MainWindow? GetMainWindow() => Window.GetWindow(this) as MainWindow;

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ApplySearchAndFilter();
		}

		private void ApplySearchAndFilter()
		{
			var text = SearchBox.Text?.ToLower() ?? "";

			if (string.IsNullOrWhiteSpace(text))
			{
				ReservationsDataGrid.ItemsSource = MockReservations;
			}
			else
			{
				ReservationsDataGrid.ItemsSource = MockReservations.Where(r =>
					(r.FirstName != null && r.FirstName.ToLower().Contains(text)) ||
					(r.LastName != null && r.LastName.ToLower().Contains(text)) ||
					(r.PhoneNumber != null && r.PhoneNumber.Contains(text))).ToList();
			}
		}

		private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!SearchBox.IsKeyboardFocusWithin) return;
			FocusManager.SetFocusedElement(FocusManager.GetFocusScope(SearchBox), null);
			Keyboard.ClearFocus();
		}

		private void AddReservations_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;

			var dialog = new AddEditReservationDialog();
			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);
			dialog.SaveRequested += (_, _) => {

				TimeOnly reservationTime = new TimeOnly(18, 0);

				var timeBox = dialog.FindName("FTimeBox") as TextBox ?? dialog.FindName("TimeBox") as TextBox;
				if (timeBox != null && !string.IsNullOrWhiteSpace(timeBox.Text))
				{
					if (TimeOnly.TryParse(timeBox.Text, out var parsed))
					{
						reservationTime = parsed;
					}
				}

				MockReservations.Add(new Reservation
				{
					Date = DateOnly.FromDateTime(DateTime.Today),
					Time = reservationTime, 
					Guests = 2,
					FirstName = dialog.FindName("FirstNameBox") is TextBox fn && !string.IsNullOrEmpty(fn.Text) ? fn.Text : "Gość",
					LastName = dialog.FindName("LastNameBox") is TextBox ln && !string.IsNullOrEmpty(ln.Text) ? ln.Text : "Restauracji",
					PhoneNumber = dialog.FindName("PhoneBox") is TextBox ph && !string.IsNullOrEmpty(ph.Text) ? ph.Text : "",
					Status = Core.Enums.ReservationStatus.Expected
				});

				ApplySearchAndFilter();
				mainWindow.DialogHost.Close(dialog, true);
			};
			mainWindow.DialogHost.Show(dialog);
		}

		private void EditReservations_Click(object sender, RoutedEventArgs e)
		{
			if (ReservationsDataGrid.SelectedItem is not Reservation selectedReservation) return;
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;

			var statusStr = selectedReservation.Status.ToString();
			if (statusStr == "Cancelled" || statusStr == "NoShow")
			{
				var unlockDialog = new UnlockReservationDialog();
				unlockDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(unlockDialog, false);
				unlockDialog.ConfirmRequested += (_, _) => {
					mainWindow.DialogHost.Close(unlockDialog, true);
					OpenEditDialog(mainWindow);
				};
				mainWindow.DialogHost.Show(unlockDialog);
			}
			else
			{
				OpenEditDialog(mainWindow);
			}
		}

		private void OpenEditDialog(MainWindow mainWindow)
		{
			var dialog = new AddEditReservationDialog(isEditMode: true);
			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);
			dialog.SaveRequested += (_, _) => {
				ApplySearchAndFilter();
				mainWindow.DialogHost.Close(dialog, true);
			};
			mainWindow.DialogHost.Show(dialog);
		}

		private void DeleteReservations_Click(object sender, RoutedEventArgs e)
		{
			if (ReservationsDataGrid.SelectedItem is not Reservation selectedReservation) return;
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;

			var deleteDialog = new DeleteReservationsDialog();
			deleteDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(deleteDialog, false);
			deleteDialog.DeleteRequested += (_, _) => {
				MockReservations.Remove(selectedReservation);
				ApplySearchAndFilter();
				mainWindow.DialogHost.Close(deleteDialog, true);
			};
			mainWindow.DialogHost.Show(deleteDialog);
		}

		private void Filter_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow == null) return;
			var dialog = new FilterReservationsDialog(_activeCriteria);
			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);
			dialog.ApplyRequested += (_, newCriteria) => {
				_activeCriteria = newCriteria;
				HasActiveFilters = !string.IsNullOrEmpty(_activeCriteria.Date) || _activeCriteria.TableIds.Count > 0 || _activeCriteria.ZoneIds.Count > 0;
				mainWindow.DialogHost.Close(dialog, true);
			};
			mainWindow.DialogHost.Show(dialog);
		}
	}
}