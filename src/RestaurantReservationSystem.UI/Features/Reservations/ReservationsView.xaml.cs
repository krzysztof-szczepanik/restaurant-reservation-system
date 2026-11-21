using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.Reservations.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantReservationSystem.UI.Features.Reservations
{
	public partial class ReservationsView : UserControl
	{

		// VIEW MODEL ROW:
		private sealed class ReservationRowItem
		{
			public required Reservation Source { get; init; }
			public int Id => Source.Id;
			public DateOnly Date => Source.Date;
			public TimeOnly Time => Source.Time;
			public string? FirstName => Source.FirstName;
			public string? LastName => Source.LastName;
			public string PhoneDisplay { get; init; } = string.Empty;
			public int Guests => Source.Guests;
			public string TablesDisplay { get; init; } = string.Empty;
			public string NoteDisplay { get; init; } = string.Empty;
		}


		// FIELDS:
		private InitialConfiguration _configuration = new();
		private ReservationFilterCriteria _activeCriteria = new();


		// PROPERTIES:
		public bool HasActiveFilters
		{
			get => FilterButton.Tag is true;
			set => FilterButton.Tag = value;
		}


		// CONSTRUCTORS:
		public ReservationsView()
		{
			InitializeComponent();

			Loaded += ReservationsView_Loaded;
		}


		// METHODS:
		private MainWindow? GetMainWindow()
		{
			return Window.GetWindow(this) as MainWindow;
		}

		private async Task LoadConfigurationAsync()
		{
			using var scope = App.ServiceProvider.CreateScope();

			var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
			var configuration = await configurationService.GetAsync();

			if (configuration is null)
			{
				throw new InvalidOperationException("Initial configuration does not exist.");
			}

			_configuration = configuration;

			ReservationsDataGrid.Columns[2].Visibility = configuration.FirstNameVisible ? Visibility.Visible : Visibility.Collapsed;
			ReservationsDataGrid.Columns[3].Visibility = configuration.LastNameVisible ? Visibility.Visible : Visibility.Collapsed;
			ReservationsDataGrid.Columns[4].Visibility = configuration.PhoneNumberVisible ? Visibility.Visible : Visibility.Collapsed;
		}

		private async Task LoadReservationsAsync(int? selectReservationId = null)
		{
			using var scope = App.ServiceProvider.CreateScope();
			var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

			var result = await reservationService.GetFilteredAsync(_activeCriteria);
			if (!result.IsSuccess || result.Data is null)
			{
				MessageBox.Show(
					result.Message ?? "Failed to load reservations.",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}

			var rows = result.Data
				.Select(r => new ReservationRowItem
				{
					Source = r,
					PhoneDisplay = FormatPhoneNumber(r.PhoneNumber),
					TablesDisplay = FormatTables(r.Tables),
					NoteDisplay = FormatSingleLineNote(r.Note)
				})
				.ToList();

			ReservationsDataGrid.ItemsSource = rows;

			if (selectReservationId.HasValue)
			{
				var targetRow = rows.FirstOrDefault(r => r.Id == selectReservationId.Value);
				if (targetRow is not null)
				{
					ReservationsDataGrid.SelectedItem = targetRow;
					ReservationsDataGrid.ScrollIntoView(targetRow);
				}
			}

			UpdateActionButtonsState();
		}

		private void UpdateActionButtonsState()
		{
			bool hasSelection = ReservationsDataGrid.SelectedItem is ReservationRowItem;
			EditButton.IsEnabled = hasSelection;
			DeleteButton.IsEnabled = hasSelection;
		}

		private string FormatTableIdentifier(Table table)
		{
			string numberPart = _configuration.TableNumberVisible && table.Number.HasValue
				? table.Number.Value.ToString()
				: string.Empty;

			string labelPart = _configuration.TableLabelVisible && !string.IsNullOrWhiteSpace(table.Label)
				? table.Label
				: string.Empty;

			return _configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber
				? $"{labelPart}{numberPart}"
				: $"{numberPart}{labelPart}";
		}

		private string FormatTables(IEnumerable<Table> tables)
		{
			var ordered = tables
				.OrderBy(t => t.Number ?? int.MaxValue)
				.ThenBy(t => t.Label ?? string.Empty, StringComparer.OrdinalIgnoreCase)
				.Select(FormatTableIdentifier)
				.Where(s => !string.IsNullOrEmpty(s));

			return string.Join(", ", ordered);
		}

		private static string FormatPhoneNumber(string? phone)
		{
			if (string.IsNullOrWhiteSpace(phone))
			{
				return string.Empty;
			}

			string digits = phone.Replace(" ", string.Empty);
			if (digits.Length != 9)
			{
				return phone;
			}

			return $"{digits[..3]} {digits[3..6]} {digits[6..9]}";
		}

		private static string FormatSingleLineNote(string? note)
		{
			if (string.IsNullOrWhiteSpace(note))
			{
				return string.Empty;
			}

			return note.Replace("\r\n", " ").Replace("\n", " ").Trim();
		}


		// EVENT HANDLERS:
		private async void ReservationsView_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= ReservationsView_Loaded;

			try
			{
				await LoadConfigurationAsync();
				await LoadReservationsAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to load reservations.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		private void ReservationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateActionButtonsState();
		}

		private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!SearchBox.IsKeyboardFocusWithin)
			{
				return;
			}

			FocusManager.SetFocusedElement(FocusManager.GetFocusScope(SearchBox), null);
			Keyboard.ClearFocus();
		}

		private void AddReservations_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow is null)
			{
				return;
			}

			var dialog = new AddEditReservationDialog(isEditMode: false);
			dialog.ApplyConfiguration(_configuration);

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.SaveRequested += async (_, _) =>
			{
				if (!dialog.ValidateForm(out var newReservation) || newReservation is null)
				{
					return;
				}

				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

					var result = await reservationService.AddAsync(newReservation);
					if (!result.IsSuccess)
					{
						if (result.ValidationErrors.Count > 0)
						{
							dialog.ApplyValidationErrors(result.ValidationErrors);
						}
						else
						{
							MessageBox.Show(
								result.Message ?? "Failed to add reservation.",
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
						return;
					}

					mainWindow.DialogHost.Close(dialog, true);
					await LoadReservationsAsync(result.Data?.Id);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"An unexpected error occurred while adding the reservation.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}

		private async void EditReservations_Click(object sender, RoutedEventArgs e)
		{
			if (ReservationsDataGrid.SelectedItem is not ReservationRowItem selectedRow)
			{
				return;
			}

			var mainWindow = GetMainWindow();
			if (mainWindow is null)
			{
				return;
			}

			Reservation? freshReservation;
			try
			{
				using var scope = App.ServiceProvider.CreateScope();
				var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
				freshReservation = await reservationService.GetByIdAsync(selectedRow.Id);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to load reservation details.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}

			if (freshReservation is null)
			{
				MessageBox.Show(
					"The selected reservation no longer exists.",
					"Not Found",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				await LoadReservationsAsync();
				return;
			}

			bool requiresUnlock = freshReservation.Status is ReservationStatus.Cancelled or ReservationStatus.NoShow;

			if (requiresUnlock)
			{
				var unlockDialog = new UnlockReservationDialog();

				unlockDialog.CancelRequested += (_, _) =>
				{
					mainWindow.DialogHost.Close(unlockDialog, false);
				};

				unlockDialog.ConfirmRequested += (_, _) =>
				{
					mainWindow.DialogHost.Close(unlockDialog, true);
					OpenEditReservationDialog(mainWindow, freshReservation, unlockCancelledOrNoShow: true);
				};

				mainWindow.DialogHost.Show(unlockDialog);
				return;
			}

			OpenEditReservationDialog(mainWindow, freshReservation, unlockCancelledOrNoShow: false);
		}

		private void OpenEditReservationDialog(MainWindow mainWindow, Reservation reservation, bool unlockCancelledOrNoShow)
		{
			var dialog = new AddEditReservationDialog(isEditMode: true);
			dialog.ApplyConfiguration(_configuration);
			dialog.LoadReservation(reservation);

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.SaveRequested += async (_, _) =>
			{
				if (!dialog.ValidateForm(out var updatedReservation) || updatedReservation is null)
				{
					return;
				}

				updatedReservation.Status = reservation.Status;

				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

					var result = await reservationService.UpdateAsync(updatedReservation, unlockCancelledOrNoShow);
					if (!result.IsSuccess)
					{
						if (result.ValidationErrors.Count > 0)
						{
							dialog.ApplyValidationErrors(result.ValidationErrors);
						}
						else
						{
							MessageBox.Show(
								result.Message ?? "Failed to update reservation.",
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
						return;
					}

					mainWindow.DialogHost.Close(dialog, true);
					await LoadReservationsAsync(updatedReservation.Id);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"An unexpected error occurred while updating the reservation.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}

		private void DeleteReservations_Click(object sender, RoutedEventArgs e)
		{
			if (ReservationsDataGrid.SelectedItem is not ReservationRowItem selectedRow)
			{
				return;
			}

			var mainWindow = GetMainWindow();
			if (mainWindow is null)
			{
				return;
			}

			var dialog = new DeleteReservationsDialog();

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.DeleteRequested += async (_, _) =>
			{
				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

					var result = await reservationService.DeleteAsync(selectedRow.Id);
					if (!result.IsSuccess)
					{
						MessageBox.Show(
							result.Message ?? "Failed to delete reservation.",
							"Error",
							MessageBoxButton.OK,
							MessageBoxImage.Error);
						return;
					}

					mainWindow.DialogHost.Close(dialog, true);
					await LoadReservationsAsync();
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"An unexpected error occurred while deleting the reservation.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}

		private void Filter_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow is null)
			{
				return;
			}

			var dialog = new FilterReservationsDialog();

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.ApplyRequested += (_, _) =>
			{
				mainWindow.DialogHost.Close(dialog, true);
			};

			mainWindow.DialogHost.Show(dialog);
		}
	}
}