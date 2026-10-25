using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
    public partial class EditResDial : Window
    {
        public EditResDial()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}