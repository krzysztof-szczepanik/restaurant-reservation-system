using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Settings.Dialogs {
    public partial class ApplicationResetDialog : UserControl {

        // EVENTS:
        public event EventHandler? CancelRequested;
        public event EventHandler? ResetRequested;


        // CONSTRUCTORS:
        public ApplicationResetDialog() {
            InitializeComponent();
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
            ResetRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}