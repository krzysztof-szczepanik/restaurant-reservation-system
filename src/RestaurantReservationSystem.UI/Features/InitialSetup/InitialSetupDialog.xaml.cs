using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.InitialSetup.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.InitialSetup
{
	public partial class InitialSetupDialog : UserControl
	{

		// FIELDS:
		private bool _confirmationOpen;
		private bool _hasReservationError;
		private bool _hasTableError;


		// EVENTS:
		public event EventHandler? SaveRequested;


		// CONSTRUCTORS:
		public InitialSetupDialog()
		{
			InitializeComponent();
		}


		// METHODS:
		public InitialConfiguration GetConfiguration()
		{
			return new InitialConfiguration
			{
				FirstNameVisible = FirstNameVisibleBox.IsChecked == true,
				FirstNameRequired = FirstNameVisibleBox.IsChecked == true && FirstNameRequiredBox.IsChecked == true,

				LastNameVisible = LastNameVisibleBox.IsChecked == true,
				LastNameRequired = LastNameVisibleBox.IsChecked == true && LastNameRequiredBox.IsChecked == true,

				PhoneNumberVisible = PhoneVisibleBox.IsChecked == true,
				PhoneNumberRequired = PhoneVisibleBox.IsChecked == true && PhoneRequiredBox.IsChecked == true,

				TableNumberVisible = NumberVisibleBox.IsChecked == true,
				TableNumberRequired = NumberVisibleBox.IsChecked == true && NumberRequiredBox.IsChecked == true,

				TableLabelVisible = LabelVisibleBox.IsChecked == true,
				TableLabelRequired = LabelVisibleBox.IsChecked == true && LabelRequiredBox.IsChecked == true,

				TableZoneVisible = ZoneVisibleBox.IsChecked == true,

				TableIdentifierOrder = LabelFirstRadio.IsChecked == true ? TableIdentifierOrder.LabelThenNumber : TableIdentifierOrder.NumberThenLabel
			};
		}

		private bool IsReservationSectionValid()
		{
			return (FirstNameVisibleBox.IsChecked == true && FirstNameRequiredBox.IsChecked == true) ||
				   (LastNameVisibleBox.IsChecked == true && LastNameRequiredBox.IsChecked == true) ||
				   (PhoneVisibleBox.IsChecked == true && PhoneRequiredBox.IsChecked == true);
		}

		private bool IsTableSectionValid()
		{
			return (NumberVisibleBox.IsChecked == true && NumberRequiredBox.IsChecked == true) ||
				   (LabelVisibleBox.IsChecked == true && LabelRequiredBox.IsChecked == true);
		}

		private void UpdateValidationVisuals()
		{
			var dangerBrush = TryFindResource("Danger") as Brush ?? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
			var secondaryBrush = TryFindResource("SecondaryText") as Brush ?? new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));

			ReservationValidationText.Foreground = _hasReservationError ? dangerBrush : secondaryBrush;
			TableValidationText.Foreground = _hasTableError ? dangerBrush : secondaryBrush;
		}

		private bool ValidateForm()
		{
			bool reservationValid = IsReservationSectionValid();
			bool tableValid = IsTableSectionValid();

			_hasReservationError = !reservationValid;
			_hasTableError = !tableValid;

			UpdateValidationVisuals();

			return reservationValid && tableValid;
		}


		// EVENT HANDLERS:
		private void ReservationField_Changed(object sender, RoutedEventArgs e)
		{
			if (_hasReservationError && IsReservationSectionValid())
			{
				_hasReservationError = false;
				UpdateValidationVisuals();
			}
		}

		private void TableField_Changed(object sender, RoutedEventArgs e)
		{
			if (_hasTableError && IsTableSectionValid())
			{
				_hasTableError = false;
				UpdateValidationVisuals();
			}
		}

		private void Submit_Click(object sender, RoutedEventArgs e)
		{
			if (_confirmationOpen || !ValidateForm())
			{
				return;
			}

			var mainWindow = Window.GetWindow(this) as MainWindow;

			if (mainWindow is null)
			{
				return;
			}

			_confirmationOpen = true;

			var confirmDialog = new ConfirmInitialSetupDialog();

			confirmDialog.CancelRequested += (_, _) =>
			{
				_confirmationOpen = false;
				mainWindow.DialogHost.Close(false);
			};

			confirmDialog.ConfirmRequested += (_, _) =>
			{
				_confirmationOpen = false;
				SaveRequested?.Invoke(this, EventArgs.Empty);
			};

			mainWindow.DialogHost.Show(confirmDialog);
		}
	}
}