using System.Windows;
using System.Windows.Controls;
using RestaurantReservationSystem.UI.Features.Reservations.Dialogs;

namespace RestaurantReservationSystem.UI.Features.Reservations
{
    public partial class ResView : UserControl
    {
        public ResView()
        {
            InitializeComponent();
        }

        private void AddRes_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddResDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void EditRes_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditResDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void DeleteRes_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DeleteResDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FilterResDial();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
    }
}