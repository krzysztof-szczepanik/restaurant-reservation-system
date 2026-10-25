using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
    public partial class DeleteResDial : Window
    {
        public DeleteResDial()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}