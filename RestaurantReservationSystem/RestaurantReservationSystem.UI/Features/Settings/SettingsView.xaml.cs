using System.Windows;
using System.Windows.Controls;
using RestaurantReservationSystem.UI.Features.Ustawienia.Dialogs;

namespace RestaurantReservationSystem.UI.Features.Ustawienia
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AppResDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
    }
}