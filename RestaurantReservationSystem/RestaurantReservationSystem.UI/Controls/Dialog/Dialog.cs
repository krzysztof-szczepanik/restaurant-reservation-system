using System.Windows;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Controls {
    public class Dialog : Control {

        // PROPERTIES:

        // Header
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(Dialog), new PropertyMetadata(string.Empty));
        public string Header { get => (string)GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }

        // Content
        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register(nameof(DialogContent), typeof(object), typeof(Dialog), new PropertyMetadata(null));
        public object DialogContent { get => GetValue(DialogContentProperty); set => SetValue(DialogContentProperty, value); }

        // Footer left
        public static readonly DependencyProperty FooterLeftProperty = DependencyProperty.Register(nameof(FooterLeft), typeof(object), typeof(Dialog), new PropertyMetadata(null));
        public object FooterLeft { get => GetValue(FooterLeftProperty); set => SetValue(FooterLeftProperty, value); }

        // Footer right
        public static readonly DependencyProperty FooterRightProperty = DependencyProperty.Register(nameof(FooterRight), typeof(object), typeof(Dialog), new PropertyMetadata(null));
        public object FooterRight { get => GetValue(FooterRightProperty); set => SetValue(FooterRightProperty, value); }


        // CONSTRUCTORS:
        static Dialog() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dialog), new FrameworkPropertyMetadata(typeof(Dialog)));
        }
    }
}