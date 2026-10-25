using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Stoliki.Dialogs
{
    public partial class EditTabDial : Window
    {
        public EditTabDial()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            // Zapisanie zmian w edytowanym stoliku
            this.Close();
        }
    }
}