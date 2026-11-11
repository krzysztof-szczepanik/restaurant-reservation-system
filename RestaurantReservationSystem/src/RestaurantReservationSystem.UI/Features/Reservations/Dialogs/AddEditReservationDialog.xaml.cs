using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs {
    public partial class AddEditReservationDialog : UserControl {

        // EVENTS:
        public event EventHandler? CancelRequested;
        public event EventHandler? SaveRequested;


        // FIELDS:
        private string? _note;


        // PROPERTIES:
        public bool IsEditMode { get; private set; }

        public string ReservationDate => DateBox.Text;
        public string ReservationTime => TimeBox.Text;
        public string FirstName => FirstNameBox.Text;
        public string LastName => LastNameBox.Text;
        public string PhoneNumber => PhoneBox.Text;
        public string Guests => GuestsBox.Text;
        public string? Note => _note;


        // CONSTRUCTORS:
        public AddEditReservationDialog(bool isEditMode = false) {
            InitializeComponent();
            SetMode(isEditMode);
        }


        // METHODS:
        public void SetMode(bool isEditMode) {
            IsEditMode = isEditMode;

            ReservationDialog.Header = isEditMode ? "Edit Reservation" : "Add Reservation";
            SaveButton.Content = isEditMode ? "Save" : "Add";

            UpdateNoteButton();
        }

        public void SetReservationData(string date, string time, string firstName, string lastName, string phoneNumber, string guests) {
            DateBox.Text = date;
            TimeBox.Text = time;
            FirstNameBox.Text = firstName;
            LastNameBox.Text = lastName;
            PhoneBox.Text = phoneNumber;
            GuestsBox.Text = guests;
        }

        public void SetTablesSummary(int selectedTables, int totalSeats) {
            SelectedTablesText.Text = $"Selected tables: {selectedTables} | Total seats: {totalSeats}";
        }

        public void SetNote(string? note) {
            _note = note;

            NoteText.Text = string.IsNullOrWhiteSpace(_note)
                ? "No note added."
                : _note;

            UpdateNoteButton();
        }

        private void UpdateNoteButton() {
            NoteButton.Content = string.IsNullOrWhiteSpace(_note)
                ? "Add Note"
                : "Edit Note";
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SelectTables_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow == null) { return; }

            var tablesDialog = new SelectTablesDialog();

            int guests = int.TryParse(GuestsBox.Text, out int parsedGuests)
                ? parsedGuests
                : 0;

            tablesDialog.SetReservationDetails(
                DateBox.Text,
                TimeBox.Text,
                guests
            );

            tablesDialog.CancelRequested += (_, _) => {
                mainWindow.DialogHost.Close(tablesDialog, false);
            };

            tablesDialog.ConfirmRequested += (_, _) => {
                SetTablesSummary(
                    tablesDialog.SelectedTablesCount,
                    tablesDialog.TotalSeats
                );

                mainWindow.DialogHost.Close(tablesDialog, true);
            };

            mainWindow.DialogHost.Show(tablesDialog);
        }

        private void Note_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow == null) { return; }

            bool hasNote = !string.IsNullOrWhiteSpace(_note);

            var noteDialog = new AddEditNoteDialog(
                isEditMode: hasNote,
                note: _note
            );

            noteDialog.CancelRequested += (_, _) => {
                mainWindow.DialogHost.Close(noteDialog, false);
            };

            noteDialog.SaveRequested += (_, _) => {
                SetNote(noteDialog.Note);
                mainWindow.DialogHost.Close(noteDialog, true);
            };

            mainWindow.DialogHost.Show(noteDialog);
        }


        // SPINNER METHODS:
        private void ChangeSpinnerValue(TextBox textBox, int change) {
            int value = int.TryParse(textBox.Text, out int parsed) ? parsed : 0;
            textBox.Text = Math.Max(1, (long)value + change).ToString();
        }


        // GUESTS EVENT HANDLERS:
        private void GuestsUp_Click(object sender, RoutedEventArgs e) {
            ChangeSpinnerValue(GuestsBox, 1);
        }

        private void GuestsDown_Click(object sender, RoutedEventArgs e) {
            ChangeSpinnerValue(GuestsBox, -1);
        }
    }
}