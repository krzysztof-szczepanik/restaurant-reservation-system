using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs {
    public partial class AddEditNoteDialog : UserControl {

        // EVENTS:
        public event EventHandler? CancelRequested;
        public event EventHandler? SaveRequested;


        // PROPERTIES:
        public bool IsEditMode { get; private set; }

        public string Note => NoteBox.Text;


        // CONSTRUCTORS:
        public AddEditNoteDialog(bool isEditMode = false, string? note = null) {
            InitializeComponent();

            SetMode(isEditMode);
            SetNote(note);
        }


        // METHODS:
        public void SetMode(bool isEditMode) {
            IsEditMode = isEditMode;

            NoteDialog.Header = isEditMode
                ? "Edit Note"
                : "Add Note";
        }

        public void SetNote(string? note) {
            NoteBox.Text = note ?? string.Empty;
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}