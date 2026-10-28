using RestaurantReservationSystem.UI.Features.Reservations.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantReservationSystem.UI.Features.Reservations {
    public partial class ReservationsView : UserControl {

        // PROPERTIES:
        public bool HasActiveFilters {
            get => FilterButton.Tag is true;
            set => FilterButton.Tag = value;
        }


        // CONSTRUCTORS:
        public ReservationsView() {
            InitializeComponent();
        }


        // METHODS:
        private MainWindow? GetMainWindow() {
            return Window.GetWindow(this) as MainWindow;
        }


        // EVENT HANDLERS:
        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            if (!SearchBox.IsKeyboardFocusWithin) { return; }

            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(SearchBox), null);
            Keyboard.ClearFocus();
        }

        private void AddReservations_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) { return; }

            var dialog = new AddEditReservationDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

            dialog.SaveRequested += (_, _) => {
                // TODO: Add reservation.
                mainWindow.DialogHost.Close(dialog, true);
            };

            mainWindow.DialogHost.Show(dialog);
        }

        private void EditReservations_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) { return; }

            var dialog = new AddEditReservationDialog(isEditMode: true);

            // TODO: Load selected reservation data.
            // dialog.SetReservationData(date, time, firstName, lastName, phoneNumber, guests);
            // dialog.SetTablesSummary(selectedTables, totalSeats);
            // dialog.SetNote(note);

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

            dialog.SaveRequested += (_, _) => {
                // TODO: Update reservation.
                mainWindow.DialogHost.Close(dialog, true);
            };

            mainWindow.DialogHost.Show(dialog);
        }

        private void DeleteReservations_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) { return; }

            var dialog = new DeleteReservationsDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

            dialog.DeleteRequested += (_, _) => {
                // TODO: Delete reservation.
                mainWindow.DialogHost.Close(dialog, true);
            };

            mainWindow.DialogHost.Show(dialog);
        }

        private void Filter_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) { return; }

            var dialog = new FilterReservationsDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

            dialog.ApplyRequested += (_, _) => {
                // TODO: Read filters and update HasActiveFilters.
                mainWindow.DialogHost.Close(dialog, true);
            };

            mainWindow.DialogHost.Show(dialog);
        }
    }
}