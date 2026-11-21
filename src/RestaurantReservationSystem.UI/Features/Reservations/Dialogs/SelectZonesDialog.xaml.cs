using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Reservations.Dialogs
{
	public partial class SelectZonesDialog : UserControl
	{

		// EVENTS:
		public event EventHandler? CancelRequested;
		public event EventHandler? ConfirmRequested;


		// FIELDS:
		private readonly List<ZoneItem> _zones = new();


		// PROPERTIES:
		public IReadOnlyList<string?> SelectedZones =>
			_zones
				.Where(z => z.IsSelected)
				.Select(z => z.IsNoZone ? null : z.Name)
				.ToList();


		// CONSTRUCTORS:
		public SelectZonesDialog()
		{
			InitializeComponent();
		}


		// METHODS:
		public async Task LoadZonesAsync(IEnumerable<string?>? preselectedZones = null)
		{
			var preselected = (preselectedZones ?? [])
				.Select(z => string.IsNullOrWhiteSpace(z) || z.Trim().Equals("No zone", StringComparison.OrdinalIgnoreCase)
					? "No zone"
					: z.Trim())
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			using var scope = App.ServiceProvider.CreateScope();
			var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
			var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

			var config = await configurationService.GetAsync();
			bool zoneVisible = config?.TableZoneVisible ?? true;

			var tables = await tableService.GetAllAsync();

			var distinctZones = new List<ZoneItem>();
			bool includeNoZone = false;

			if (zoneVisible)
			{
				var namedZones = tables
					.Select(t => t.Zone?.Trim())
					.Where(z => !string.IsNullOrWhiteSpace(z) && !z.Equals("No zone", StringComparison.OrdinalIgnoreCase))
					.Select(z => z!)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(z => z, StringComparer.OrdinalIgnoreCase);

				foreach (var z in namedZones)
				{
					distinctZones.Add(new ZoneItem(z, isNoZone: false, isSelected: preselected.Contains(z)));
				}

				includeNoZone = tables.Any(t => string.IsNullOrWhiteSpace(t.Zone) || t.Zone.Trim().Equals("No zone", StringComparison.OrdinalIgnoreCase))
					|| preselected.Contains("No zone");
			}
			else
			{
				includeNoZone = true;
			}

			if (includeNoZone)
			{
				distinctZones.Add(new ZoneItem("No zone", isNoZone: true, isSelected: preselected.Contains("No zone")));
			}

			_zones.Clear();
			_zones.AddRange(distinctZones);

			RenderZones();
			UpdateSummary();
		}

		private void RenderZones()
		{
			if (ZonesListPanel == null) { return; }

			ZonesListPanel.Children.Clear();

			string filter = SearchBox?.Text?.Trim() ?? string.Empty;

			var visibleZones = _zones
				.Where(z => string.IsNullOrWhiteSpace(filter) || z.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
				.ToList();

			foreach (var zone in visibleZones)
			{
				var border = new Border
				{
					Background = zone.IsSelected ? new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF)) : Brushes.Transparent,
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

				checkBox.Checked += (_, _) =>
				{
					zone.IsSelected = true;
					border.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
					UpdateSummary();
				};

				checkBox.Unchecked += (_, _) =>
				{
					zone.IsSelected = false;
					border.Background = Brushes.Transparent;
					UpdateSummary();
				};

				border.Child = checkBox;
				ZonesListPanel.Children.Add(border);
			}
		}

		private void UpdateSummary()
		{
			if (SelectedZonesSummaryText == null) { return; }

			int count = _zones.Count(z => z.IsSelected);
			SelectedZonesSummaryText.Text = $"Selected zones: {count}";
		}


		// EVENT HANDLERS:
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


		// MODELS:
		private sealed class ZoneItem
		{
			public string Name { get; }
			public bool IsNoZone { get; }
			public bool IsSelected { get; set; }

			public ZoneItem(string name, bool isNoZone, bool isSelected)
			{
				Name = name;
				IsNoZone = isNoZone;
				IsSelected = isSelected;
			}
		}
	}
}