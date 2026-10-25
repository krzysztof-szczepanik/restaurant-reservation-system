using System.Windows;
using System.Windows.Controls;
using RestaurantReservationSystem.UI.Features.Stoliki.Dialogs;

namespace RestaurantReservationSystem.UI.Features.Stoliki
{
    public partial class TabView : UserControl
    {
        public TabView()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTabDial();
            dialog.Owner = Window.GetWindow(this); 
            dialog.ShowDialog();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditTabDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void DeleteTab_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DeleteTabDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
    }
}