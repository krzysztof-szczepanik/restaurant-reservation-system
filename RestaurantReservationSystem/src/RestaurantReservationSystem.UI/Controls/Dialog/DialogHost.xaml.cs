using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RestaurantReservationSystem.UI.Controls {
    public partial class DialogHost : UserControl {

        // FIELDS:
        private readonly Stack<Action<bool?>?> closeCallbacks = new();


        // CONSTRUCTORS:
        public DialogHost() {
            InitializeComponent();
        }


        // METHODS:
        public void Show(UIElement content, Action<bool?>? onClosed = null) {
            var layer = new Grid {
                Background = (Brush)FindResource("DialogOverlay")
            };

            var dialogContainer = new ContentControl {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(24),
                Focusable = false
            };

            // Add dialog to layer
            layer.Children.Add(dialogContainer);

            // Disable previous layer
            if (DialogLayers.Children.Count > 0) {
                DialogLayers.Children[DialogLayers.Children.Count - 1].IsEnabled = false;
            }

            // Configure keyboard navigation
            KeyboardNavigation.SetTabNavigation(layer, KeyboardNavigationMode.Cycle);

            // Show new layer
            DialogLayers.Children.Add(layer);
            closeCallbacks.Push(onClosed);

            Visibility = Visibility.Visible;

            content.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        public void Close(UIElement content, bool? result) {
            if (DialogLayers.Children.Count == 0) { return; }

            var layer = (Grid)DialogLayers.Children[DialogLayers.Children.Count - 1];
            var container = (ContentControl)layer.Children[0];

            if (!ReferenceEquals(container.Content, content)) { return; }

            Close(result);
        }

        public void Close(bool? result = null) {
            if (DialogLayers.Children.Count == 0) {
                return;
            }

            // Remove current layer
            DialogLayers.Children.RemoveAt(DialogLayers.Children.Count - 1);

            Action<bool?>? callback = closeCallbacks.Pop();

            // Hide host if empty
            if (DialogLayers.Children.Count == 0) {
                Visibility = Visibility.Collapsed;
            }

            // Restore previous layer
            if (DialogLayers.Children.Count > 0) {
                var topLayer = (Grid)DialogLayers.Children[DialogLayers.Children.Count - 1];

                topLayer.IsEnabled = true;
                topLayer.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }

            // Invoke close callback
            callback?.Invoke(result);
        }

        public void CloseAll() {
            DialogLayers.Children.Clear();
            closeCallbacks.Clear();

            Visibility = Visibility.Collapsed;
        }
    }
}