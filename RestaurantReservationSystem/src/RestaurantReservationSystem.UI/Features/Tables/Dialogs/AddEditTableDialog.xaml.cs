using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs {
    public partial class AddEditTableDialog : UserControl {

        // EVENTS:
        public event EventHandler? SaveRequested;
        public event EventHandler? CancelRequested;


        // PROPERTIES:
        public bool IsEditMode { get; }

        public string TableNumber => NumberBox.Text;
        public string TableLabel => LabelBox.Text;
        public string TableZone => (ZoneBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No zone";
        public string TableSeats => SeatsBox.Text;


        // CONSTRUCTORS:
        public AddEditTableDialog() {
            InitializeComponent();
            IsEditMode = false;
            ConfigureDialog();
        }

        public AddEditTableDialog(string number, string label, string zone, string seats) {
            InitializeComponent();
            IsEditMode = true;

            NumberBox.Text = number;
            LabelBox.Text = label;
            SeatsBox.Text = seats;

            foreach (ComboBoxItem item in ZoneBox.Items) {
                if (item.Content?.ToString() == zone) {
                    ZoneBox.SelectedItem = item;
                    break;
                }
            }

            ConfigureDialog();
        }


        // METHODS:
        private void ConfigureDialog() {
            TableDialog.Header = IsEditMode ? "Edit Table" : "Add Table";
            SaveButton.Content = IsEditMode ? "Save" : "Add";
        }


        // EVENT HANDLERS:
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }


        // SPINNER METHODS:
        private void ChangeSpinnerValue(TextBox textBox, int change) {
            int value = int.TryParse(textBox.Text, out int parsed) ? parsed : 0;
            textBox.Text = Math.Max(1, (long)value + change).ToString();
        }


        // NUMBER EVENT HANDLERS:
        private void NumberUp_Click(object sender, RoutedEventArgs e) {
            ChangeSpinnerValue(NumberBox, 1);
        }

        private void NumberDown_Click(object sender, RoutedEventArgs e) {
            ChangeSpinnerValue(NumberBox, -1);
        }


        // SEATS EVENT HANDLERS:
        private void SeatsUp_Click(object sender, RoutedEventArgs e) {
            ChangeSpinnerValue(SeatsBox, 1);
        }

        private void SeatsDown_Click(object sender, RoutedEventArgs e) {
            ChangeSpinnerValue(SeatsBox, -1);
        }
    }
}