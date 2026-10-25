using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
    public partial class FilterResDial : Window
    {
        public FilterResDial()
        {
            InitializeComponent();
        }

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}