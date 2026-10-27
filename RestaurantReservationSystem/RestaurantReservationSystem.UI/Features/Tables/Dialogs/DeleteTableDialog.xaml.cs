using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Stoliki.Dialogs
{
    public partial class DeleteTabDial : Window
    {
        public DeleteTabDial()
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