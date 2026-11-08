using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class SelectZonesDialog : UserControl
	{

		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;

		private readonly List<ZoneItem> _zones = new() {
			new("Main Hall", true),
			new("Terrace", false),
			new("No zone", false)
		};

		public IReadOnlyList<string> SelectedZones =>
			_zones.Where(z => z.IsSelected).Select(z => z.Name).ToList();

		public SelectZonesDialog()
		{
			InitializeComponent();
			RenderZones();
			UpdateSummary();
		}

		private void RenderZones()
		{
			ZonesPanel.Children.Clear();
			string search = SearchBox?.Text?.Trim() ?? string.Empty;

			var filtered = _zones.Where(z => string.IsNullOrWhiteSpace(search) || z.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

			foreach (var zone in filtered)
			{
				var border = new Border
				{
					Background = zone.IsSelected ? (Brush)FindResource("Surface") : Brushes.Transparent,
					CornerRadius = new CornerRadius(4),
					Padding = new Thickness(8, 8, 8, 8),
					Margin = new Thickness(0, 0, 0, 4)
				};

				var checkBox = new CheckBox
				{
					Content = zone.Name,
					IsChecked = zone.IsSelected,
					Foreground = (Brush)FindResource("PrimaryText"),
					FontSize = 13,
					VerticalContentAlignment = VerticalAlignment.Center
				};

				checkBox.Checked += (_, _) => {
					zone.IsSelected = true;
					RenderZones();
					UpdateSummary();
				};

				checkBox.Unchecked += (_, _) => {
					zone.IsSelected = false;
					RenderZones();
					UpdateSummary();
				};

				border.Child = checkBox;
				ZonesPanel.Children.Add(border);
			}
		}

		private void UpdateSummary()
		{
			int count = _zones.Count(z => z.IsSelected);
			SelectedZonesSummaryText.Text = $"Selected zones: {count}";
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			RenderZones();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			ConfirmRequested?.Invoke(this, EventArgs.Empty);
		}

		private class ZoneItem
		{
			public string Name { get; }
			public bool IsSelected { get; set; }

			public ZoneItem(string name, bool isSelected)
			{
				Name = name;
				IsSelected = isSelected;
			}
		}
	}
}