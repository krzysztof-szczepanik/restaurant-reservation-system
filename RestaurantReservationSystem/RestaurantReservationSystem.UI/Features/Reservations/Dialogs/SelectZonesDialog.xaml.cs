using System;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs {
    public partial class SelectZonesDialog : UserControl {

        // EVENTS:
        public event EventHandler? CancelRequested;
        public event EventHandler? ConfirmRequested;


        // CONSTRUCTORS:
        public SelectZonesDialog() {
            InitializeComponent();
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) {
            ConfirmRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}