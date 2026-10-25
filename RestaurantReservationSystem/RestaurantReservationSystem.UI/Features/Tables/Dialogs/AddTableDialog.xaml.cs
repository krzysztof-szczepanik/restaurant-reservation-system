using System.Windows;

namespace RestaurantReservationSystem.UI.Features.Stoliki.Dialogs
{
    public partial class AddTabDial : Window
    {
        public AddTabDial()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Tu odbywać się będzie walidacja 
            // i wywołanie zapisu
            this.Close();
        }

        private void ZoneBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}