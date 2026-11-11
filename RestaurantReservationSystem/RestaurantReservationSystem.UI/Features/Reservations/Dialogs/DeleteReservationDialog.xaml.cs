using System;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs {
    public partial class DeleteReservationsDialog : UserControl {

        // EVENTS:
        public event EventHandler? CancelRequested;
        public event EventHandler? DeleteRequested;


        // CONSTRUCTORS:
        public DeleteReservationsDialog() {
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