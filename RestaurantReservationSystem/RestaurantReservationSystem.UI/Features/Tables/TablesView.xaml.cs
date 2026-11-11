using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.UI.Features.Tables.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Tables {
    public partial class TablesView : UserControl {

        // CONSTRUCTORS:
        public TablesView() {
            InitializeComponent();

            Loaded += TablesView_Loaded;
        }


        // METHODS:
        private MainWindow? GetMainWindow() {
            return Window.GetWindow(this) as MainWindow;
        }

        private async Task LoadConfigurationAsync() {
            using var scope = App.ServiceProvider.CreateScope();

            var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            var configuration = await configurationService.GetAsync();

            if (configuration is null) {
                throw new InvalidOperationException("Initial configuration does not exist.");
            }

            TablesDataGrid.Columns[0].Visibility = configuration.TableNumberVisible ? Visibility.Visible : Visibility.Collapsed;
            TablesDataGrid.Columns[1].Visibility = configuration.TableLabelVisible ? Visibility.Visible : Visibility.Collapsed;
            TablesDataGrid.Columns[2].Visibility = configuration.TableZoneVisible ? Visibility.Visible : Visibility.Collapsed;

            if (configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber) {
                TablesDataGrid.Columns[0].DisplayIndex = 1;
                TablesDataGrid.Columns[1].DisplayIndex = 0;
            } else {
                TablesDataGrid.Columns[0].DisplayIndex = 0;
                TablesDataGrid.Columns[1].DisplayIndex = 1;
            }
        }


        // EVENT HANDLERS:
        private async void TablesView_Loaded(object sender, RoutedEventArgs e) {
            Loaded -= TablesView_Loaded;

            try {
                await LoadConfigurationAsync();
            } catch (Exception ex) {
                MessageBox.Show(
                    $"Failed to load table configuration.\n\n{ex.Message}",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            var mainWindow = GetMainWindow();

            if (mainWindow == null) {
                return;
            }

            var dialog = new AddEditTableDialog();

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            dialog.SaveRequested += (_, _) => {
                // TODO: Add table.
                mainWindow.DialogHost.Close(true);
            };

            mainWindow.DialogHost.Show(dialog);
        }

        private void Edit_Click(object sender, RoutedEventArgs e) {

            // Temporarily disabled for testing:
            // if (TablesDataGrid.SelectedItem == null) {
            //     return;
            // }

            var mainWindow = GetMainWindow();

            if (mainWindow == null) {
                return;
            }

            // TODO: Load selected table data.
            var dialog = new AddEditTableDialog("5", "E", "Terrace", "4");

            dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(false);

            dialog.SaveRequested += (_, _) => {
                // TODO: Update table.
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
                // TODO: Delete table.
                mainWindow.DialogHost.Close(true);
            };

            mainWindow.DialogHost.Show(dialog);
        }
    }
}