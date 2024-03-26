using System.Windows;
using System.Windows.Controls;

namespace DEModLauncher_GUI.View;

public partial class FileDragControl : UserControl
{
    public event DragEventHandler? FileDragged;

    public FileDragControl()
    {
        InitializeComponent();
    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        FileDragged?.Invoke(this, e);
    }
}
