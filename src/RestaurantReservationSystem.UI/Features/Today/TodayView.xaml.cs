using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.Reservations.Dialogs;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Today
{
	public partial class TodayView : UserControl
	{

		// VIEW MODELS:
		public sealed class StatusOptionItem
		{
			public required string Label { get; init; }
			public ReservationStatus? TargetStatus { get; init; }
			public bool IsUnlockAction { get; init; }
			public bool ShowSeparatorBefore { get; init; }

			public override bool Equals(object? obj) =>
				obj is StatusOptionItem other &&
				Label == other.Label &&
				TargetStatus == other.TargetStatus &&
				IsUnlockAction == other.IsUnlockAction;

			public override int GetHashCode() => HashCode.Combine(Label, TargetStatus, IsUnlockAction);
		}

		public sealed class TodayReservationRow
		{
			private static readonly IReadOnlyList<StatusOptionItem> ActiveStatusOptions =
			[
				new StatusOptionItem { Label = "Expected", TargetStatus = ReservationStatus.Expected },
				new StatusOptionItem { Label = "Late", TargetStatus = ReservationStatus.Late },
				new StatusOptionItem { Label = "Arrived", TargetStatus = ReservationStatus.Arrived },
				new StatusOptionItem { Label = "Cancelled", TargetStatus = ReservationStatus.Cancelled },
				new StatusOptionItem { Label = "No-show", TargetStatus = ReservationStatus.NoShow }
			];

			private static readonly IReadOnlyList<StatusOptionItem> InactiveStatusOptions =
			[
				new StatusOptionItem { Label = "Cancelled", TargetStatus = ReservationStatus.Cancelled },
				new StatusOptionItem { Label = "No-show", TargetStatus = ReservationStatus.NoShow },
				new StatusOptionItem { Label = "Unlock", TargetStatus = null, IsUnlockAction = true, ShowSeparatorBefore = true }
			];

			public required Reservation Source { get; init; }
			public int Id => Source.Id;
			public string DateDisplay => Source.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
			public string TimeDisplay => Source.Time.ToString("HH:mm", CultureInfo.InvariantCulture);
			public string? FirstName => Source.FirstName;
			public string? LastName => Source.LastName;
			public string PhoneDisplay { get; init; } = string.Empty;
			public int Guests => Source.Guests;
			public string TablesDisplay { get; init; } = string.Empty;
			public string? TablesTooltip { get; init; }
			public string NoteDisplay { get; init; } = string.Empty;
			public string? NoteTooltip => string.IsNullOrWhiteSpace(Source.Note) ? null : Source.Note;
			public ReservationStatus Status => Source.Status;

			public IReadOnlyList<StatusOptionItem> StatusOptions =>
				Status is ReservationStatus.Cancelled or ReservationStatus.NoShow
					? InactiveStatusOptions
					: ActiveStatusOptions;

			public StatusOptionItem? SelectedOption =>
				StatusOptions.FirstOrDefault(o => !o.IsUnlockAction && o.TargetStatus == Status);

			public string StatusBackground => Status switch
			{
				ReservationStatus.Expected => "#EFF6FF",
				ReservationStatus.Late => "#FEFCE8",
				ReservationStatus.Arrived => "#F0FDF4",
				ReservationStatus.Cancelled => "#F8FAFC",
				ReservationStatus.NoShow => "#FEF2F2",
				_ => "#F8FAFC"
			};

			public string StatusBorder => Status switch
			{
				ReservationStatus.Expected => "#BFDBFE",
				ReservationStatus.Late => "#FEF08A",
				ReservationStatus.Arrived => "#BBF7D0",
				ReservationStatus.Cancelled => "#E2E8F0",
				ReservationStatus.NoShow => "#FECACA",
				_ => "#E2E8F0"
			};

			public string StatusForeground => Status switch
			{
				ReservationStatus.Expected => "#1D4ED8",
				ReservationStatus.Late => "#A16207",
				ReservationStatus.Arrived => "#15803D",
				ReservationStatus.Cancelled => "#64748B",
				ReservationStatus.NoShow => "#DC2626",
				_ => "#64748B"
			};
		}


		// PROPERTIES:
		public ObservableCollection<TodayReservationRow> TodayReservations { get; } = new();


		// FIELDS:
		private InitialConfiguration _configuration = new();
		private DateOnly _currentDate = DateOnly.FromDateTime(DateTime.Now);
		private bool _isHandlingStatusChange;
		private int? _selectedReservationId;


		// CONSTRUCTORS:
		public TodayView()
		{
			InitializeComponent();

			DataContext = this;
			UpdateHeaderDate();

			Loaded += TodayView_Loaded;
		}


		// METHODS:
		private MainWindow? GetMainWindow()
		{
			return Window.GetWindow(this) as MainWindow;
		}

		private void UpdateHeaderDate()
		{
			_currentDate = DateOnly.FromDateTime(DateTime.Now);
			TodayDateText.Text = _currentDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
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

			ReservationsTable.Columns[2].Visibility = configuration.FirstNameVisible ? Visibility.Visible : Visibility.Collapsed;
			ReservationsTable.Columns[3].Visibility = configuration.LastNameVisible ? Visibility.Visible : Visibility.Collapsed;
			ReservationsTable.Columns[4].Visibility = configuration.PhoneNumberVisible ? Visibility.Visible : Visibility.Collapsed;
		}

		private async Task LoadTodayReservationsAsync(int? selectReservationId = null)
		{
			UpdateHeaderDate();

			using var scope = App.ServiceProvider.CreateScope();
			var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

			var criteria = new ReservationFilterCriteria
			{
				Date = new ValueFilter<DateOnly>
				{
					Mode = FilterValueMode.Exact,
					Exact = _currentDate
				}
			};

			var result = await reservationService.GetFilteredAsync(criteria);
			if (!result.IsSuccess || result.Data is null)
			{
				MessageBox.Show(
					result.Message ?? "Failed to load today's reservations.",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}

			var sorted = result.Data
				.Where(r => r.Date == _currentDate)
				.OrderBy(r => GetStatusSortOrder(r.Status))
				.ThenBy(r => r.Time)
				.ThenBy(r => r.Id)
				.Select(r => new TodayReservationRow
				{
					Source = r,
					PhoneDisplay = FormatPhoneNumber(r.PhoneNumber),
					TablesDisplay = FormatTables(r.Tables),
					TablesTooltip = FormatTablesTooltip(r.Tables)
				})
				.ToList();

			_isHandlingStatusChange = true;
			try
			{
				TodayReservations.Clear();
				foreach (var row in sorted)
				{
					TodayReservations.Add(row);
				}

				int? targetId = selectReservationId ?? _selectedReservationId;
				if (targetId.HasValue)
				{
					var targetRow = TodayReservations.FirstOrDefault(r => r.Id == targetId.Value);
					if (targetRow is not null)
					{
						ReservationsTable.SelectedItem = targetRow;
						ReservationsTable.ScrollIntoView(targetRow);
					}
				}
			}
			finally
			{
				_isHandlingStatusChange = false;
			}
		}

		private static int GetStatusSortOrder(ReservationStatus status) => status switch
		{
			ReservationStatus.Expected => 0,
			ReservationStatus.Late => 1,
			ReservationStatus.Arrived => 2,
			ReservationStatus.Cancelled => 3,
			ReservationStatus.NoShow => 4,
			_ => 5
		};

		private string FormatTableIdentifier(Table table)
		{
			string numberPart = _configuration.TableNumberVisible && table.Number.HasValue
				? table.Number.Value.ToString()
				: string.Empty;

			string labelPart = _configuration.TableLabelVisible && !string.IsNullOrWhiteSpace(table.Label)
				? table.Label!
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

		private string? FormatTablesTooltip(IEnumerable<Table> tables)
		{
			var list = tables.ToList();
			if (list.Count == 0)
			{
				return null;
			}

			var grouped = list
				.GroupBy(t => !_configuration.TableZoneVisible || string.IsNullOrWhiteSpace(t.Zone) ? "No Zone" : t.Zone.Trim())
				.OrderBy(g => g.Key.Equals("No Zone", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
				.ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

			var lines = new List<string>();
			foreach (var group in grouped)
			{
				lines.Add($"{group.Key}:");
				foreach (var table in group.OrderBy(t => t.Number ?? int.MaxValue).ThenBy(t => t.Label ?? string.Empty, StringComparer.OrdinalIgnoreCase))
				{
					string idText = FormatTableIdentifier(table);
					lines.Add($"- {idText} ({table.Seats} seats)");
				}
			}

			return string.Join(Environment.NewLine, lines);
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

		private async Task<bool> PersistStatusChangeAsync(int reservationId, ReservationStatus newStatus)
		{
			try
			{
				using var scope = App.ServiceProvider.CreateScope();
				var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

				var reservation = await reservationService.GetByIdAsync(reservationId);
				if (reservation is null)
				{
					MessageBox.Show(
						"The selected reservation no longer exists.",
						"Not Found",
						MessageBoxButton.OK,
						MessageBoxImage.Warning);
					await LoadTodayReservationsAsync();
					return false;
				}

				reservation.Status = newStatus;
				var result = await reservationService.UpdateAsync(reservation, unlockCancelledOrNoShow: false);
				if (!result.IsSuccess)
				{
					MessageBox.Show(
						result.Message ?? "Failed to update reservation status.",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					return false;
				}

				await LoadTodayReservationsAsync(reservationId);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to update reservation status.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return false;
			}
		}

		private void OpenUnlockDialog(TodayReservationRow row)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow is null)
			{
				return;
			}

			var unlockDialog = new UnlockReservationDialog();

			unlockDialog.CancelRequested += (_, _) =>
			{
				mainWindow.DialogHost.Close(unlockDialog, false);
			};

			unlockDialog.ConfirmRequested += async (_, _) =>
			{
				mainWindow.DialogHost.Close(unlockDialog, true);

				Reservation? freshReservation;
				using (var scope = App.ServiceProvider.CreateScope())
				{
					var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
					freshReservation = await reservationService.GetByIdAsync(row.Id);
				}

				if (freshReservation is null)
				{
					await LoadTodayReservationsAsync();
					return;
				}

				// Switch to Reservations view if available on MainWindow before opening Edit dialog
				if (mainWindow.FindName("ReservationsNavButton") is Button navBtn)
				{
					navBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
				}

				var editDialog = new AddEditReservationDialog(isEditMode: true);
				editDialog.ApplyConfiguration(_configuration);
				editDialog.LoadReservation(freshReservation);

				editDialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(editDialog, false);

				editDialog.SaveRequested += async (_, _) =>
				{
					if (!editDialog.ValidateForm(out var updatedReservation) || updatedReservation is null)
					{
						return;
					}

					updatedReservation.Status = ReservationStatus.Expected;

					using var scope = App.ServiceProvider.CreateScope();
					var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

					var updateResult = await reservationService.UpdateAsync(updatedReservation, unlockCancelledOrNoShow: true);
					if (!updateResult.IsSuccess)
					{
						if (updateResult.ValidationErrors.Count > 0)
						{
							editDialog.ApplyValidationErrors(updateResult.ValidationErrors);
						}
						else
						{
							MessageBox.Show(
								updateResult.Message ?? "Failed to unlock and save reservation.",
								"Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
						return;
					}

					mainWindow.DialogHost.Close(editDialog, true);
					await LoadTodayReservationsAsync(updatedReservation.Id);
				};

				mainWindow.DialogHost.Show(editDialog);
			};

			mainWindow.DialogHost.Show(unlockDialog);
		}


		// EVENT HANDLERS:
		private async void TodayView_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= TodayView_Loaded;

			try
			{
				await LoadConfigurationAsync();
				await LoadTodayReservationsAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to load today's reservations.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		private void ReservationsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_isHandlingStatusChange)
			{
				return;
			}

			_selectedReservationId = (ReservationsTable.SelectedItem as TodayReservationRow)?.Id;
		}

		private void ReservationsTable_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.OriginalSource is DependencyObject dep)
			{
				var row = FindVisualParent<DataGridRow>(dep);
				if (row is not null && row.IsSelected && FindVisualParent<ComboBox>(dep) is null)
				{
					e.Handled = true;
					ReservationsTable.SelectedItem = null;
					_selectedReservationId = null;
				}
			}
		}

		private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
		{
			DependencyObject? current = child;
			while (current is not null)
			{
				if (current is T match)
				{
					return match;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			return null;
		}

		private async void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_isHandlingStatusChange)
			{
				return;
			}

			if (sender is not ComboBox comboBox || comboBox.DataContext is not TodayReservationRow row)
			{
				return;
			}

			if (comboBox.SelectedItem is not StatusOptionItem chosenOption)
			{
				return;
			}

			if (chosenOption.IsUnlockAction)
			{
				_isHandlingStatusChange = true;
				comboBox.SelectedItem = row.SelectedOption;
				_isHandlingStatusChange = false;

				OpenUnlockDialog(row);
				return;
			}

			if (!chosenOption.TargetStatus.HasValue || chosenOption.TargetStatus.Value == row.Status)
			{
				return;
			}

			ReservationStatus previousStatus = row.Status;
			ReservationStatus newStatus = chosenOption.TargetStatus.Value;

			bool isCurrentActive = previousStatus is ReservationStatus.Expected or ReservationStatus.Late or ReservationStatus.Arrived;
			bool isTargetInactive = newStatus is ReservationStatus.Cancelled or ReservationStatus.NoShow;

			if (isCurrentActive && isTargetInactive)
			{
				var mainWindow = GetMainWindow();
				if (mainWindow is null)
				{
					return;
				}

				var confirmDialog = new ChangeStatusDialog();
				confirmDialog.SetTargetStatus(newStatus);

				_isHandlingStatusChange = true;
				comboBox.SelectedItem = row.SelectedOption;
				_isHandlingStatusChange = false;

				confirmDialog.CancelRequested += (_, _) =>
				{
					mainWindow.DialogHost.Close(confirmDialog, false);
				};

				confirmDialog.ConfirmRequested += async (_, _) =>
				{
					mainWindow.DialogHost.Close(confirmDialog, true);
					await PersistStatusChangeAsync(row.Id, newStatus);
				};

				mainWindow.DialogHost.Show(confirmDialog);
				return;
			}

			bool saved = await PersistStatusChangeAsync(row.Id, newStatus);
			if (!saved)
			{
				_isHandlingStatusChange = true;
				comboBox.SelectedItem = row.SelectedOption;
				_isHandlingStatusChange = false;
			}
		}
	}
}