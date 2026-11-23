using RestaurantReservationSystem.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AppValidationError = RestaurantReservationSystem.Application.OperationResults.ValidationError;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class AddEditReservationDialog : UserControl
	{

		// REGEXES:
		private static readonly Regex FirstNameRegex = new(@"^[\p{L}](?:[\p{L}\s'\-]*[\p{L}])?$", RegexOptions.Compiled);
		private static readonly Regex TwoLastNamesWithSpaceRegex = new(@"^[\p{L}]+(?:['\-][\p{L}]+)*\s+[\p{L}]+(?:['\-][\p{L}]+)*$", RegexOptions.Compiled);
		private static readonly Regex LastNameRegex = new(@"^[\p{L}]+(?:['\-][\p{L}]+)*$", RegexOptions.Compiled);
		private static readonly Regex PhoneCharsRegex = new(@"^[0-9 ]+$", RegexOptions.Compiled);


		// EVENTS:
		public event EventHandler? CancelRequested;
		public event EventHandler? SaveRequested;


		// FIELDS:
		private InitialConfiguration _configuration = new();
		private List<Table> _selectedTables = new();
		private string? _note;
		private int? _editingReservationId;


		// PROPERTIES:
		public bool IsEditMode { get; private set; }
		public int? EditingReservationId => _editingReservationId;
		public IReadOnlyList<Table> SelectedTables => _selectedTables;
		public string? Note => string.IsNullOrWhiteSpace(_note) ? null : _note.Trim();


		// CONSTRUCTORS:
		public AddEditReservationDialog(bool isEditMode = false)
		{
			InitializeComponent();
			SetMode(isEditMode);
		}


		// CONFIGURATION & DATA SETUP:
		public void ApplyConfiguration(InitialConfiguration configuration)
		{
			_configuration = configuration;

			FirstNamePanel.Visibility = configuration.FirstNameVisible ? Visibility.Visible : Visibility.Collapsed;
			LastNamePanel.Visibility = configuration.LastNameVisible ? Visibility.Visible : Visibility.Collapsed;
			PhonePanel.Visibility = configuration.PhoneNumberVisible ? Visibility.Visible : Visibility.Collapsed;

			FirstNameLabel.Text = configuration.FirstNameRequired ? "First Name *" : "First Name";
			LastNameLabel.Text = configuration.LastNameRequired ? "Last Name *" : "Last Name";
			PhoneLabel.Text = configuration.PhoneNumberRequired ? "Phone Number *" : "Phone Number";

			if (!configuration.FirstNameVisible && !configuration.LastNameVisible)
			{
				NameRowGrid.Visibility = Visibility.Collapsed;
			}
			else if (!configuration.FirstNameVisible)
			{
				NameRowGrid.Visibility = Visibility.Visible;
				FirstNameCol.Width = new GridLength(0);
				NameSpacerCol.Width = new GridLength(0);
				LastNameCol.Width = new GridLength(1, GridUnitType.Star);
			}
			else if (!configuration.LastNameVisible)
			{
				NameRowGrid.Visibility = Visibility.Visible;
				FirstNameCol.Width = new GridLength(1, GridUnitType.Star);
				NameSpacerCol.Width = new GridLength(0);
				LastNameCol.Width = new GridLength(0);
			}
			else
			{
				NameRowGrid.Visibility = Visibility.Visible;
				FirstNameCol.Width = new GridLength(1, GridUnitType.Star);
				NameSpacerCol.Width = new GridLength(16);
				LastNameCol.Width = new GridLength(1, GridUnitType.Star);
			}

			if (!configuration.PhoneNumberVisible)
			{
				PhoneCol.Width = new GridLength(0);
				PhoneGuestsSpacerCol.Width = new GridLength(0);
			}
			else
			{
				PhoneCol.Width = new GridLength(1, GridUnitType.Star);
				PhoneGuestsSpacerCol.Width = new GridLength(16);
			}
		}

		public void SetMode(bool isEditMode)
		{
			IsEditMode = isEditMode;

			ReservationDialog.Header = isEditMode ? "Edit Reservation" : "Add Reservation";
			SaveButton.Content = isEditMode ? "Save" : "Add";

			UpdateNoteButton();
			UpdateSelectTablesButtonState();
		}

		public void LoadReservation(Reservation reservation)
		{
			_editingReservationId = reservation.Id;

			DateBox.Text = reservation.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
			TimeBox.Text = reservation.Time.ToString("HH:mm", CultureInfo.InvariantCulture);
			FirstNameBox.Text = reservation.FirstName ?? string.Empty;
			LastNameBox.Text = reservation.LastName ?? string.Empty;
			PhoneBox.Text = reservation.PhoneNumber ?? string.Empty;
			GuestsBox.Text = reservation.Guests.ToString(CultureInfo.InvariantCulture);

			SetSelectedTables(reservation.Tables);
			SetNote(reservation.Note);
			UpdateSelectTablesButtonState();
		}

		public void SetSelectedTables(IEnumerable<Table> tables)
		{
			_selectedTables = tables.ToList();
			int totalSeats = _selectedTables.Sum(t => t.Seats);
			SelectedTablesText.Text = $"Selected tables: {_selectedTables.Count} | Total seats: {totalSeats}";

			ClearFieldError(TablesBorder, TablesErrorText);
			UpdateCapacityWarning();
		}

		public void SetNote(string? note)
		{
			_note = note;

			NoteText.Text = string.IsNullOrWhiteSpace(_note)
				? "No note added."
				: _note.Trim();

			UpdateNoteButton();
		}

		private void UpdateNoteButton()
		{
			AddNoteButton.Content = string.IsNullOrWhiteSpace(_note)
				? "Add Note"
				: "Edit Note";
		}

		private void UpdateSelectTablesButtonState()
		{
			bool isDateValid = TryParseDate(DateBox.Text, out _);
			bool isTimeValid = TryParseTime(TimeBox.Text, out _);
			SelectTablesButton.IsEnabled = isDateValid && isTimeValid;
		}

		private void UpdateCapacityWarning()
		{
			if (_selectedTables.Count > 0 && int.TryParse(GuestsBox.Text.Trim(), out int guests) && guests > 0)
			{
				int totalSeats = _selectedTables.Sum(t => t.Seats);
				CapacityWarningText.Visibility = guests > totalSeats ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				CapacityWarningText.Visibility = Visibility.Collapsed;
			}
		}


		// VALIDATION & MODEL BUILDING:
		public bool ValidateForm(out Reservation? reservation)
		{
			reservation = null;
			ClearAllErrors();

			Control? firstInvalidControl = null;

			void RegisterError(Control focusTarget, FrameworkElement borderElement, TextBlock errorText, string message)
			{
				SetFieldError(borderElement, errorText, message);
				firstInvalidControl ??= focusTarget;
			}

			string rawDate = DateBox.Text.Trim();
			bool hasValidDate = false;
			DateOnly parsedDate = default;

			if (string.IsNullOrEmpty(rawDate))
			{
				RegisterError(DateBox, DateBox, DateErrorText, "This field is required.");
			}
			else if (!TryParseDate(rawDate, out parsedDate))
			{
				RegisterError(DateBox, DateBox, DateErrorText, "Enter a valid date.");
			}
			else if (parsedDate < DateOnly.FromDateTime(DateTime.Now))
			{
				RegisterError(DateBox, DateBox, DateErrorText, "Date cannot be in the past.");
			}
			else
			{
				hasValidDate = true;
			}

			string rawTime = TimeBox.Text.Trim();
			bool hasValidTime = false;
			TimeOnly parsedTime = default;

			if (string.IsNullOrEmpty(rawTime))
			{
				RegisterError(TimeBox, TimeBox, TimeErrorText, "This field is required.");
			}
			else if (!TryParseTime(rawTime, out parsedTime))
			{
				RegisterError(TimeBox, TimeBox, TimeErrorText, "Enter a valid time.");
			}
			else
			{
				hasValidTime = true;
			}

			if (hasValidDate && hasValidTime)
			{
				DateTime now = DateTime.Now;
				DateOnly today = DateOnly.FromDateTime(now);
				TimeOnly currentMinute = new(now.Hour, now.Minute);

				if (parsedDate == today && parsedTime < currentMinute)
				{
					RegisterError(TimeBox, TimeBox, TimeErrorText, "Reservation time cannot be in the past.");
				}
			}

			string? normalizedFirstName = null;
			if (_configuration.FirstNameVisible)
			{
				string rawFirstName = FirstNameBox.Text.Trim();
				if (string.IsNullOrEmpty(rawFirstName))
				{
					if (_configuration.FirstNameRequired)
					{
						RegisterError(FirstNameBox, FirstNameBox, FirstNameErrorText, "This field is required.");
					}
				}
				else if (!FirstNameRegex.IsMatch(rawFirstName))
				{
					RegisterError(FirstNameBox, FirstNameBox, FirstNameErrorText, "Enter a valid first name.");
				}
				else
				{
					normalizedFirstName = rawFirstName;
				}
			}

			string? normalizedLastName = null;
			if (_configuration.LastNameVisible)
			{
				string rawLastName = LastNameBox.Text.Trim();
				if (string.IsNullOrEmpty(rawLastName))
				{
					if (_configuration.LastNameRequired)
					{
						RegisterError(LastNameBox, LastNameBox, LastNameErrorText, "This field is required.");
					}
				}
				else if (TwoLastNamesWithSpaceRegex.IsMatch(rawLastName))
				{
					RegisterError(LastNameBox, LastNameBox, LastNameErrorText, "Use a hyphen (-) between two last names.");
				}
				else if (!LastNameRegex.IsMatch(rawLastName))
				{
					RegisterError(LastNameBox, LastNameBox, LastNameErrorText, "Enter a valid last name.");
				}
				else
				{
					normalizedLastName = rawLastName;
				}
			}

			string? normalizedPhone = null;
			if (_configuration.PhoneNumberVisible)
			{
				string rawPhone = PhoneBox.Text.Trim();
				if (string.IsNullOrEmpty(rawPhone))
				{
					if (_configuration.PhoneNumberRequired)
					{
						RegisterError(PhoneBox, PhoneBox, PhoneErrorText, "This field is required.");
					}
				}
				else
				{
					string digitsOnly = rawPhone.Replace(" ", string.Empty);
					if (!PhoneCharsRegex.IsMatch(rawPhone) || digitsOnly.Length != 9)
					{
						RegisterError(PhoneBox, PhoneBox, PhoneErrorText, "Enter a valid 9-digit phone number. Spaces are allowed.");
					}
					else
					{
						normalizedPhone = digitsOnly;
					}
				}
			}

			string rawGuests = GuestsBox.Text.Trim();
			int parsedGuests = 0;
			if (string.IsNullOrEmpty(rawGuests))
			{
				RegisterError(GuestsBox, GuestsBorder, GuestsErrorText, "This field is required.");
			}
			else if (!int.TryParse(rawGuests, NumberStyles.None, CultureInfo.InvariantCulture, out parsedGuests) || parsedGuests <= 0)
			{
				RegisterError(GuestsBox, GuestsBorder, GuestsErrorText, "Enter a number greater than 0.");
			}

			if (_selectedTables.Count == 0)
			{
				RegisterError(SelectTablesButton, TablesBorder, TablesErrorText, "Select at least one table.");
			}

			if (firstInvalidControl is not null)
			{
				firstInvalidControl.Focus();
				return false;
			}

			reservation = new Reservation
			{
				Id = _editingReservationId ?? 0,
				Date = parsedDate,
				Time = parsedTime,
				FirstName = normalizedFirstName,
				LastName = normalizedLastName,
				PhoneNumber = normalizedPhone,
				Guests = parsedGuests,
				Note = Note,
				Tables = _selectedTables.ToList()
			};

			return true;
		}

		public void ApplyValidationErrors(IReadOnlyList<AppValidationError> errors)
		{
			ClearAllErrors();
			Control? firstInvalidControl = null;

			foreach (var error in errors)
			{
				switch (error.FieldName)
				{
					case nameof(Reservation.Date):
						SetFieldError(DateBox, DateErrorText, error.Message);
						firstInvalidControl ??= DateBox;
						break;
					case nameof(Reservation.Time):
						SetFieldError(TimeBox, TimeErrorText, error.Message);
						firstInvalidControl ??= TimeBox;
						break;
					case nameof(Reservation.FirstName):
						SetFieldError(FirstNameBox, FirstNameErrorText, error.Message);
						firstInvalidControl ??= FirstNameBox;
						break;
					case nameof(Reservation.LastName):
						SetFieldError(LastNameBox, LastNameErrorText, error.Message);
						firstInvalidControl ??= LastNameBox;
						break;
					case nameof(Reservation.PhoneNumber):
						SetFieldError(PhoneBox, PhoneErrorText, error.Message);
						firstInvalidControl ??= PhoneBox;
						break;
					case nameof(Reservation.Guests):
						SetFieldError(GuestsBorder, GuestsErrorText, error.Message);
						firstInvalidControl ??= GuestsBox;
						break;
					case nameof(Reservation.Tables):
						SetFieldError(TablesBorder, TablesErrorText, error.Message);
						firstInvalidControl ??= SelectTablesButton;
						break;
					default:
						MessageBox.Show(error.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
						break;
				}
			}

			firstInvalidControl?.Focus();
		}

		private void SetFieldError(FrameworkElement borderElement, TextBlock errorText, string message)
		{
			var dangerBrush = (Brush)FindResource("Danger");
			if (borderElement is Control control)
			{
				control.BorderBrush = dangerBrush;
			}
			else if (borderElement is Border border)
			{
				border.BorderBrush = dangerBrush;
			}

			errorText.Text = message;
			errorText.Visibility = Visibility.Visible;
		}

		private void ClearFieldError(FrameworkElement borderElement, TextBlock errorText)
		{
			var defaultBorderBrush = (Brush)FindResource("Border");
			if (borderElement is Control control)
			{
				control.BorderBrush = defaultBorderBrush;
			}
			else if (borderElement is Border border)
			{
				border.BorderBrush = defaultBorderBrush;
			}

			errorText.Text = string.Empty;
			errorText.Visibility = Visibility.Collapsed;
		}

		private void ClearAllErrors()
		{
			ClearFieldError(DateBox, DateErrorText);
			ClearFieldError(TimeBox, TimeErrorText);
			ClearFieldError(FirstNameBox, FirstNameErrorText);
			ClearFieldError(LastNameBox, LastNameErrorText);
			ClearFieldError(PhoneBox, PhoneErrorText);
			ClearFieldError(GuestsBorder, GuestsErrorText);
			ClearFieldError(TablesBorder, TablesErrorText);
		}

		private static bool TryParseDate(string input, out DateOnly date)
		{
			return DateOnly.TryParseExact(input.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
		}

		private static bool TryParseTime(string input, out TimeOnly time)
		{
			return TimeOnly.TryParseExact(input.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
		}


		// EVENT HANDLERS:
		private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				SaveRequested?.Invoke(this, EventArgs.Empty);
			}
		}

		private void DateOrTimeBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender == DateBox)
			{
				ClearFieldError(DateBox, DateErrorText);
			}
			else if (sender == TimeBox)
			{
				ClearFieldError(TimeBox, TimeErrorText);
			}

			UpdateSelectTablesButtonState();
		}

		private void FirstNameBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ClearFieldError(FirstNameBox, FirstNameErrorText);
		}

		private void LastNameBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ClearFieldError(LastNameBox, LastNameErrorText);
		}

		private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ClearFieldError(PhoneBox, PhoneErrorText);
		}

		private void GuestsBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ClearFieldError(GuestsBorder, GuestsErrorText);
			UpdateCapacityWarning();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			SaveRequested?.Invoke(this, EventArgs.Empty);
		}

		private async void SelectTables_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) { return; }

			if (!TryParseDate(DateBox.Text, out var parsedDate) || !TryParseTime(TimeBox.Text, out var parsedTime))
			{
				return;
			}

			var tablesDialog = new SelectTablesDialog();

			int guests = int.TryParse(GuestsBox.Text.Trim(), out int parsedGuests)
				? parsedGuests
				: 0;

			tablesDialog.SetReservationDetails(
				DateBox.Text.Trim(),
				TimeBox.Text.Trim(),
				guests
			);

			tablesDialog.CancelRequested += (_, _) =>
			{
				mainWindow.DialogHost.Close(tablesDialog, false);
			};

			tablesDialog.ConfirmRequested += (_, _) =>
			{
				SetSelectedTables(tablesDialog.SelectedTables);
				mainWindow.DialogHost.Close(tablesDialog, true);
			};

			mainWindow.DialogHost.Show(tablesDialog);

			await tablesDialog.LoadTablesAsync(
				parsedDate,
				parsedTime,
				guests,
				_selectedTables.Select(t => t.Id),
				_editingReservationId
			);
		}

		private void AddNote_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) { return; }

			bool hasNote = !string.IsNullOrWhiteSpace(_note);

			var noteDialog = new AddEditNoteDialog(
				isEditMode: hasNote,
				note: _note
			);

			noteDialog.CancelRequested += (_, _) =>
			{
				mainWindow.DialogHost.Close(noteDialog, false);
			};

			noteDialog.SaveRequested += (_, _) =>
			{
				SetNote(noteDialog.Note);
				mainWindow.DialogHost.Close(noteDialog, true);
			};

			mainWindow.DialogHost.Show(noteDialog);
		}


		// SPINNER METHODS:
		private void ChangeSpinnerValue(TextBox textBox, int change)
		{
			int value = int.TryParse(textBox.Text.Trim(), out int parsed) ? parsed : 0;
			textBox.Text = Math.Max(1, (long)value + change).ToString(CultureInfo.InvariantCulture);
		}


		// GUESTS EVENT HANDLERS:
		private void GuestsUp_Click(object sender, RoutedEventArgs e)
		{
			ChangeSpinnerValue(GuestsBox, 1);
		}

		private void GuestsDown_Click(object sender, RoutedEventArgs e)
		{
			ChangeSpinnerValue(GuestsBox, -1);
		}
	}
}