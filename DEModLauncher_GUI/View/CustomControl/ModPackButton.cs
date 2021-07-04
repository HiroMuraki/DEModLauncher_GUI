using System.Windows;
using System.Windows.Controls;

namespace DEModLauncher_GUI.View {
    public class ModPackButton : RadioButton {
        public static readonly DependencyProperty PackNameProperty =
            DependencyProperty.Register(nameof(PackName), typeof(string), typeof(ModPackButton), new PropertyMetadata(""));
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(ModPackButton), new PropertyMetadata(""));

        public string PackName {
            get {
                return (string)GetValue(PackNameProperty);
            }
            set {
                SetValue(PackNameProperty, value);
            }
        }
        public string ImagePath {
            get {
                return (string)GetValue(ImagePathProperty);
            }
            set {
                SetValue(ImagePathProperty, value);
            }
        }

        static ModPackButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModPackButton), new FrameworkPropertyMetadata(typeof(ModPackButton)));
        }
    }
}
