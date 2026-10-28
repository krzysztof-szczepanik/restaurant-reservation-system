using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Controls.Sidebar {
    public partial class Sidebar : UserControl {

        // EVENTS:
        public event EventHandler<string>? NavigationRequested;


        // CONSTRUCTORS:
        public Sidebar() {
            InitializeComponent();
        }


        // METHODS:
        private void RequestNavigation(string destination) {
            NavigationRequested?.Invoke(this, destination);
        }


        // EVENT HANDLERS:
        private void Today_Checked(object sender, RoutedEventArgs e) {
            RequestNavigation("Today");
        }

        private void Reservations_Checked(object sender, RoutedEventArgs e) {
            RequestNavigation("Reservations");
        }

        private void Tables_Checked(object sender, RoutedEventArgs e) {
            RequestNavigation("Tables");
        }

        private void Settings_Checked(object sender, RoutedEventArgs e) {
            RequestNavigation("Settings");
        }
    }
}