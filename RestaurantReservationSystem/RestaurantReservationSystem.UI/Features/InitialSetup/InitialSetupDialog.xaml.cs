using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.InitialSetup.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.InitialSetup {
    public partial class InitialSetupDialog : UserControl {

        // FIELDS:
        private bool _confirmationOpen;


        // EVENTS:
        public event EventHandler? SaveRequested;


        // CONSTRUCTORS:
        public InitialSetupDialog() {
            InitializeComponent();
        }


        // METHODS:
        public InitialConfiguration GetConfiguration() {
            return new InitialConfiguration {
                FirstNameVisible = FirstNameVisibleBox.IsChecked == true,
                FirstNameRequired = FirstNameVisibleBox.IsChecked == true && FirstNameRequiredBox.IsChecked == true,

                LastNameVisible = LastNameVisibleBox.IsChecked == true,
                LastNameRequired = LastNameVisibleBox.IsChecked == true && LastNameRequiredBox.IsChecked == true,

                PhoneNumberVisible = PhoneVisibleBox.IsChecked == true,
                PhoneNumberRequired = PhoneVisibleBox.IsChecked == true && PhoneRequiredBox.IsChecked == true,

                TableNumberVisible = NumberVisibleBox.IsChecked == true,
                TableNumberRequired = NumberVisibleBox.IsChecked == true && NumberRequiredBox.IsChecked == true,

                TableLabelVisible = LabelVisibleBox.IsChecked == true,
                TableLabelRequired = LabelVisibleBox.IsChecked == true && LabelRequiredBox.IsChecked == true,

                TableZoneVisible = ZoneVisibleBox.IsChecked == true,

                TableIdentifierOrder = LabelFirstRadio.IsChecked == true ? TableIdentifierOrder.LabelThenNumber : TableIdentifierOrder.NumberThenLabel
            };
        }

        private bool ValidateForm() {

            // Customer fields:
            bool reservationValid =
                (FirstNameVisibleBox.IsChecked == true && FirstNameRequiredBox.IsChecked == true) ||
                (LastNameVisibleBox.IsChecked == true && LastNameRequiredBox.IsChecked == true) ||
                (PhoneVisibleBox.IsChecked == true && PhoneRequiredBox.IsChecked == true);

            // Table identifier fields:
            bool tableValid = (NumberVisibleBox.IsChecked == true && NumberRequiredBox.IsChecked == true) || (LabelVisibleBox.IsChecked == true && LabelRequiredBox.IsChecked == true);
            ReservationValidationText.Visibility = reservationValid ? Visibility.Collapsed : Visibility.Visible;
            TableValidationText.Visibility = tableValid ? Visibility.Collapsed : Visibility.Visible;

            return reservationValid && tableValid;
        }


        // EVENT HANDLERS:
        private void Submit_Click(object sender, RoutedEventArgs e) {
            if (_confirmationOpen || !ValidateForm()) {
                return;
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow is null) {
                return;
            }

            _confirmationOpen = true;

            var confirmDialog = new ConfirmInitialSetupDialog();

            confirmDialog.CancelRequested += (_, _) => {
                _confirmationOpen = false;
                mainWindow.DialogHost.Close(false);
            };

            confirmDialog.ConfirmRequested += (_, _) => {
                _confirmationOpen = false;
                SaveRequested?.Invoke(this, EventArgs.Empty);
            };

            mainWindow.DialogHost.Show(confirmDialog);
        }
    }
}