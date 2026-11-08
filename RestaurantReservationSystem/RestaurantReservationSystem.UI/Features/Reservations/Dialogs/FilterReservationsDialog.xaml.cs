using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class FilterReservationsDialog : UserControl
	{
		// EVENTS
		public event EventHandler<ReservationFilterCriteria>? ApplyRequested;
		public event EventHandler? CancelRequested;

		// FIELDS
		private readonly ReservationFilterCriteria _currentCriteria;
		private bool _isDateRangeMode = false;
		private bool _isTimeRangeMode = false;
		private bool _isGuestsRangeMode = false;

		// CONSTRUCTORS
		public FilterReservationsDialog(ReservationFilterCriteria? existingCriteria = null)
		{
			InitializeComponent();

			_currentCriteria = existingCriteria ?? new ReservationFilterCriteria();
			LoadCriteriaToUI();
		}

		// METHODS
		private void LoadCriteriaToUI()
		{
			FDateBox.Text = _currentCriteria.Date ?? string.Empty;
			FTimeBox.Text = _currentCriteria.Time ?? string.Empty;
			FGuestsBox.Text = _currentCriteria.Guests?.ToString() ?? string.Empty;
		}

		private void DateMode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.Tag is string mode)
			{
				_isDateRangeMode = mode == "Range";
				if (_isDateRangeMode)
				{
					DateExactPanel.Visibility = Visibility.Collapsed;
					DateRangePanel.Visibility = Visibility.Visible;
					SetTabActive(DateRangeTabButton, DateExactTabButton);
				}
				else
				{
					DateRangePanel.Visibility = Visibility.Collapsed;
					DateExactPanel.Visibility = Visibility.Visible;
					SetTabActive(DateExactTabButton, DateRangeTabButton);
				}
			}
		}

		private void TimeMode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.Tag is string mode)
			{
				_isTimeRangeMode = mode == "Range";
				if (_isTimeRangeMode)
				{
					TimeExactPanel.Visibility = Visibility.Collapsed;
					TimeRangePanel.Visibility = Visibility.Visible;
					SetTabActive(TimeRangeTabButton, TimeExactTabButton);
				}
				else
				{
					TimeRangePanel.Visibility = Visibility.Collapsed;
					TimeExactPanel.Visibility = Visibility.Visible;
					SetTabActive(TimeExactTabButton, TimeRangeTabButton);
				}
			}
		}

		private void GuestsMode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.Tag is string mode)
			{
				_isGuestsRangeMode = mode == "Range";
				if (_isGuestsRangeMode)
				{
					GuestsExactPanel.Visibility = Visibility.Collapsed;
					GuestsRangePanel.Visibility = Visibility.Visible;
					SetTabActive(GuestsRangeTabButton, GuestsExactTabButton);
				}
				else
				{
					GuestsRangePanel.Visibility = Visibility.Collapsed;
					GuestsExactPanel.Visibility = Visibility.Visible;
					SetTabActive(GuestsExactTabButton, GuestsRangeTabButton);
				}
			}
		}

		private void SetTabActive(Button active, Button inactive)
		{
			active.Background = (Brush)FindResource("Surface");
			active.Foreground = (Brush)FindResource("Primary");
			active.FontWeight = FontWeights.SemiBold;

			inactive.Background = Brushes.Transparent;
			inactive.Foreground = (Brush)FindResource("SecondaryText");
			inactive.FontWeight = FontWeights.Normal;
		}

		private bool ValidateAndSaveCriteria()
		{
			bool isValid = true;

			ShowError(DateErrorText, DateSpacer, null, FDateBox);
			ShowError(TimeErrorText, TimeSpacer, null, FTimeBox);
			ShowError(GuestsErrorText, GuestsSpacer, null, FGuestsBox);

			if (_isDateRangeMode)
			{
				string fromStr = FDateFromBox.Text.Trim();
				string toStr = FDateToBox.Text.Trim();
				bool hasFrom = !string.IsNullOrEmpty(fromStr);
				bool hasTo = !string.IsNullOrEmpty(toStr);

				if (hasFrom && !DateTime.TryParse(fromStr, out DateTime dtFrom))
				{
					ShowError(DateErrorText, DateSpacer, "Enter a valid start date.", FDateFromBox);
					isValid = false;
				}
				else if (hasTo && !DateTime.TryParse(toStr, out DateTime dtTo))
				{
					ShowError(DateErrorText, DateSpacer, "Enter a valid end date.", FDateToBox);
					isValid = false;
				}
				else if (hasFrom && hasTo && DateTime.TryParse(fromStr, out DateTime from) && DateTime.TryParse(toStr, out DateTime to))
				{
					if (from > to)
					{
						ShowError(DateErrorText, DateSpacer, "Start date cannot be after end date.", FDateFromBox, FDateToBox);
						isValid = false;
					}
				}
			}
			else
			{
				string dateStr = FDateBox.Text.Trim();
				if (!string.IsNullOrEmpty(dateStr) && !DateTime.TryParse(dateStr, out _))
				{
					ShowError(DateErrorText, DateSpacer, "Enter a valid date. Use dd.MM.yyyy.", FDateBox);
					isValid = false;
				}
			}

			if (_isGuestsRangeMode)
			{
				string fromStr = FGuestsFromBox.Text.Trim();
				string toStr = FGuestsToBox.Text.Trim();
				bool hasFrom = !string.IsNullOrEmpty(fromStr);
				bool hasTo = !string.IsNullOrEmpty(toStr);

				if (hasFrom && (!int.TryParse(fromStr, out int gFrom) || gFrom <= 0))
				{
					ShowError(GuestsErrorText, GuestsSpacer, "Enter a number greater than 0.", FGuestsFromBox);
					isValid = false;
				}
				else if (hasTo && (!int.TryParse(toStr, out int gTo) || gTo <= 0))
				{
					ShowError(GuestsErrorText, GuestsSpacer, "Enter a number greater than 0.", FGuestsToBox);
					isValid = false;
				}
				else if (hasFrom && hasTo && int.TryParse(fromStr, out int gf) && int.TryParse(toStr, out int gt))
				{
					if (gf > gt)
					{
						ShowError(GuestsErrorText, GuestsSpacer, "Minimum guests cannot exceed maximum guests.", FGuestsFromBox, FGuestsToBox);
						isValid = false;
					}
				}
			}
			else
			{
				string guestStr = FGuestsBox.Text.Trim();
				if (!string.IsNullOrEmpty(guestStr) && (!int.TryParse(guestStr, out int g) || g <= 0))
				{
					ShowError(GuestsErrorText, GuestsSpacer, "Enter a number greater than 0.", FGuestsBox);
					isValid = false;
				}
			}

			return isValid;
		}

		private void ShowError(TextBlock errorTextBlock, FrameworkElement spacer, string? message, params Control[] textBoxes)
		{
			if (string.IsNullOrEmpty(message))
			{
				errorTextBlock.Visibility = Visibility.Collapsed;
				spacer.Visibility = Visibility.Visible;
				foreach (var tb in textBoxes)
				{
					tb.ClearValue(BorderBrushProperty);
				}
			}
			else
			{
				errorTextBlock.Text = message;
				errorTextBlock.Visibility = Visibility.Visible;
				spacer.Visibility = Visibility.Collapsed;
				var dangerBrush = (Brush)FindResource("Danger");
				foreach (var tb in textBoxes)
				{
					tb.BorderBrush = dangerBrush;
				}
			}
		}

		// EVENT HANDLERS
		private void Apply_Click(object sender, RoutedEventArgs e)
		{
			if (ValidateAndSaveCriteria())
			{
				_currentCriteria.Date = _isDateRangeMode ? $"{FDateFromBox.Text}\n{FDateToBox.Text}" : FDateBox.Text;
				_currentCriteria.Time = _isTimeRangeMode ? $"{FTimeFromBox.Text}\n{FTimeToBox.Text}" : FTimeBox.Text;
				if (!_isGuestsRangeMode && int.TryParse(FGuestsBox.Text, out int guests))
				{
					_currentCriteria.Guests = guests;
				}

				ApplyRequested?.Invoke(this, _currentCriteria);
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			FDateBox.Text = string.Empty;
			FDateFromBox.Text = string.Empty;
			FDateToBox.Text = string.Empty;
			FTimeBox.Text = string.Empty;
			FTimeFromBox.Text = string.Empty;
			FTimeToBox.Text = string.Empty;
			FGuestsBox.Text = string.Empty;
			FGuestsFromBox.Text = string.Empty;
			FGuestsToBox.Text = string.Empty;

			ShowError(DateErrorText, DateSpacer, null, FDateBox, FDateFromBox, FDateToBox);
			ShowError(TimeErrorText, TimeSpacer, null, FTimeBox, FTimeFromBox, FTimeToBox);
			ShowError(GuestsErrorText, GuestsSpacer, null, FGuestsBox, FGuestsFromBox, FGuestsToBox);
		}

		private void Clean_Click(object sender, RoutedEventArgs e)
		{
			Reset_Click(sender, e);
		}

		private void SelectZones_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) return;

			var selectZonesDialog = new SelectZonesDialog();
			selectZonesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(selectZonesDialog, false);
			selectZonesDialog.ConfirmRequested += (_, _) => {
				mainWindow.DialogHost.Close(selectZonesDialog, true);
			};
			mainWindow.DialogHost.Show(selectZonesDialog);
		}

		private void SelectTables_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) return;

			var selectTablesDialog = new SelectTablesDialog();
			selectTablesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(selectTablesDialog, false);
			selectTablesDialog.ConfirmRequested += (_, _) => {
				mainWindow.DialogHost.Close(selectTablesDialog, true);
			};
			mainWindow.DialogHost.Show(selectTablesDialog);
		}
	}

	public class ReservationFilterCriteria
	{
		public string? Date { get; set; }
		public string? Time { get; set; }
		public int? Guests { get; set; }
		public List<string> ZoneIds { get; set; } = new();
		public List<int> TableIds { get; set; } = new();
		public string SearchTerm { get; set; } = string.Empty;
	}
}