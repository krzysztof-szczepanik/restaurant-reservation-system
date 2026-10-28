using RestaurantReservationSystem.UI.Features.Settings.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Settings {
    public partial class SettingsView : UserControl {

        // CONSTRUCTORS:
        public SettingsView() {
            InitializeComponent();
        }


        // EVENT HANDLERS:
        private void Reset_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow == null) {
                return;
            }

            var dialog = new ApplicationResetDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            dialog.ResetRequested += (_, _) => {
                // TODO: Reset application data and initial configuration.
                mainWindow.DialogHost.Close(true);
            };

            mainWindow.DialogHost.Show(dialog);
        }
    }
}