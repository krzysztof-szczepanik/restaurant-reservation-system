using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.DTOs;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class SelectTablesDialog : UserControl
	{

		// EVENTS:
		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;


		// FIELDS:
		private readonly List<TableItem> _tables = new();
		private readonly HashSet<string> _collapsedZones = new();
		private readonly bool _isFilterMode;

		private InitialConfiguration _configuration = new();
		private int _guests;


		// PROPERTIES:
		public IReadOnlyList<int> SelectedTableIds =>
			_tables.Where(table => table.IsSelected).Select(table => table.Id).ToList();

		public int SelectedTablesCount =>
			_tables.Count(table => table.IsSelected);

		public int TotalSeats =>
			_tables.Where(table => table.IsSelected).Sum(table => table.Seats);

		public IReadOnlyList<Table> SelectedTables =>
			_tables
				.Where(table => table.IsSelected)
				.Select(table => new Table
				{
					Id = table.Id,
					Zone = table.Zone == "No zone" ? null : table.Zone,
					Number = table.Number,
					Label = table.Label,
					Seats = table.Seats
				})
				.ToList();


		// CONSTRUCTORS:
		public SelectTablesDialog(bool isFilterMode = false)
		{
			_isFilterMode = isFilterMode;
			InitializeComponent();

			if (_isFilterMode)
			{
				ReservationDetailsGrid.Visibility = Visibility.Collapsed;
				CapacityWarningContainer.Visibility = Visibility.Collapsed;
			}

			RenderTables();
			UpdateSummary();
		}


		// METHODS:
		public void SetReservationDetails(string date, string time, int guests)
		{
			_guests = guests;

			string cleanDate = date.Trim();
			string cleanTime = time.Trim();

			if (!string.IsNullOrWhiteSpace(cleanDate) && !string.IsNullOrWhiteSpace(cleanTime))
			{
				ReservationDateText.Text = $"{cleanDate} · {cleanTime}";
			}
			else
			{
				ReservationDateText.Text = $"{cleanDate}{cleanTime}";
			}

			GuestsText.Text = $"Guests: {guests}";

			UpdateSummary();
		}

		public async Task LoadTablesAsync(
			DateOnly date,
			TimeOnly time,
			int guests,
			IEnumerable<int> preselectedTableIds,
			int? excludeReservationId = null,
			bool checkAvailability = true)
		{
			_guests = guests;
			var preselectedSet = preselectedTableIds.ToHashSet();

			using var scope = App.ServiceProvider.CreateScope();
			var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
			var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

			var config = await configurationService.GetAsync();
			if (config is not null)
			{
				_configuration = config;
			}

			var allTables = await tableService.GetAllAsync();
			var unavailableTableIds = new HashSet<int>();

			if (checkAvailability)
			{
				var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

				var criteria = new ReservationFilterCriteria
				{
					Date = new ValueFilter<DateOnly>
					{
						Mode = FilterValueMode.Exact,
						Exact = date
					}
				};

				var reservationsResult = await reservationService.GetFilteredAsync(criteria);

				if (reservationsResult.IsSuccess && reservationsResult.Data is not null)
				{
					foreach (var reservation in reservationsResult.Data)
					{
						if (excludeReservationId.HasValue && reservation.Id == excludeReservationId.Value)
						{
							continue;
						}

						bool isBlockingStatus = reservation.Status is ReservationStatus.Expected
							or ReservationStatus.Late
							or ReservationStatus.Arrived;

						if (isBlockingStatus && reservation.Time <= time)
						{
							foreach (var assignedTable in reservation.Tables)
							{
								unavailableTableIds.Add(assignedTable.Id);
							}
						}
					}
				}
			}

			_tables.Clear();

			foreach (var table in allTables)
			{
				string zoneName = FormatZoneName(table.Zone);
				bool isAvailable = !unavailableTableIds.Contains(table.Id);
				bool isSelected = preselectedSet.Contains(table.Id);

				_tables.Add(new TableItem(
					id: table.Id,
					zone: zoneName,
					number: table.Number,
					label: table.Label,
					seats: table.Seats,
					isAvailable: isAvailable,
					isSelected: isSelected));
			}

			RenderTables();
			UpdateSummary();
		}

		private string FormatZoneName(string? zone)
		{
			if (!_configuration.TableZoneVisible || string.IsNullOrWhiteSpace(zone))
			{
				return "No zone";
			}

			string trimmed = zone.Trim();
			return trimmed.Equals("No zone", StringComparison.OrdinalIgnoreCase)
				? "No zone"
				: trimmed;
		}

		private string FormatIdentifier(TableItem table)
		{
			string numberPart = _configuration.TableNumberVisible && table.Number.HasValue
				? table.Number.Value.ToString()
				: string.Empty;

			string labelPart = _configuration.TableLabelVisible && !string.IsNullOrWhiteSpace(table.Label)
				? table.Label!
				: string.Empty;

			string identifier = _configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber
				? $"{labelPart}{numberPart}"
				: $"{numberPart}{labelPart}";

			return string.IsNullOrWhiteSpace(identifier)
				? $"Table {table.Id}"
				: identifier;
		}

		private static int ZoneSortOrder(string zoneName)
		{
			return zoneName.Equals("No zone", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
		}

		private void RenderTables()
		{
			ZonesPanel.Children.Clear();

			string search = SearchBox?.Text?.Trim() ?? string.Empty;

			var filteredZones = _tables
				.Where(table =>
					string.IsNullOrWhiteSpace(search) ||
					GetTableLabel(table).Contains(search, StringComparison.OrdinalIgnoreCase))
				.GroupBy(table => table.Zone)
				.OrderBy(g => ZoneSortOrder(g.Key))
				.ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

			foreach (var zone in filteredZones)
			{
				var zonePanel = new StackPanel
				{
					Margin = new Thickness(0, 0, 0, 16)
				};

				var zoneHeader = new Button
				{
					Background = Brushes.Transparent,
					BorderThickness = new Thickness(0),
					Padding = new Thickness(0),
					HorizontalContentAlignment = HorizontalAlignment.Left,
					Cursor = System.Windows.Input.Cursors.Hand
				};

				var headerContent = new StackPanel
				{
					Orientation = Orientation.Horizontal,
					Margin = new Thickness(0, 0, 0, 10)
				};

				var arrow = new Path
				{
					Data = Geometry.Parse(_collapsedZones.Contains(zone.Key)
						? "M1 5 L5 1 L9 5"
						: "M1 1 L5 5 L9 1"),
					Stroke = (Brush)FindResource("SecondaryText"),
					StrokeThickness = 2,
					StrokeStartLineCap = PenLineCap.Round,
					StrokeEndLineCap = PenLineCap.Round,
					StrokeLineJoin = PenLineJoin.Round,
					Width = 10,
					Height = 8,
					Stretch = Stretch.Uniform,
					VerticalAlignment = VerticalAlignment.Center,
					Margin = new Thickness(0, 0, 8, 0)
				};

				var zoneName = new TextBlock
				{
					Text = zone.Key,
					FontWeight = FontWeights.SemiBold,
					FontSize = 13,
					Foreground = (Brush)FindResource("PrimaryText"),
					VerticalAlignment = VerticalAlignment.Center
				};

				headerContent.Children.Add(arrow);
				headerContent.Children.Add(zoneName);
				zoneHeader.Content = headerContent;

				var tablesPanel = new StackPanel
				{
					Margin = new Thickness(18, 0, 0, 0),
					Visibility = _collapsedZones.Contains(zone.Key)
						? Visibility.Collapsed
						: Visibility.Visible
				};

				foreach (var table in zone)
				{
					tablesPanel.Children.Add(CreateTableRow(table));
				}

				string zoneKey = zone.Key;

				zoneHeader.Click += (_, _) =>
				{
					if (!_collapsedZones.Add(zoneKey))
					{
						_collapsedZones.Remove(zoneKey);
					}

					RenderTables();
				};

				zonePanel.Children.Add(zoneHeader);
				zonePanel.Children.Add(tablesPanel);

				ZonesPanel.Children.Add(zonePanel);
			}
		}

		private FrameworkElement CreateTableRow(TableItem table)
		{
			if (!table.IsAvailable && !table.IsSelected)
			{
				var unavailableBorder = new Border
				{
					Background = Brushes.Transparent,
					CornerRadius = new CornerRadius(6),
					Padding = new Thickness(8, 6, 8, 6),
					Margin = new Thickness(0, 0, 0, 4)
				};

				var unavailableRow = new Grid();

				var disabledCheckBox = new CheckBox
				{
					Content = GetTableLabel(table),
					IsChecked = false,
					IsEnabled = false,
					FontSize = 13,
					Foreground = (Brush)FindResource("SecondaryText"),
					VerticalContentAlignment = VerticalAlignment.Center
				};

				var unavailableText = new TextBlock
				{
					Text = "Unavailable",
					FontSize = 12,
					Foreground = (Brush)FindResource("SecondaryText"),
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center
				};

				unavailableRow.Children.Add(disabledCheckBox);
				unavailableRow.Children.Add(unavailableText);
				unavailableBorder.Child = unavailableRow;

				return unavailableBorder;
			}

			var selectedBg = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
			var conflictBg = new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2));

			var rowBorder = new Border
			{
				Background = !table.IsAvailable && table.IsSelected
					? conflictBg
					: (table.IsSelected ? selectedBg : Brushes.Transparent),
				CornerRadius = new CornerRadius(6),
				Padding = new Thickness(8, 6, 8, 6),
				Margin = new Thickness(0, 0, 0, 4),
				Cursor = System.Windows.Input.Cursors.Hand
			};

			var rowGrid = new Grid();

			var checkBox = new CheckBox
			{
				Content = GetTableLabel(table),
				IsChecked = table.IsSelected,
				FontSize = 13,
				Foreground = (Brush)FindResource("PrimaryText"),
				VerticalContentAlignment = VerticalAlignment.Center
			};

			checkBox.Checked += (_, _) =>
			{
				table.IsSelected = true;
				rowBorder.Background = table.IsAvailable ? selectedBg : conflictBg;
				UpdateSummary();
			};

			checkBox.Unchecked += (_, _) =>
			{
				table.IsSelected = false;
				rowBorder.Background = Brushes.Transparent;
				if (!table.IsAvailable)
				{
					RenderTables();
				}
				UpdateSummary();
			};

			rowBorder.MouseLeftButtonDown += (_, e) =>
			{
				if (e.OriginalSource is CheckBox)
				{
					return;
				}
				checkBox.IsChecked = !(checkBox.IsChecked ?? false);
				e.Handled = true;
			};

			rowGrid.Children.Add(checkBox);

			if (!table.IsAvailable)
			{
				var unavailableBadge = new TextBlock
				{
					Text = "Unavailable",
					FontSize = 12,
					Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center
				};

				rowGrid.Children.Add(unavailableBadge);
			}

			rowBorder.Child = rowGrid;
			return rowBorder;
		}

		private string GetTableLabel(TableItem table)
		{
			string identifier = FormatIdentifier(table);
			return table.Zone == "No zone"
				? $"{identifier} - {table.Seats} seats"
				: $"{table.Zone}: {identifier} - {table.Seats} seats";
		}

		private void UpdateSummary()
		{
			if (SelectionSummaryText == null) { return; }

			SelectionSummaryText.Text = _isFilterMode
				? $"Selected tables: {SelectedTablesCount}"
				: $"Selected tables: {SelectedTablesCount} | Total seats: {TotalSeats}";

			if (!_isFilterMode && CapacityWarning != null)
			{
				CapacityWarning.Visibility = _guests > TotalSeats
					? Visibility.Visible
					: Visibility.Collapsed;
			}

			bool hasConflictingSelectedTable = _tables.Any(table => table.IsSelected && !table.IsAvailable);

			if (UnavailableWarning != null)
			{
				UnavailableWarning.Visibility = hasConflictingSelectedTable
					? Visibility.Visible
					: Visibility.Collapsed;
			}

			if (ConfirmButton != null)
			{
				ConfirmButton.IsEnabled = !hasConflictingSelectedTable;
			}
		}


		// EVENT HANDLERS:
		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (ZonesPanel == null) { return; }

			RenderTables();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			if (_tables.Any(table => table.IsSelected && !table.IsAvailable))
			{
				return;
			}

			ConfirmRequested?.Invoke(this, EventArgs.Empty);
		}


		// DATA MODELS:
		private class TableItem
		{
			public int Id { get; }
			public string Zone { get; }
			public int? Number { get; }
			public string? Label { get; }
			public int Seats { get; }
			public bool IsAvailable { get; }
			public bool IsSelected { get; set; }

			public TableItem(int id, string zone, int? number, string? label, int seats, bool isAvailable, bool isSelected)
			{
				Id = id;
				Zone = zone;
				Number = number;
				Label = label;
				Seats = seats;
				IsAvailable = isAvailable;
				IsSelected = isSelected;
			}
		}
	}
}