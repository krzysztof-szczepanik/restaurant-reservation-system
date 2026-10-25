using System.Windows;
using RestaurantReservationSystem.UI.Features.Today;
using RestaurantReservationSystem.UI.Features.Reservations;
using RestaurantReservationSystem.UI.Features.Stoliki;
using RestaurantReservationSystem.UI.Features.Ustawienia;

namespace RestaurantReservationSystem.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Menu_Today_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentArea != null)
            {
                MainContentArea.Content = new TodayView();
            }
        }

        private void Menu_Reservations_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentArea != null)
            {
                MainContentArea.Content = new ResView();
            }
        }

        private void Menu_Stoliki_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentArea != null)
            {
                MainContentArea.Content = new TabView();
            }
        }

        private void Menu_Ustawienia_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentArea != null)
            {
                MainContentArea.Content = new SettingsView();
            }
        }
    }
}