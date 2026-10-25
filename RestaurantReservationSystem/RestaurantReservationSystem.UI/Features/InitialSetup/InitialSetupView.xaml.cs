using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.InitialSetup
{
    public partial class InitialSetupView : UserControl
    {
        public InitialSetupView()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            // Tutaj będą dane restauracji do bazy
            // Ten widok zostanie zamknięty i
            // pokazany zostanie MainWindow
            MessageBox.Show("Profil Twojej restauracji został pomyślnie utworzony.", "Konfiguracja zakończona", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}