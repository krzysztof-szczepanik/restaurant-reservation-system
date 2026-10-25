using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Ustawienia.Dialogs
{
    public partial class AppResDial : Window
    {
        public AppResDial()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Tutaj będzie implementacja wywołania czyszczenia bazy
            // i przeniesienie do InitialSetupView
            this.Close();
        }
    }
}