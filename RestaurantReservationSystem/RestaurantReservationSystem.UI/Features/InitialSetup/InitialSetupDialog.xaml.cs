using RestaurantReservationSystem.UI.Features.InitialSetup.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.InitialSetup
{
	public partial class InitialSetupDialog : UserControl
	{

		// EVENTS:
		public event EventHandler<InitialSetupConfiguration>? SaveRequested;

		// CONSTRUCTORS:
		public InitialSetupDialog()
		{
			InitializeComponent();
			AttachValidationEvents();
			UpdateValidationVisuals();
		}

		private void AttachValidationEvents()
		{
			FirstNameVisibleBox.Checked += OnCheckboxChanged;
			FirstNameVisibleBox.Unchecked += OnCheckboxChanged;
			FirstNameRequiredBox.Checked += OnCheckboxChanged;
			FirstNameRequiredBox.Unchecked += OnCheckboxChanged;

			LastNameVisibleBox.Checked += OnCheckboxChanged;
			LastNameVisibleBox.Unchecked += OnCheckboxChanged;
			LastNameRequiredBox.Checked += OnCheckboxChanged;
			LastNameRequiredBox.Unchecked += OnCheckboxChanged;

			PhoneVisibleBox.Checked += OnCheckboxChanged;
			PhoneVisibleBox.Unchecked += OnCheckboxChanged;
			PhoneRequiredBox.Checked += OnCheckboxChanged;
			PhoneRequiredBox.Unchecked += OnCheckboxChanged;

			NumberVisibleBox.Checked += OnCheckboxChanged;
			NumberVisibleBox.Unchecked += OnCheckboxChanged;
			NumberRequiredBox.Checked += OnCheckboxChanged;
			NumberRequiredBox.Unchecked += OnCheckboxChanged;

			LabelVisibleBox.Checked += OnCheckboxChanged;
			LabelVisibleBox.Unchecked += OnCheckboxChanged;
			LabelRequiredBox.Checked += OnCheckboxChanged;
			LabelRequiredBox.Unchecked += OnCheckboxChanged;

			ZoneVisibleBox.Checked += OnCheckboxChanged;
			ZoneVisibleBox.Unchecked += OnCheckboxChanged;
			ZoneRequiredBox.Checked += OnCheckboxChanged;
			ZoneRequiredBox.Unchecked += OnCheckboxChanged;
		}

		private void OnCheckboxChanged(object sender, RoutedEventArgs e)
		{
			UpdateValidationVisuals();
		}

		private bool IsReservationValid()
		{
			return (FirstNameVisibleBox.IsChecked == true && FirstNameRequiredBox.IsChecked == true) ||
				   (LastNameVisibleBox.IsChecked == true && LastNameRequiredBox.IsChecked == true) ||
				   (PhoneVisibleBox.IsChecked == true && PhoneRequiredBox.IsChecked == true);
		}

		private bool IsTableValid()
		{
			return (NumberVisibleBox.IsChecked == true && NumberRequiredBox.IsChecked == true) ||
				   (LabelVisibleBox.IsChecked == true && LabelRequiredBox.IsChecked == true);
		}

		private void UpdateValidationVisuals()
		{
			bool reservationValid = IsReservationValid();
			bool tableValid = IsTableValid();

			var dangerBrush = (Brush)FindResource("Danger");
			var secondaryBrush = (Brush)FindResource("SecondaryText");

			ReservationValidationText.Foreground = reservationValid ? secondaryBrush : dangerBrush;
			TableValidationText.Foreground = tableValid ? secondaryBrush : dangerBrush;
		}

		private bool ValidateForm()
		{
			return IsReservationValid() && IsTableValid();
		}

		private InitialSetupConfiguration GetCurrentConfiguration()
		{
			return new InitialSetupConfiguration
			{
				FirstNameVisible = FirstNameVisibleBox.IsChecked == true,
				FirstNameRequired = FirstNameRequiredBox.IsChecked == true,
				LastNameVisible = LastNameVisibleBox.IsChecked == true,
				LastNameRequired = LastNameRequiredBox.IsChecked == true,
				PhoneVisible = PhoneVisibleBox.IsChecked == true,
				PhoneRequired = PhoneRequiredBox.IsChecked == true,
				TableNumberVisible = NumberVisibleBox.IsChecked == true,
				TableNumberRequired = NumberRequiredBox.IsChecked == true,
				TableLabelVisible = LabelVisibleBox.IsChecked == true,
				TableLabelRequired = LabelRequiredBox.IsChecked == true,
				TableZoneVisible = ZoneVisibleBox.IsChecked == true,
				TableZoneRequired = ZoneRequiredBox.IsChecked == true,
				IsNumberFirst = NumberFirstRadio.IsChecked == true
			};
		}

		// EVENT HANDLERS:
		private void Submit_Click(object sender, RoutedEventArgs e)
		{
			UpdateValidationVisuals();

			if (!ValidateForm())
			{
				return;
			}

			var mainWindow = Window.GetWindow(this) as MainWindow;
			if (mainWindow == null) return;

			var confirmDialog = new ConfirmInitialSetupDialog();
			confirmDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(confirmDialog, false);

			confirmDialog.ConfirmRequested += (_, _) => {
				var config = GetCurrentConfiguration();
				// TODO backend: _configurationRepository.SaveConfiguration(config);
				SaveRequested?.Invoke(this, config);
				mainWindow.DialogHost.Close(confirmDialog, true);
			};

			mainWindow.DialogHost.Show(confirmDialog);
		}
	}

	public class InitialSetupConfiguration
	{
		public bool FirstNameVisible { get; set; }
		public bool FirstNameRequired { get; set; }
		public bool LastNameVisible { get; set; }
		public bool LastNameRequired { get; set; }
		public bool PhoneVisible { get; set; }
		public bool PhoneRequired { get; set; }

		public bool TableNumberVisible { get; set; }
		public bool TableNumberRequired { get; set; }
		public bool TableLabelVisible { get; set; }
		public bool TableLabelRequired { get; set; }
		public bool TableZoneVisible { get; set; }
		public bool TableZoneRequired { get; set; }

		public bool IsNumberFirst { get; set; }
	}
}