using RestaurantReservationSystem.UI.Features.Tables.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Tables {
    public partial class TablesView : UserControl {

        // CONSTRUCTORS:
        public TablesView() {
            InitializeComponent();
        }


        // METHODS:
        private MainWindow? GetMainWindow() {
            return Window.GetWindow(this) as MainWindow;
        }


        // EVENT HANDLERS:
        private void Add_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) {
                return;
            }

            var dialog = new AddEditTableDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            dialog.SaveRequested += (_, _) => {
                // TODO: Dodawanie stolika.
                mainWindow.DialogHost.Close(true);
            };

            mainWindow.DialogHost.Show(dialog);
        }

        private void Edit_Click(object sender, RoutedEventArgs e) {

            // Tymczasowo zakomentowane do testów:
            //if (TablesDataGrid.SelectedItem == null) {
            //    return;
            //}

            var mainWindow = GetMainWindow();

            if (mainWindow == null) {
                return;
            }

            // TODO: Pobranie danych zaznaczonego stolika.
            var dialog = new AddEditTableDialog("5", "E", "Terrace", "4");

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            dialog.SaveRequested += (_, _) => {
                // TODO: Aktualizacja stolika.
                mainWindow.DialogHost.Close(true);
            };

            mainWindow.DialogHost.Show(dialog);
        }

        private void DeleteTab_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) {
                return;
            }

            var dialog = new DeleteTablesDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            dialog.DeleteRequested += (_, _) => {
                // TODO: Usuwanie stolika.
                mainWindow.DialogHost.Close(true);
            };

            mainWindow.DialogHost.Show(dialog);
        }
    }
}