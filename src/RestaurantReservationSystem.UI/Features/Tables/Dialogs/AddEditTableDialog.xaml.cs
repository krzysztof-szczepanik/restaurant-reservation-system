using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AppValidationError = RestaurantReservationSystem.Application.OperationResults.ValidationError;

namespace RestaurantReservationSystem.UI.Features.Tables.Dialogs
{
	public partial class AddEditTableDialog : UserControl
	{

		// REGEXES:
		private static readonly Regex LabelRegex = new(@"^[A-Za-z]+$", RegexOptions.Compiled);


		// EVENTS:
		public event EventHandler? SaveRequested;
		public event EventHandler? CancelRequested;


		// FIELDS:
		private InitialConfiguration _configuration = new();
		private readonly int? _editingTableId;
		private readonly string? _initialZone;


		// PROPERTIES:
		public bool IsEditMode { get; }

		public string TableNumber => NumberBox.Text;
		public string TableLabel => LabelBox.Text;
		public string TableZone
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(ZoneBox.Text))
				{
					return ZoneBox.Text.Trim();
				}

				return (ZoneBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim() ?? "No zone";
			}
		}
		public string TableSeats => SeatsBox.Text;


		// CONSTRUCTORS:
		public AddEditTableDialog()
		{
			InitializeComponent();
			IsEditMode = false;
			ConfigureDialog();
			Loaded += AddEditTableDialog_Loaded;
		}

		public AddEditTableDialog(int tableId, string number, string label, string zone, string seats)
		{
			InitializeComponent();
			IsEditMode = true;
			_editingTableId = tableId;
			_initialZone = zone;

			NumberBox.Text = number;
			LabelBox.Text = label;
			ZoneBox.Text = zone;
			SeatsBox.Text = seats;

			ConfigureDialog();
			Loaded += AddEditTableDialog_Loaded;
		}


		// METHODS:
		private void ConfigureDialog()
		{
			TableDialog.Header = IsEditMode ? "Edit Table" : "Add Table";
			SaveButton.Content = IsEditMode ? "Save" : "Add";
		}

		public void ApplyConfiguration(InitialConfiguration configuration, IEnumerable<string> existingZones)
		{
			_configuration = configuration;

			NumberPanel.Visibility = configuration.TableNumberVisible ? Visibility.Visible : Visibility.Collapsed;
			LabelPanel.Visibility = configuration.TableLabelVisible ? Visibility.Visible : Visibility.Collapsed;
			ZonePanel.Visibility = configuration.TableZoneVisible ? Visibility.Visible : Visibility.Collapsed;

			NumberLabel.Text = configuration.TableNumberRequired ? "Number *" : "Number";
			LabelFieldLabel.Text = configuration.TableLabelRequired ? "Label *" : "Label";
			ZoneLabel.Text = configuration.TableZoneRequired ? "Zone *" : "Zone";

			// Reorder Number and Label panels according to TableIdentifierOrder:
			FieldsContainer.Children.Remove(NumberPanel);
			FieldsContainer.Children.Remove(LabelPanel);

			if (configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber)
			{
				FieldsContainer.Children.Insert(0, LabelPanel);
				FieldsContainer.Children.Insert(1, NumberPanel);
			}
			else
			{
				FieldsContainer.Children.Insert(0, NumberPanel);
				FieldsContainer.Children.Insert(1, LabelPanel);
			}

			// Populate ZoneBox items:
			ZoneBox.Items.Clear();
			foreach (var zone in existingZones)
			{
				ZoneBox.Items.Add(new ComboBoxItem { Content = zone });
			}
			ZoneBox.Items.Add(new ComboBoxItem { Content = "No zone" });

			if (!string.IsNullOrWhiteSpace(_initialZone))
			{
				ZoneBox.Text = _initialZone;
			}
		}

		public bool ValidateForm(out Table? table)
		{
			table = null;
			ClearAllErrors();

			Control? firstInvalidControl = null;

			void RegisterError(Control focusTarget, FrameworkElement borderElement, TextBlock errorText, string message)
			{
				SetFieldError(borderElement, errorText, message);
				firstInvalidControl ??= focusTarget;
			}

			int? parsedNumber = null;
			if (_configuration.TableNumberVisible)
			{
				string rawNumber = NumberBox.Text.Trim();
				if (string.IsNullOrEmpty(rawNumber))
				{
					if (_configuration.TableNumberRequired)
					{
						RegisterError(NumberBox, NumberBorder, NumberErrorText, "This field is required.");
					}
				}
				else if (!int.TryParse(rawNumber, NumberStyles.None, CultureInfo.InvariantCulture, out int numVal) || numVal <= 0)
				{
					RegisterError(NumberBox, NumberBorder, NumberErrorText, "Enter a number greater than 0.");
				}
				else
				{
					parsedNumber = numVal;
				}
			}

			string? normalizedLabel = null;
			if (_configuration.TableLabelVisible)
			{
				string rawLabel = LabelBox.Text.Trim();
				if (string.IsNullOrEmpty(rawLabel))
				{
					if (_configuration.TableLabelRequired)
					{
						RegisterError(LabelBox, LabelBox, LabelErrorText, "This field is required.");
					}
				}
				else if (!LabelRegex.IsMatch(rawLabel))
				{
					RegisterError(LabelBox, LabelBox, LabelErrorText, "Use letters A-Z only.");
				}
				else
				{
					normalizedLabel = rawLabel.ToUpperInvariant();
				}
			}

			string? normalizedZone = null;
			if (_configuration.TableZoneVisible)
			{
				string rawZone = TableZone.Trim();
				bool isEmptyOrNoZone = string.IsNullOrEmpty(rawZone) ||
									   rawZone.Equals("No zone", StringComparison.OrdinalIgnoreCase);

				if (isEmptyOrNoZone)
				{
					if (_configuration.TableZoneRequired && string.IsNullOrEmpty(rawZone))
					{
						RegisterError(ZoneBox, ZoneBox, ZoneErrorText, "This field is required.");
					}
					else
					{
						normalizedZone = null;
					}
				}
				else
				{
					normalizedZone = rawZone;
				}
			}

			string rawSeats = SeatsBox.Text.Trim();
			int parsedSeats = 0;
			if (string.IsNullOrEmpty(rawSeats))
			{
				RegisterError(SeatsBox, SeatsBorder, SeatsErrorText, "This field is required.");
			}
			else if (!int.TryParse(rawSeats, NumberStyles.None, CultureInfo.InvariantCulture, out parsedSeats) || parsedSeats <= 0)
			{
				RegisterError(SeatsBox, SeatsBorder, SeatsErrorText, "Enter a number greater than 0.");
			}

			if (firstInvalidControl is not null)
			{
				firstInvalidControl.Focus();
				return false;
			}

			table = new Table
			{
				Id = _editingTableId ?? 0,
				Number = parsedNumber,
				Label = normalizedLabel,
				Zone = normalizedZone,
				Seats = parsedSeats
			};

			return true;
		}

		public void ApplyValidationErrors(IReadOnlyList<AppValidationError> errors)
		{
			ClearAllErrors();
			Control? firstInvalidControl = null;

			foreach (var error in errors)
			{
				switch (error.FieldName)
				{
					case nameof(Table.Number):
						SetFieldError(NumberBorder, NumberErrorText, error.Message);
						firstInvalidControl ??= NumberBox;
						break;
					case nameof(Table.Label):
						SetFieldError(LabelBox, LabelErrorText, error.Message);
						firstInvalidControl ??= LabelBox;
						break;
					case nameof(Table.Zone):
						SetFieldError(ZoneBox, ZoneErrorText, error.Message);
						firstInvalidControl ??= ZoneBox;
						break;
					case nameof(Table.Seats):
						SetFieldError(SeatsBorder, SeatsErrorText, error.Message);
						firstInvalidControl ??= SeatsBox;
						break;
					default:
						if (_configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber && _configuration.TableLabelVisible)
						{
							SetFieldError(LabelBox, LabelErrorText, error.Message);
							firstInvalidControl ??= LabelBox;
						}
						else if (_configuration.TableNumberVisible)
						{
							SetFieldError(NumberBorder, NumberErrorText, error.Message);
							firstInvalidControl ??= NumberBox;
						}
						else if (_configuration.TableLabelVisible)
						{
							SetFieldError(LabelBox, LabelErrorText, error.Message);
							firstInvalidControl ??= LabelBox;
						}
						else
						{
							MessageBox.Show(error.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
						break;
				}
			}

			firstInvalidControl?.Focus();
		}

		private void SetFieldError(FrameworkElement borderElement, TextBlock errorText, string message)
		{
			var dangerBrush = (Brush)FindResource("Danger");
			if (borderElement is Control control)
			{
				control.BorderBrush = dangerBrush;
			}
			else if (borderElement is Border border)
			{
				border.BorderBrush = dangerBrush;
			}

			errorText.Text = message;
			errorText.Visibility = Visibility.Visible;
		}

		private void ClearFieldError(FrameworkElement borderElement, TextBlock errorText)
		{
			var defaultBorderBrush = (Brush)FindResource("Border");
			if (borderElement is Control control)
			{
				control.BorderBrush = defaultBorderBrush;
			}
			else if (borderElement is Border border)
			{
				border.BorderBrush = defaultBorderBrush;
			}

			errorText.Text = string.Empty;
			errorText.Visibility = Visibility.Collapsed;
		}

		private void ClearAllErrors()
		{
			ClearFieldError(NumberBorder, NumberErrorText);
			ClearFieldError(LabelBox, LabelErrorText);
			ClearFieldError(ZoneBox, ZoneErrorText);
			ClearFieldError(SeatsBorder, SeatsErrorText);
		}


		// EVENT HANDLERS:
		private void AddEditTableDialog_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= AddEditTableDialog_Loaded;

			if (_configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber && _configuration.TableLabelVisible)
			{
				LabelBox.Focus();
			}
			else if (_configuration.TableNumberVisible)
			{
				NumberBox.Focus();
			}
			else if (_configuration.TableLabelVisible)
			{
				LabelBox.Focus();
			}
			else if (_configuration.TableZoneVisible)
			{
				ZoneBox.Focus();
			}
			else
			{
				SeatsBox.Focus();
			}
		}

		private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				SaveRequested?.Invoke(this, EventArgs.Empty);
			}
		}

		private void Field_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender == NumberBox)
			{
				ClearFieldError(NumberBorder, NumberErrorText);
			}
			else if (sender == LabelBox)
			{
				ClearFieldError(LabelBox, LabelErrorText);
			}
			else if (sender == ZoneBox || e.OriginalSource is TextBox { TemplatedParent: ComboBox })
			{
				ClearFieldError(ZoneBox, ZoneErrorText);
			}
			else if (sender == SeatsBox)
			{
				ClearFieldError(SeatsBorder, SeatsErrorText);
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			SaveRequested?.Invoke(this, EventArgs.Empty);
		}


		// SPINNER METHODS:
		private void ChangeSpinnerValue(TextBox textBox, int change)
		{
			int value = int.TryParse(textBox.Text.Trim(), out int parsed) ? parsed : 0;
			textBox.Text = Math.Max(1, (long)value + change).ToString(CultureInfo.InvariantCulture);
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