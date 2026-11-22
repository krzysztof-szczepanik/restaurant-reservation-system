using Microsoft.Extensions.DependencyInjection;
using RestaurantReservationSystem.Application.Interfaces;
using RestaurantReservationSystem.Core.Enums;
using RestaurantReservationSystem.Core.Models;
using RestaurantReservationSystem.UI.Features.Tables.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Features.Tables
{
	public partial class TablesView : UserControl
	{

		// VIEW MODEL ROW:
		private sealed class TableRowItem
		{
			public required Table Source { get; init; }
			public int Id => Source.Id;
			public string NumberDisplay => Source.Number?.ToString() ?? string.Empty;
			public string LabelDisplay => Source.Label ?? string.Empty;
			public string ZoneDisplay => string.IsNullOrWhiteSpace(Source.Zone) ? "No zone" : Source.Zone;
			public int Seats => Source.Seats;
		}


		// FIELDS:
		private InitialConfiguration _configuration = new();


		// PROPERTIES:
		private ObservableCollection<TableRowItem> TableRows { get; } = new();


		// CONSTRUCTORS:
		public TablesView()
		{
			InitializeComponent();

			TablesDataGrid.ItemsSource = TableRows;
			TablesDataGrid.SelectionChanged += TablesDataGrid_SelectionChanged;
			TablesDataGrid.PreviewMouseLeftButtonDown += TablesDataGrid_PreviewMouseLeftButtonDown;
			UpdateActionButtonsState();
			Loaded += TablesView_Loaded;
		}


		// METHODS:
		private MainWindow? GetMainWindow()
		{
			return Window.GetWindow(this) as MainWindow;
		}

		private void UpdateActionButtonsState()
		{
			bool hasSelection = TablesDataGrid.SelectedItem is TableRowItem;
			EditButton.IsEnabled = hasSelection;
			DeleteButton.IsEnabled = hasSelection;
		}

		private static DataGridRow? FindVisualParentRow(DependencyObject? child)
		{
			while (child is not null)
			{
				if (child is DataGridRow row)
				{
					return row;
				}
				child = VisualTreeHelper.GetParent(child);
			}
			return null;
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

			TablesDataGrid.Columns[0].Visibility = configuration.TableNumberVisible ? Visibility.Visible : Visibility.Collapsed;
			TablesDataGrid.Columns[1].Visibility = configuration.TableLabelVisible ? Visibility.Visible : Visibility.Collapsed;
			TablesDataGrid.Columns[2].Visibility = configuration.TableZoneVisible ? Visibility.Visible : Visibility.Collapsed;

			if (configuration.TableIdentifierOrder == TableIdentifierOrder.LabelThenNumber)
			{
				TablesDataGrid.Columns[0].DisplayIndex = 1;
				TablesDataGrid.Columns[1].DisplayIndex = 0;
			}
			else
			{
				TablesDataGrid.Columns[0].DisplayIndex = 0;
				TablesDataGrid.Columns[1].DisplayIndex = 1;
			}
		}

		private async Task LoadTablesAsync(int? selectTableId = null)
		{
			using var scope = App.ServiceProvider.CreateScope();
			var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

			var tables = await tableService.GetAllAsync();

			TableRows.Clear();
			foreach (var table in tables)
			{
				TableRows.Add(new TableRowItem { Source = table });
			}

			if (selectTableId.HasValue)
			{
				var target = TableRows.FirstOrDefault(t => t.Id == selectTableId.Value);
				if (target is not null)
				{
					TablesDataGrid.SelectedItem = target;
					TablesDataGrid.ScrollIntoView(target);
				}
			}

			UpdateActionButtonsState();
		}

		private List<string> GetAvailableZones()
		{
			return TableRows
				.Select(r => r.Source.Zone)
				.Where(z => !string.IsNullOrWhiteSpace(z) && !z.Trim().Equals("No zone", StringComparison.OrdinalIgnoreCase))
				.Select(z => z!.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(z => z, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}


		// EVENT HANDLERS:
		private async void TablesView_Loaded(object sender, RoutedEventArgs e)
		{
			Loaded -= TablesView_Loaded;

			try
			{
				await LoadConfigurationAsync();
				await LoadTablesAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to load tables.\n\n{ex.Message}",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		private void TablesDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var row = FindVisualParentRow(e.OriginalSource as DependencyObject);
			if (row is not null && row.IsSelected)
			{
				e.Handled = true;
				TablesDataGrid.SelectedItem = null;
				UpdateActionButtonsState();
			}
		}

		private void TablesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateActionButtonsState();
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			var mainWindow = GetMainWindow();
			if (mainWindow == null)
			{
				return;
			}

			var dialog = new AddEditTableDialog();
			dialog.ApplyConfiguration(_configuration, GetAvailableZones());

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.SaveRequested += async (_, _) =>
			{
				if (!dialog.ValidateForm(out var newTable) || newTable is null)
				{
					return;
				}

				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

					var result = await tableService.AddAsync(newTable);
					if (!result.IsSuccess)
					{
						if (result.ValidationErrors.Count > 0)
						{
							dialog.ApplyValidationErrors(result.ValidationErrors);
						}
						else
						{
							MessageBox.Show(
								result.Message ?? "Failed to add table.",
								"Validation Error",
								MessageBoxButton.OK,
								MessageBoxImage.Warning);
						}
						return;
					}

					await LoadTablesAsync(result.Data?.Id);
					mainWindow.DialogHost.Close(dialog, true);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"Failed to add table.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}

		private async void Edit_Click(object sender, RoutedEventArgs e)
		{
			if (TablesDataGrid.SelectedItem is not TableRowItem selectedRow)
			{
				return;
			}

			var mainWindow = GetMainWindow();
			if (mainWindow == null)
			{
				return;
			}

			Table? freshTable;
			using (var scope = App.ServiceProvider.CreateScope())
			{
				var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
				freshTable = await tableService.GetByIdAsync(selectedRow.Id);
			}

			var targetTable = freshTable ?? selectedRow.Source;

			var dialog = new AddEditTableDialog(
				targetTable.Id,
				targetTable.Number?.ToString() ?? string.Empty,
				targetTable.Label ?? string.Empty,
				string.IsNullOrWhiteSpace(targetTable.Zone) ? "No zone" : targetTable.Zone,
				targetTable.Seats.ToString());

			dialog.ApplyConfiguration(_configuration, GetAvailableZones());

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.SaveRequested += async (_, _) =>
			{
				if (!dialog.ValidateForm(out var updatedTable) || updatedTable is null)
				{
					return;
				}

				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

					var result = await tableService.UpdateAsync(updatedTable);
					if (!result.IsSuccess)
					{
						if (result.ValidationErrors.Count > 0)
						{
							dialog.ApplyValidationErrors(result.ValidationErrors);
						}
						else
						{
							MessageBox.Show(
								result.Message ?? "Failed to update table.",
								"Validation Error",
								MessageBoxButton.OK,
								MessageBoxImage.Warning);
						}
						return;
					}

					await LoadTablesAsync(updatedTable.Id);
					mainWindow.DialogHost.Close(dialog, true);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"Failed to update table.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}

		private void DeleteTab_Click(object sender, RoutedEventArgs e)
		{
			if (TablesDataGrid.SelectedItem is not TableRowItem selectedRow)
			{
				return;
			}

			var mainWindow = GetMainWindow();
			if (mainWindow == null)
			{
				return;
			}

			var dialog = new DeleteTablesDialog();

			dialog.CancelRequested += (_, _) => mainWindow.DialogHost.Close(dialog, false);

			dialog.DeleteRequested += async (_, _) =>
			{
				try
				{
					using var scope = App.ServiceProvider.CreateScope();
					var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

					var result = await tableService.DeleteAsync(selectedRow.Id);
					mainWindow.DialogHost.Close(dialog, result.IsSuccess);

					if (!result.IsSuccess)
					{
						var cannotDeleteDialog = new CannotDeleteTableDialog();
						cannotDeleteDialog.CloseRequested += (_, _) => mainWindow.DialogHost.Close(cannotDeleteDialog, false);
						mainWindow.DialogHost.Show(cannotDeleteDialog);
						return;
					}

					await LoadTablesAsync();
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						$"Failed to delete table.\n\n{ex.Message}",
						"Error",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			};

			mainWindow.DialogHost.Show(dialog);
		}
	}
}