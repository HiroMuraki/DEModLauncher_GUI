using System;
using System.Windows;
using System.Windows.Controls;

namespace DEModLauncher_GUI.View;

internal class ModResource : Control
{
    public static readonly DependencyProperty ResourceNameProperty =
        DependencyProperty.Register(nameof(ResourceName), typeof(string), typeof(ModResource), new PropertyMetadata(""));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(Status), typeof(ModResource), new PropertyMetadata(Status.Disable));

    public event EventHandler<DataDragDropEventArgs>? DataDragDrop;

    public string ResourceName
    {
        get { return (string)GetValue(ResourceNameProperty); }
        set { SetValue(ResourceNameProperty, value); }
    }

    public Status Status
    {
        get { return (Status)GetValue(StatusProperty); }
        set { SetValue(StatusProperty, value); }
    }

    public override void OnApplyTemplate()
    {
        DragOver += ModResource_DragOver;
        DragLeave += ModResource_DragLeave;
        Drop += ModResource_Drop;
    }

    #region NonPublic
    private void ModResource_DragOver(object sender, DragEventArgs e)
    {
        Point relatePos = e.GetPosition(this);
        if (relatePos.Y <= ActualHeight / 2)
        {
            ShowTipBorder(Direction.Up);
        }
        else
        {
            ShowTipBorder(Direction.Down);
        }
    }
    private void ModResource_DragLeave(object sender, DragEventArgs e)
    {
        ResetTipBorder();
    }
    private void ModResource_Drop(object sender, DragEventArgs e)
    {
        Point relatePos = e.GetPosition(this);
        if (relatePos.Y <= ActualHeight / 2)
        {
            DataDragDrop?.Invoke(this, new DataDragDropEventArgs(Direction.Up, e.Data));
        }
        else
        {
            DataDragDrop?.Invoke(this, new DataDragDropEventArgs(Direction.Down, e.Data));
        }
        ResetTipBorder();
    }
    private void ShowTipBorder(Direction direction)
    {
        var tipBorder = (Border)Template.FindName("PART_TipBorder", this);
        var thickness = new Thickness(0);
        switch (direction)
        {
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
    private void ResetTipBorder()
    {
        ((Border)Template.FindName("PART_TipBorder", this)).BorderThickness = new Thickness(0);
    }
    static ModResource()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ModResource), new FrameworkPropertyMetadata(typeof(ModResource)));
    }
    #endregion
}
