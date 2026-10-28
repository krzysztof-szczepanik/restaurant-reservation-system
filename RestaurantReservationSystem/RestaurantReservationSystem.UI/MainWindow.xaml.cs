using RestaurantReservationSystem.UI.Controls;
using RestaurantReservationSystem.UI.Features.InitialSetup;
using RestaurantReservationSystem.UI.Features.Reservations;
using RestaurantReservationSystem.UI.Features.Settings;
using RestaurantReservationSystem.UI.Features.Tables;
using RestaurantReservationSystem.UI.Features.Today;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RestaurantReservationSystem.UI {
    public partial class MainWindow : Window {

        // CONSTANTS:
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x00000001;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private const int TITLE_BAR_COLOR = 0x00EFE7E0;

        // PROPERTIES:
        public DialogHost DialogHost => MainDialogHost;

        // CONSTRUCTORS:
        public MainWindow() {
            InitializeComponent();

            SourceInitialized += (_, _) => ConfigureTitleBar();

            // DEFAULT VIEW
            NavigateTo("Today");

            // TEMPORARY INITIAL SETUP TEST
            Loaded += MainWindow_Loaded;
        }

        // WINDOW CONFIGURATION:
        private void ConfigureTitleBar() {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            long style = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();

            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(style | WS_EX_DLGMODALFRAME));
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);

            int captionColor = TITLE_BAR_COLOR;
            DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));
        }

        // NAVIGATION:
        private void NavigateTo(string destination) {
            switch (destination) {
                case "Today":
                    MainContentArea.Content = new TodayView();
                    break;

                case "Reservations":
                    MainContentArea.Content = new ReservationsView();
                    break;

                case "Tables":
                    MainContentArea.Content = new TablesView();
                    break;

                case "Settings":
                    MainContentArea.Content = new SettingsView();
                    break;
            }
        }

        // EVENT HANDLERS:
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Loaded -= MainWindow_Loaded;

            var dialog = new InitialSetupDialog();

            dialog.SaveRequested += (_, _) => MainDialogHost.CloseAll();

            MainDialogHost.Show(dialog);
        }

        private void Sidebar_NavigationRequested(object sender, string destination) {
            NavigateTo(destination);
        }

        // NATIVE METHODS:
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(
            IntPtr hwnd, int index);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(
            IntPtr hwnd, int index, IntPtr newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hwnd, IntPtr insertAfter, int x, int y,
            int width, int height, uint flags);
    }
}