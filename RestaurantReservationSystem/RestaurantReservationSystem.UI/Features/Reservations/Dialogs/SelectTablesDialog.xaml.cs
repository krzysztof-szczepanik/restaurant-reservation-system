using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class SelectTablesDialog : UserControl
	{
		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;

		private readonly List<TableItem> _tables = new();
		private readonly HashSet<string> _collapsedZones = new();

		private int _guests;
		private string _reservationDate = string.Empty;
		private string _reservationTime = string.Empty;

		public IReadOnlyList<int> SelectedTableIds =>
			_tables.Where(t => t.IsSelected).Select(t => t.Id).ToList();

		public int SelectedTablesCount =>
			_tables.Count(t => t.IsSelected);

		public int TotalSeats =>
			_tables.Where(t => t.IsSelected).Sum(t => t.Seats);

		public SelectTablesDialog()
		{
			InitializeComponent();
			LoadSampleTables();
			SetReservationDetails("23.10.2026", "18:30", 12);
			RenderTables();
			UpdateSummary();
		}

		public void SetReservationDetails(string date, string time, int guests)
		{
			_guests = guests;
			_reservationDate = date;
			_reservationTime = time;

			ReservationDateText.Text = $"{date} · {time}";
			GuestsText.Text = $"Guests: {guests}";
			UpdateSummary();
		}

		private void LoadSampleTables()
		{
			_tables.Clear();
			_tables.Add(new TableItem(1, "Main Hall", 1, 4, true, true));
			_tables.Add(new TableItem(2, "Main Hall", 2, 6, false, true));
			_tables.Add(new TableItem(3, "Terrace", 3, 4, true, false));
			_tables.Add(new TableItem(4, "Terrace", 4, 2, false, false));
			_tables.Add(new TableItem(5, "No zone", 5, 4, true, false));
		}

		private void RenderTables()
		{
			ZonesPanel.Children.Clear();
			string search = SearchBox?.Text?.Trim() ?? string.Empty;

			var filteredTables = _tables
				.Where(table =>
					string.IsNullOrWhiteSpace(search) ||
					GetTableLabel(table).Contains(search, StringComparison.OrdinalIgnoreCase))
				.GroupBy(table => table.Zone);

			foreach (var zone in filteredTables)
			{
				var zonePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };

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
					Data = Geometry.Parse(_collapsedZones.Contains(zone.Key) ? "M1 1 L5 5 L9 1" : "M1 5 L5 1 L9 5"),
					Stroke = (Brush)FindResource("SecondaryText"),
					StrokeThickness = 2,
					Width = 10,
					Height = 6,
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
					Visibility = _collapsedZones.Contains(zone.Key) ? Visibility.Collapsed : Visibility.Visible
				};

				foreach (var table in zone)
				{
					tablesPanel.Children.Add(CreateTableRow(table));
				}

				string zoneKey = zone.Key;
				zoneHeader.Click += (_, _) => {
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
			if (!table.IsAvailable && table.IsSelected)
			{
				var border = new Border
				{
					Background = new SolidColorBrush(Color.FromArgb(255, 254, 242, 242)), 
					BorderBrush = new SolidColorBrush(Color.FromArgb(255, 254, 202, 202)),
					BorderThickness = new Thickness(1),
					CornerRadius = new CornerRadius(4),
					Padding = new Thickness(8, 6, 8, 6),
					Margin = new Thickness(0, 0, 0, 8)
				};

				var grid = new Grid();
				var checkBox = new CheckBox
				{
					Content = GetTableLabel(table),
					IsChecked = true,
					FontSize = 13,
					Foreground = (Brush)FindResource("Danger"),
					VerticalContentAlignment = VerticalAlignment.Center
				};

				checkBox.Unchecked += (_, _) => {
					table.IsSelected = false;
					RenderTables();
					UpdateSummary();
				};

				var unavailableText = new TextBlock
				{
					Text = "Unavailable",
					FontSize = 12,
					Foreground = (Brush)FindResource("Danger"),
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center
				};

				grid.Children.Add(checkBox);
				grid.Children.Add(unavailableText);
				border.Child = grid;
				return border;
			}

			if (!table.IsAvailable)
			{
				var unavailableRow = new Grid { Margin = new Thickness(0, 0, 0, 8) };
				var tableName = new TextBlock
				{
					Text = GetTableLabel(table),
					FontSize = 13,
					Foreground = (Brush)FindResource("SecondaryText"),
					VerticalAlignment = VerticalAlignment.Center
				};
				var unavailableText = new TextBlock
				{
					Text = "Unavailable",
					FontSize = 12,
					Foreground = (Brush)FindResource("SecondaryText"),
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center
				};

				unavailableRow.Children.Add(tableName);
				unavailableRow.Children.Add(unavailableText);
				return unavailableRow;
			}

			var standardCheckBox = new CheckBox
			{
				Content = GetTableLabel(table),
				IsChecked = table.IsSelected,
				FontSize = 13,
				Foreground = (Brush)FindResource("PrimaryText"),
				Margin = new Thickness(0, 0, 0, 8),
				VerticalContentAlignment = VerticalAlignment.Center
			};

			standardCheckBox.Checked += (_, _) => {
				table.IsSelected = true;
				UpdateSummary();
			};

			standardCheckBox.Unchecked += (_, _) => {
				table.IsSelected = false;
				UpdateSummary();
			};

			return standardCheckBox;
		}

		private string GetTableLabel(TableItem table)
		{
			return table.Zone == "No zone"
				 ? $"Table {table.Number} - {table.Seats} seats"
				 : $"{table.Zone}: Table {table.Number} - {table.Seats} seats";
		}

		private void UpdateSummary()
		{
			if (SelectionSummaryText == null) return;

			SelectionSummaryText.Text = $"Selected tables: {SelectedTablesCount} | Total seats: {TotalSeats}";

			if (CapacityWarning != null)
			{
				CapacityWarning.Visibility = _guests > TotalSeats ? Visibility.Visible : Visibility.Hidden;
			}

			bool hasConflicts = _tables.Any(t => t.IsSelected && !t.IsAvailable);
			if (UnavailableWarning != null)
			{
				UnavailableWarning.Visibility = hasConflicts ? Visibility.Visible : Visibility.Collapsed;
			}

			if (ConfirmButton != null)
			{
				ConfirmButton.IsEnabled = !hasConflicts;
			}
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (ZonesPanel == null) return;
			RenderTables();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			if (_tables.Any(t => t.IsSelected && !t.IsAvailable)) return;
			ConfirmRequested?.Invoke(this, EventArgs.Empty);
		}

		private class TableItem
		{
			public int Id { get; }
			public string Zone { get; }
			public int Number { get; }
			public int Seats { get; }
			public bool IsAvailable { get; }
			public bool IsSelected { get; set; }

			public TableItem(int id, string zone, int number, int seats, bool isAvailable, bool isSelected)
			{
				Id = id;
				Zone = zone;
				Number = number;
				Seats = seats;
				IsAvailable = isAvailable;
				IsSelected = isSelected;
			}
		}
	}
}