using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEModLauncher_GUI.View {
    public class ModResource : Control {
        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register(nameof(ResourceName), typeof(string), typeof(ModResource), new PropertyMetadata(""));
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(Status), typeof(ModResource), new PropertyMetadata(Status.Disable));

        public event EventHandler<DataDragDropEventArgs> DataDragDrop;

        public string ResourceName {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }
        public Status Status {
            get { return (Status)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public override void OnApplyTemplate() {
            DragOver += ModResource_DragOver;
            DragLeave += ModResource_DragLeave;
            Drop += ModResource_Drop;
        }

        private void ModResource_DragOver(object sender, DragEventArgs e) {
            var relatePos = e.GetPosition(this);
            if (relatePos.Y <= ActualHeight / 2) {
                ShowTipBorder(Direction.Up);
            }
            else {
                ShowTipBorder(Direction.Down);
            }
        }
        private void ModResource_DragLeave(object sender, DragEventArgs e) {
            ResetTipBorder();
        }
        private void ModResource_Drop(object sender, DragEventArgs e) {
            var relatePos = e.GetPosition(this);
            if (relatePos.Y <= ActualHeight / 2) {
                DataDragDrop?.Invoke(this, new DataDragDropEventArgs(Direction.Up, e.Data));
            }
            else {
                DataDragDrop?.Invoke(this, new DataDragDropEventArgs(Direction.Down, e.Data));
            }
            ResetTipBorder();
        }
        private void ShowTipBorder(Direction direction) {
            var tipBorder = Template.FindName("PART_TipBorder", this) as Border;
            Thickness thickness = new Thickness(0);
            switch (direction) {
                case Direction.Up:
                    thickness.Top = 3;
                    break;
                case Direction.Down:
                    thickness.Bottom = 3;
                    break;
                case Direction.Left:
                    thickness.Left = 3;
                    break;
                case Direction.Right:
                    thickness.Right = 3;
                    break;
                default:
                    break;
            }
            tipBorder.BorderThickness = thickness;
        }
        private void ResetTipBorder() {
            var tipBorder = Template.FindName("PART_TipBorder", this) as Border;
            tipBorder.BorderThickness = new Thickness(0);
        }
        static ModResource() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModResource), new FrameworkPropertyMetadata(typeof(ModResource)));
        }
    }
}
