using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Controls {
    public partial class DataTable : DataGrid {

        // PROPERTIES:
        public static readonly DependencyProperty EmptyMessageProperty = DependencyProperty.Register(nameof(EmptyMessage), typeof(string), typeof(DataTable), new PropertyMetadata("No data available."));
        public string EmptyMessage { get => (string)GetValue(EmptyMessageProperty); set => SetValue(EmptyMessageProperty, value); }


        // CONSTRUCTORS:
        public DataTable() {
            InitializeComponent();
            Loaded += DataTable_Loaded;
            SizeChanged += DataTable_SizeChanged;
        }


        // METHODS:
        private void DataTable_Loaded(object sender, RoutedEventArgs e) {
            UpdateClip();
        }

        private void DataTable_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateClip();
        }

        private void UpdateClip() {
            Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight), 6, 6);
        }
    }
}