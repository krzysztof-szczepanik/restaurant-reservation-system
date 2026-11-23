using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class FilterReservationsDialog : UserControl
	{

		// EVENTS:
		public event EventHandler? CancelRequested;
		public event EventHandler? ApplyRequested;


		// FIELDS:
		private FilterValueMode _dateMode = FilterValueMode.Exact;
		private FilterValueMode _timeMode = FilterValueMode.Exact;
		private FilterValueMode _guestsMode = FilterValueMode.Exact;
		private FilterMatchMode _zoneMatchMode = FilterMatchMode.Or;
		private FilterMatchMode _tableMatchMode = FilterMatchMode.Or;

		private List<string?> _selectedZones = new();
		private List<int> _selectedTableIds = new();
		private string? _searchText;
		private InitialConfiguration _configuration = new();


		// PROPERTIES:
		public ReservationFilterCriteria BuiltCriteria { get; private set; } = new();


		// CONSTRUCTORS:
		public FilterReservationsDialog()
		{
			InitializeComponent();
			UpdateAllModeStyles();
			UpdateSelectionLabels();
		}


		// METHODS:
		public void ApplyConfiguration(InitialConfiguration configuration)
		{
			_configuration = configuration;
			ZoneFilterSection.Visibility = configuration.TableZoneVisible
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		public void LoadCriteria(ReservationFilterCriteria criteria)
		{
			_searchText = criteria.SearchText;

			_dateMode = criteria.Date.Mode;
			FDateExactBox.Text = criteria.Date.Exact?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
			FDateFromBox.Text = criteria.Date.From?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
			FDateToBox.Text = criteria.Date.To?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

			_timeMode = criteria.Time.Mode;
			FTimeExactBox.Text = criteria.Time.Exact?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
			FTimeFromBox.Text = criteria.Time.From?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
			FTimeToBox.Text = criteria.Time.To?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;

			_guestsMode = criteria.Guests.Mode;
			FGuestsExactBox.Text = criteria.Guests.Exact?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
			FGuestsFromBox.Text = criteria.Guests.From?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
			FGuestsToBox.Text = criteria.Guests.To?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

			_zoneMatchMode = criteria.ZoneMatchMode;
			_selectedZones = criteria.Zones.ToList();

			_tableMatchMode = criteria.TableMatchMode;
			_selectedTableIds = criteria.TableIds.Distinct().ToList();

			ClearValidationErrors();
			UpdateAllModeStyles();
			UpdateSelectionLabels();
		}

		private void ClearValidationErrors()
		{
			ResetBoxBorder(FDateExactBox);
			ResetBoxBorder(FDateFromBox);
			ResetBoxBorder(FDateToBox);
			HideError(DateExactErrorText);
			HideError(DateFromErrorText);
			HideError(DateToErrorText);

			ResetBoxBorder(FTimeExactBox);
			ResetBoxBorder(FTimeFromBox);
			ResetBoxBorder(FTimeToBox);
			HideError(TimeExactErrorText);
			HideError(TimeFromErrorText);
			HideError(TimeToErrorText);

			ResetBoxBorder(FGuestsExactBox);
			ResetBoxBorder(FGuestsFromBox);
			ResetBoxBorder(FGuestsToBox);
			HideError(GuestsExactErrorText);
			HideError(GuestsFromErrorText);
			HideError(GuestsToErrorText);

			HideError(GeneralFilterErrorText);
		}

		private void ResetBoxBorder(TextBox box)
		{
			box.BorderBrush = (Brush)FindResource("Border");
		}

		private void SetErrorBorder(TextBox box)
		{
			box.BorderBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
		}

		private static void ShowError(TextBlock target, string message)
		{
			target.Text = message;
			target.Visibility = Visibility.Visible;
		}

		private static void HideError(TextBlock target)
		{
			target.Text = string.Empty;
			target.Visibility = Visibility.Collapsed;
		}

		public bool TryBuildCriteria(out ReservationFilterCriteria criteria)
		{
			criteria = new ReservationFilterCriteria();
			ClearValidationErrors();

			bool isValid = true;
			DateOnly today = DateOnly.FromDateTime(DateTime.Now);

			DateOnly? dateExact = null;
			DateOnly? dateFrom = null;
			DateOnly? dateTo = null;

			if (_dateMode == FilterValueMode.Exact)
			{
				if (!TryParseNullableDate(FDateExactBox.Text, out dateExact, out var err))
				{
					SetErrorBorder(FDateExactBox);
					ShowError(DateExactErrorText, err!);
					isValid = false;
				}
				else if (dateExact.HasValue && dateExact.Value < today)
				{
					SetErrorBorder(FDateExactBox);
					ShowError(DateExactErrorText, "Date cannot be in the past.");
					isValid = false;
				}
			}
			else
			{
				bool fromOk = TryParseNullableDate(FDateFromBox.Text, out dateFrom, out var fromErr);
				bool toOk = TryParseNullableDate(FDateToBox.Text, out dateTo, out var toErr);

				if (!fromOk)
				{
					SetErrorBorder(FDateFromBox);
					ShowError(DateFromErrorText, fromErr!);
					isValid = false;
				}
				else if (dateFrom.HasValue && dateFrom.Value < today)
				{
					SetErrorBorder(FDateFromBox);
					ShowError(DateFromErrorText, "Date cannot be in the past.");
					isValid = false;
				}

				if (!toOk)
				{
					SetErrorBorder(FDateToBox);
					ShowError(DateToErrorText, toErr!);
					isValid = false;
				}
				else if (dateTo.HasValue && dateTo.Value < today)
				{
					SetErrorBorder(FDateToBox);
					ShowError(DateToErrorText, "Date cannot be in the past.");
					isValid = false;
				}

				if (fromOk && toOk && dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
				{
					SetErrorBorder(FDateFromBox);
					SetErrorBorder(FDateToBox);
					ShowError(DateFromErrorText, "Start date cannot be after end date.");
					isValid = false;
				}
			}

			TimeOnly? timeExact = null;
			TimeOnly? timeFrom = null;
			TimeOnly? timeTo = null;

			if (_timeMode == FilterValueMode.Exact)
			{
				if (!TryParseNullableTime(FTimeExactBox.Text, out timeExact, out var err))
				{
					SetErrorBorder(FTimeExactBox);
					ShowError(TimeExactErrorText, err!);
					isValid = false;
				}
			}
			else
			{
				bool fromOk = TryParseNullableTime(FTimeFromBox.Text, out timeFrom, out var fromErr);
				bool toOk = TryParseNullableTime(FTimeToBox.Text, out timeTo, out var toErr);

				if (!fromOk)
				{
					SetErrorBorder(FTimeFromBox);
					ShowError(TimeFromErrorText, fromErr!);
					isValid = false;
				}

				if (!toOk)
				{
					SetErrorBorder(FTimeToBox);
					ShowError(TimeToErrorText, toErr!);
					isValid = false;
				}

				if (fromOk && toOk && timeFrom.HasValue && timeTo.HasValue && timeFrom.Value > timeTo.Value)
				{
					SetErrorBorder(FTimeFromBox);
					SetErrorBorder(FTimeToBox);
					ShowError(TimeFromErrorText, "Start time cannot be after end time.");
					isValid = false;
				}
			}

			int? guestsExact = null;
			int? guestsFrom = null;
			int? guestsTo = null;

			if (_guestsMode == FilterValueMode.Exact)
			{
				if (!TryParseNullablePositiveInt(FGuestsExactBox.Text, out guestsExact, out var err))
				{
					SetErrorBorder(FGuestsExactBox);
					ShowError(GuestsExactErrorText, err!);
					isValid = false;
				}
			}
			else
			{
				bool fromOk = TryParseNullablePositiveInt(FGuestsFromBox.Text, out guestsFrom, out var fromErr);
				bool toOk = TryParseNullablePositiveInt(FGuestsToBox.Text, out guestsTo, out var toErr);

				if (!fromOk)
				{
					SetErrorBorder(FGuestsFromBox);
					ShowError(GuestsFromErrorText, fromErr!);
					isValid = false;
				}

				if (!toOk)
				{
					SetErrorBorder(FGuestsToBox);
					ShowError(GuestsToErrorText, toErr!);
					isValid = false;
				}

				if (fromOk && toOk && guestsFrom.HasValue && guestsTo.HasValue && guestsFrom.Value > guestsTo.Value)
				{
					SetErrorBorder(FGuestsFromBox);
					SetErrorBorder(FGuestsToBox);
					ShowError(GuestsFromErrorText, "Minimum guests cannot exceed maximum guests.");
					isValid = false;
				}
			}

			if (!isValid)
			{
				return false;
			}

			criteria = new ReservationFilterCriteria
			{
				SearchText = _searchText,
				Date = new ValueFilter<DateOnly>
				{
					Mode = _dateMode,
					Exact = dateExact,
					From = dateFrom,
					To = dateTo
				},
				Time = new ValueFilter<TimeOnly>
				{
					Mode = _timeMode,
					Exact = timeExact,
					From = timeFrom,
					To = timeTo
				},
				Guests = new ValueFilter<int>
				{
					Mode = _guestsMode,
					Exact = guestsExact,
					From = guestsFrom,
					To = guestsTo
				},
				Zones = _configuration.TableZoneVisible ? _selectedZones.ToList() : new List<string?>(),
				ZoneMatchMode = _zoneMatchMode,
				TableIds = _selectedTableIds.ToList(),
				TableMatchMode = _tableMatchMode
			};

			return true;
		}

		private static bool TryParseNullableDate(string raw, out DateOnly? value, out string? error)
		{
			value = null;
			error = null;
			string trimmed = raw.Trim();
			if (string.IsNullOrEmpty(trimmed))
			{
				return true;
			}

			if (!DateOnly.TryParseExact(trimmed, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
			{
				error = "Enter a valid date.";
				return false;
			}

			value = parsed;
			return true;
		}

		private static bool TryParseNullableTime(string raw, out TimeOnly? value, out string? error)
		{
			value = null;
			error = null;
			string trimmed = raw.Trim();
			if (string.IsNullOrEmpty(trimmed))
			{
				return true;
			}

			if (!TimeOnly.TryParseExact(trimmed, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
			{
				error = "Enter a valid time.";
				return false;
			}

			value = parsed;
			return true;
		}

		private static bool TryParseNullablePositiveInt(string raw, out int? value, out string? error)
		{
			value = null;
			error = null;
			string trimmed = raw.Trim();
			if (string.IsNullOrEmpty(trimmed))
			{
				return true;
			}

			if (!int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
			{
				error = "Enter a number greater than 0.";
				return false;
			}

			value = parsed;
			return true;
		}

		private void UpdateAllModeStyles()
		{
			SetModeButtons(DateExactModeButton, DateRangeModeButton, _dateMode == FilterValueMode.Exact);
			DateExactPanel.Visibility = _dateMode == FilterValueMode.Exact ? Visibility.Visible : Visibility.Collapsed;
			DateRangePanel.Visibility = _dateMode == FilterValueMode.Range ? Visibility.Visible : Visibility.Collapsed;

			SetModeButtons(TimeExactModeButton, TimeRangeModeButton, _timeMode == FilterValueMode.Exact);
			TimeExactPanel.Visibility = _timeMode == FilterValueMode.Exact ? Visibility.Visible : Visibility.Collapsed;
			TimeRangePanel.Visibility = _timeMode == FilterValueMode.Range ? Visibility.Visible : Visibility.Collapsed;

			SetModeButtons(GuestsExactModeButton, GuestsRangeModeButton, _guestsMode == FilterValueMode.Exact);
			GuestsExactPanel.Visibility = _guestsMode == FilterValueMode.Exact ? Visibility.Visible : Visibility.Collapsed;
			GuestsRangePanel.Visibility = _guestsMode == FilterValueMode.Range ? Visibility.Visible : Visibility.Collapsed;

			SetModeButtons(ZoneOrButton, ZoneAndButton, _zoneMatchMode == FilterMatchMode.Or);
			SetModeButtons(TablesOrButton, TablesAndButton, _tableMatchMode == FilterMatchMode.Or);
		}

		private void SetModeButtons(Button firstActiveButton, Button secondActiveButton, bool firstIsActive)
		{
			var primaryBrush = (Brush)FindResource("Primary");
			var secondaryBrush = (Brush)FindResource("SecondaryText");
			var surfaceBrush = (Brush)FindResource("Surface");

			firstActiveButton.Background = firstIsActive ? surfaceBrush : Brushes.Transparent;
			firstActiveButton.Foreground = firstIsActive ? primaryBrush : secondaryBrush;
			firstActiveButton.FontWeight = firstIsActive ? FontWeights.SemiBold : FontWeights.Normal;

			secondActiveButton.Background = !firstIsActive ? surfaceBrush : Brushes.Transparent;
			secondActiveButton.Foreground = !firstIsActive ? primaryBrush : secondaryBrush;
			secondActiveButton.FontWeight = !firstIsActive ? FontWeights.SemiBold : FontWeights.Normal;
		}

		private void UpdateSelectionLabels()
		{
			SelectedZonesText.Text = $"Selected zones: {_selectedZones.Count}";
			SelectedTablesText.Text = $"Selected tables: {_selectedTableIds.Count}";
		}


		// EVENT HANDLERS:
		private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && ApplyButton.IsEnabled)
			{
				e.Handled = true;
				Apply_Click(ApplyButton, new RoutedEventArgs());
			}
		}

		private void FilterField_TextChanged(object sender, TextChangedEventArgs e)
		{
			HideError(GeneralFilterErrorText);

			if (sender == FDateExactBox)
			{
				ResetBoxBorder(FDateExactBox);
				HideError(DateExactErrorText);
			}
			else if (sender == FDateFromBox || sender == FDateToBox)
			{
				ResetBoxBorder(FDateFromBox);
				ResetBoxBorder(FDateToBox);
				HideError(DateFromErrorText);
				HideError(DateToErrorText);
			}
			else if (sender == FTimeExactBox)
			{
				ResetBoxBorder(FTimeExactBox);
				HideError(TimeExactErrorText);
			}
			else if (sender == FTimeFromBox || sender == FTimeToBox)
			{
				ResetBoxBorder(FTimeFromBox);
				ResetBoxBorder(FTimeToBox);
				HideError(TimeFromErrorText);
				HideError(TimeToErrorText);
			}
			else if (sender == FGuestsExactBox)
			{
				ResetBoxBorder(FGuestsExactBox);
				HideError(GuestsExactErrorText);
			}
			else if (sender == FGuestsFromBox || sender == FGuestsToBox)
			{
				ResetBoxBorder(FGuestsFromBox);
				ResetBoxBorder(FGuestsToBox);
				HideError(GuestsFromErrorText);
				HideError(GuestsToErrorText);
			}
		}

		private void DateMode_Click(object sender, RoutedEventArgs e)
		{
			_dateMode = sender == DateRangeModeButton ? FilterValueMode.Range : FilterValueMode.Exact;
			UpdateAllModeStyles();
		}

		private void TimeMode_Click(object sender, RoutedEventArgs e)
		{
			_timeMode = sender == TimeRangeModeButton ? FilterValueMode.Range : FilterValueMode.Exact;
			UpdateAllModeStyles();
		}

		private void GuestsMode_Click(object sender, RoutedEventArgs e)
		{
			_guestsMode = sender == GuestsRangeModeButton ? FilterValueMode.Range : FilterValueMode.Exact;
			UpdateAllModeStyles();
		}

		private void ZoneMatchMode_Click(object sender, RoutedEventArgs e)
		{
			_zoneMatchMode = sender == ZoneAndButton ? FilterMatchMode.And : FilterMatchMode.Or;
			UpdateAllModeStyles();
		}

		private void TablesMatchMode_Click(object sender, RoutedEventArgs e)
		{
			_tableMatchMode = sender == TablesAndButton ? FilterMatchMode.And : FilterMatchMode.Or;
			UpdateAllModeStyles();
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			_dateMode = FilterValueMode.Exact;
			_timeMode = FilterValueMode.Exact;
			_guestsMode = FilterValueMode.Exact;
			_zoneMatchMode = FilterMatchMode.Or;
			_tableMatchMode = FilterMatchMode.Or;

			FDateExactBox.Text = string.Empty;
			FDateFromBox.Text = string.Empty;
			FDateToBox.Text = string.Empty;

			FTimeExactBox.Text = string.Empty;
			FTimeFromBox.Text = string.Empty;
			FTimeToBox.Text = string.Empty;

			FGuestsExactBox.Text = string.Empty;
			FGuestsFromBox.Text = string.Empty;
			FGuestsToBox.Text = string.Empty;

			_selectedZones.Clear();
			_selectedTableIds.Clear();

			ClearValidationErrors();
			UpdateAllModeStyles();
			UpdateSelectionLabels();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private async void Apply_Click(object sender, RoutedEventArgs e)
		{
			if (!TryBuildCriteria(out var criteria))
			{
				return;
			}

			try
			{
				using var scope = App.ServiceProvider.CreateScope();
				var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
				var validation = await reservationService.ValidateFiltersAsync(criteria);

				if (!validation.IsSuccess)
				{
					string message = validation.ValidationErrors.Count > 0
						? string.Join("\n", validation.ValidationErrors.Select(err => err.Message))
						: (validation.Message ?? "Invalid filter criteria.");

					ShowError(GeneralFilterErrorText, message);
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to validate filters.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}

			BuiltCriteria = criteria;
			ApplyRequested?.Invoke(this, EventArgs.Empty);
		}

		private async void SelectZones_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) { return; }

			var selectZonesDialog = new SelectZonesDialog();
			selectZonesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(selectZonesDialog, false);
			selectZonesDialog.ConfirmRequested += (_, _) =>
			{
				_selectedZones = selectZonesDialog.SelectedZones.ToList();
				UpdateSelectionLabels();
				mainWindow.DialogHost.Close(selectZonesDialog, true);
			};

			await selectZonesDialog.LoadZonesAsync(_selectedZones);
			mainWindow.DialogHost.Show(selectZonesDialog);
		}

		private async void SelectTables_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) { return; }

			var selectTablesDialog = new SelectTablesDialog(isFilterMode: true);

			selectTablesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(selectTablesDialog, false);
			selectTablesDialog.ConfirmRequested += (_, _) =>
			{
				_selectedTableIds = selectTablesDialog.SelectedTableIds.ToList();
				UpdateSelectionLabels();
				mainWindow.DialogHost.Close(selectTablesDialog, true);
			};

			await selectTablesDialog.LoadTablesAsync(
				DateOnly.FromDateTime(DateTime.Now),
				new TimeOnly(12, 0),
				guests: 0,
				_selectedTableIds,
				checkAvailability: false);

			mainWindow.DialogHost.Show(selectTablesDialog);
		}
	}
}