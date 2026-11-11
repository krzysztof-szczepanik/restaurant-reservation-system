using System;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs {
    public partial class FilterReservationsDialog : UserControl {

        // EVENTS:
        public event EventHandler? CancelRequested;
        public event EventHandler? ApplyRequested;


        // CONSTRUCTORS:
        public FilterReservationsDialog() {
            InitializeComponent();
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Apply_Click(object sender, RoutedEventArgs e) {
            ApplyRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
        }

        private void Clean_Click(object sender, RoutedEventArgs e) {
        }

        private void SelectZones_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) { return; }

            var selectZonesDialog = new SelectZonesDialog();
            selectZonesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(selectZonesDialog, false);
            selectZonesDialog.ConfirmRequested += (_, _) => mainWindow.DialogHost.Close(selectZonesDialog, true);
            mainWindow.DialogHost.Show(selectZonesDialog);
        }

        private void SelectTables_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) { return; }

            var selectTablesDialog = new SelectTablesDialog();
            selectTablesDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(selectTablesDialog, false);
            selectTablesDialog.ConfirmRequested += (_, _) => mainWindow.DialogHost.Close(selectTablesDialog, true);
            mainWindow.DialogHost.Show(selectTablesDialog);
        }
    }
}