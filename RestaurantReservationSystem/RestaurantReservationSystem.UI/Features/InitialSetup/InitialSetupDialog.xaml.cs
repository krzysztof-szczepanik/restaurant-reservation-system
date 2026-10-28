using RestaurantReservationSystem.UI.Features.InitialSetup.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.InitialSetup {
    public partial class InitialSetupDialog : UserControl {

        // EVENTS:
        public event EventHandler? SaveRequested;


        // CONSTRUCTORS:
        public InitialSetupDialog() {
            InitializeComponent();
        }


        // METHODS:
        private bool ValidateForm() {
            bool reservationValid =
                (FirstNameVisibleBox.IsChecked == true && FirstNameRequiredBox.IsChecked == true) ||
                (LastNameVisibleBox.IsChecked == true && LastNameRequiredBox.IsChecked == true);

            bool tableValid =
                (NumberVisibleBox.IsChecked == true && NumberRequiredBox.IsChecked == true) ||
                (LabelVisibleBox.IsChecked == true && LabelRequiredBox.IsChecked == true);

            ReservationValidationText.Visibility = reservationValid ? Visibility.Collapsed : Visibility.Visible;
            TableValidationText.Visibility = tableValid ? Visibility.Collapsed : Visibility.Visible;

            return reservationValid && tableValid;
        }


        // EVENT HANDLERS:
        private void Submit_Click(object sender, RoutedEventArgs e) {
            if (!ValidateForm()) {
                return;
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow == null) {
                return;
            }

            var confirmDialog = new ConfirmInitialSetupDialog();

            confirmDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            confirmDialog.ConfirmRequested += (_, _) => {
                // TODO: Save initial setup configuration.
                SaveRequested?.Invoke(this, EventArgs.Empty);
            };

            mainWindow.DialogHost.Show(confirmDialog);
        }
    }
}