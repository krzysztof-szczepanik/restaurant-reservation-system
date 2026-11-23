using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs
{
	public partial class AddEditTableDialog : UserControl
	{

		// EVENTS:
		public event EventHandler? SaveRequested;
		public event EventHandler? CancelRequested;

		// PROPERTIES:
		public bool IsEditMode { get; }

		public string TableNumber => NumberBox.Text.Trim();
		public string TableLabel => LabelBox.Text.Trim();
		public string TableZone => ZoneBox.Text?.Trim() is { Length: > 0 } zoneText ? zoneText : "No zone";
		public string TableSeats => SeatsBox.Text.Trim();

		// CONSTRUCTORS:
		public AddEditTableDialog()
		{
			InitializeComponent();
			IsEditMode = false;
			ConfigureDialog();
		}

		public AddEditTableDialog(string number, string label, string zone, string seats)
		{
			InitializeComponent();
			IsEditMode = true;

			NumberBox.Text = number;
			LabelBox.Text = label;
			SeatsBox.Text = seats;
			ZoneBox.Text = zone;

			ConfigureDialog();
		}

		// METHODS:
		private void ConfigureDialog()
		{
			TableDialog.Header = IsEditMode ? "Edit Table" : "Add Table";
			SaveButton.Content = IsEditMode ? "Save" : "Add";
		}

		// EVENT HANDLERS:
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (ValidateForm())
			{
				SaveRequested?.Invoke(this, EventArgs.Empty);
			}
		}

		private bool ValidateForm()
		{
			bool isValid = true;
			var dangerBrush = (Brush)FindResource("Danger");
			var defaultBorder = (Brush)FindResource("Border");

			if (string.IsNullOrWhiteSpace(TableNumber) || !int.TryParse(TableNumber, out int num) || num <= 0)
			{
				NumberBorder.BorderBrush = dangerBrush;
				NumberErrorText.Visibility = Visibility.Visible;
				isValid = false;
			}
			else
			{
				NumberBorder.BorderBrush = defaultBorder;
				NumberErrorText.Visibility = Visibility.Collapsed;
			}

			if (!string.IsNullOrEmpty(TableLabel) && !Regex.IsMatch(TableLabel, "^[A-Za-z]+$"))
			{
				LabelBox.BorderBrush = dangerBrush;
				LabelErrorText.Visibility = Visibility.Visible;
				isValid = false;
			}
			else
			{
				LabelBox.BorderBrush = defaultBorder;
				LabelErrorText.Visibility = Visibility.Collapsed;
			}

			if (string.IsNullOrWhiteSpace(TableSeats) || !int.TryParse(TableSeats, out int seats) || seats <= 0)
			{
				SeatsBorder.BorderBrush = dangerBrush;
				SeatsErrorText.Text = string.IsNullOrWhiteSpace(TableSeats) ? "This field is required." : "Enter a number greater than 0.";
				SeatsErrorText.Visibility = Visibility.Visible;
				isValid = false;
			}
			else
			{
				SeatsBorder.BorderBrush = defaultBorder;
				SeatsErrorText.Visibility = Visibility.Collapsed;
			}

			return isValid;
		}

		// SPINNER METHODS:
		private void ChangeSpinnerValue(TextBox textBox, int change)
		{
			int value = int.TryParse(textBox.Text, out int parsed) ? parsed : 0;
			textBox.Text = Math.Max(1, (long)value + change).ToString();
		}

		// NUMBER EVENT HANDLERS:
		private void NumberUp_Click(object sender, RoutedEventArgs e)
		{
			ChangeSpinnerValue(NumberBox, 1);
		}

		private void NumberDown_Click(object sender, RoutedEventArgs e)
		{
			ChangeSpinnerValue(NumberBox, -1);
		}

		// SEATS EVENT HANDLERS:
		private void SeatsUp_Click(object sender, RoutedEventArgs e)
		{
			ChangeSpinnerValue(SeatsBox, 1);
		}

		private void SeatsDown_Click(object sender, RoutedEventArgs e)
		{
			ChangeSpinnerValue(SeatsBox, -1);
		}
	}
}