using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs {
    public partial class CannotDeleteTableDialog : UserControl {

        // EVENTS:
        public event EventHandler? CloseRequested;


        // CONSTRUCTORS:
        public CannotDeleteTableDialog() {
            InitializeComponent();
        }


        // EVENT HANDLERS:
        private void Ok_Click(object sender, RoutedEventArgs e) {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}