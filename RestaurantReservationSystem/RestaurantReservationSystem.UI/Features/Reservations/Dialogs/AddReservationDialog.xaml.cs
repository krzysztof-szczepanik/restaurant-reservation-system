using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
    public partial class AddResDial : Window
    {
        public AddResDial()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Tutaj zostanie dodane wywołanie walidacji
            this.Close();
        }
    }
}