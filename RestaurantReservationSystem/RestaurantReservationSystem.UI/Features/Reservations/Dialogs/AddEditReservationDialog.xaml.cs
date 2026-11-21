using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class AddEditReservationDialog : UserControl
	{

		public event EventHandler? CancelRequested;
		public event EventHandler? SaveRequested;

		private string? _note;
		private int _selectedTablesCount = 0;
		private int _totalSeats = 0;

		public bool IsEditMode { get; private set; }

		public string ReservationDate => DateBox.Text.Trim();
		public string ReservationTime => TimeBox.Text.Trim();
		public string FirstName => FirstNameBox.Text.Trim();
		public string LastName => LastNameBox.Text.Trim();
		public string PhoneNumber => PhoneBox.Text.Trim();
		public string Guests => GuestsBox.Text.Trim();
		public string? Note => _note;

		public AddEditReservationDialog(bool isEditMode = false)
		{
			InitializeComponent();
			SetMode(isEditMode);
		}

		public void SetMode(bool isEditMode)
		{
			IsEditMode = isEditMode;
			ReservationDialog.Header = isEditMode ? "Edit Reservation" : "Add Reservation";
			SaveButton.Content = isEditMode ? "Save" : "Add";
			UpdateNoteButton();
		}

		public void SetReservationData(string date, string time, string firstName, string lastName, string phoneNumber, string guests)
		{
			DateBox.Text = date;
			TimeBox.Text = time;
			FirstNameBox.Text = firstName;
			LastNameBox.Text = lastName;
			PhoneBox.Text = phoneNumber;
			GuestsBox.Text = guests;
		}

		public void SetTablesSummary(int selectedTables, int totalSeats)
		{
			_selectedTablesCount = selectedTables;
			_totalSeats = totalSeats;
			SelectedTablesText.Text = $"Selected tables: {selectedTables} | Total seats: {totalSeats}";
		}

		public void SetNote(string? note)
		{
			_note = note;
			NoteText.Text = string.IsNullOrWhiteSpace(_note) ? "No note added." : _note;
			UpdateNoteButton();
		}

		private void UpdateNoteButton()
		{
			NoteButton.Content = string.IsNullOrWhiteSpace(_note) ? "Add Note" : "Edit Note";
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (ValidateForm())
			{
				SaveRequested?.Invoke(this, EventArgs.Empty);
			}
		}

		private bool ValidateForm()
		{
			bool isValid = true;
			var dangerBrush = (Brush)FindResource("Danger");
			var defaultBorderBrush = (Brush)FindResource("Border");

			if (string.IsNullOrWhiteSpace(ReservationDate))
			{
				ShowError(DateBox, DateErrorText, "This field is required.");
				isValid = false;
			}
			else if (!DateTime.TryParse(ReservationDate, out _))
			{
				ShowError(DateBox, DateErrorText, "Enter a valid date. Use dd.MM.yyyy.");
				isValid = false;
			}
			else
			{
				ClearError(DateBox, DateErrorText, defaultBorderBrush);
			}

			if (string.IsNullOrWhiteSpace(ReservationTime))
			{
				ShowError(TimeBox, TimeErrorText, "This field is required.");
				isValid = false;
			}
			else if (!TimeSpan.TryParse(ReservationTime, out _))
			{
				ShowError(TimeBox, TimeErrorText, "Enter a valid time.");
				isValid = false;
			}
			else
			{
				ClearError(TimeBox, TimeErrorText, defaultBorderBrush);
			}

			if (string.IsNullOrWhiteSpace(LastName))
			{
				ShowError(LastNameBox, LastNameErrorText, "This field is required.");
				isValid = false;
			}
			else
			{
				ClearError(LastNameBox, LastNameErrorText, defaultBorderBrush);
			}

			if (string.IsNullOrWhiteSpace(Guests) || !int.TryParse(Guests, out int guestCount) || guestCount <= 0)
			{
				GuestsBorder.BorderBrush = dangerBrush;
				GuestsErrorText.Text = "Enter a number greater than 0.";
				GuestsErrorText.Visibility = Visibility.Visible;
				isValid = false;
			}
			else
			{
				GuestsBorder.BorderBrush = defaultBorderBrush;
				GuestsErrorText.Visibility = Visibility.Collapsed;
			}

			if (_selectedTablesCount <= 0)
			{
				TablesBorder.BorderBrush = dangerBrush;
				TablesErrorText.Text = "Select at least one table.";
				TablesErrorText.Visibility = Visibility.Visible;
				isValid = false;
			}
			else
			{
				TablesBorder.BorderBrush = defaultBorderBrush;
				TablesErrorText.Visibility = Visibility.Collapsed;
			}

			return isValid;
		}

		private void ShowError(TextBox textBox, TextBlock errorText, string message)
		{
			textBox.BorderBrush = (Brush)FindResource("Danger");
			errorText.Text = message;
			errorText.Visibility = Visibility.Visible;
		}

		private void ClearError(TextBox textBox, TextBlock errorText, Brush defaultBorder)
		{
			textBox.BorderBrush = defaultBorder;
			errorText.Visibility = Visibility.Collapsed;
		}

		private void SelectTables_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) return;

			var tablesDialog = new SelectTablesDialog();
			int guests = int.TryParse(GuestsBox.Text, out int parsedGuests) ? parsedGuests : 0;

			tablesDialog.SetReservationDetails(DateBox.Text, TimeBox.Text, guests);

			tablesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(tablesDialog, false);
			tablesDialog.ConfirmRequested += (_, _) => {
				SetTablesSummary(tablesDialog.SelectedTablesCount, tablesDialog.TotalSeats);
				mainWindow.DialogHost.Close(tablesDialog, true);
			};

			mainWindow.DialogHost.Show(tablesDialog);
		}

		private void Note_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) return;

			bool hasNote = !string.IsNullOrWhiteSpace(_note);
			var noteDialog = new AddEditNoteDialog(isEditMode: hasNote, note: _note);

			noteDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(noteDialog, false);
			noteDialog.SaveRequested += (_, _) => {
				SetNote(noteDialog.Note);
				mainWindow.DialogHost.Close(noteDialog, true);
			};

			mainWindow.DialogHost.Show(noteDialog);
		}

		private void ChangeSpinnerValue(TextBox textBox, int change)
		{
			int value = int.TryParse(textBox.Text, out int parsed) ? parsed : 0;
			textBox.Text = Math.Max(1, (long)value + change).ToString();
		}

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