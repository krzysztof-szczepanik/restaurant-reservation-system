using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs {
    public partial class DeleteTablesDialog : UserControl {

        // EVENTS:
        public event EventHandler? DeleteRequested;
        public event EventHandler? CancelRequested;


        // CONSTRUCTORS:
        public DeleteTablesDialog() {
            InitializeComponent();
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}