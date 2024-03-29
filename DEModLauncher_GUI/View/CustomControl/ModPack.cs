﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace DEModLauncher_GUI.View;

internal class ModPack : Control
{
    public static readonly DependencyProperty PackNameProperty =
        DependencyProperty.Register(nameof(PackName), typeof(string), typeof(ModPack), new PropertyMetadata(""));

    public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(ModPack), new PropertyMetadata(""));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(Status), typeof(ModPack), new PropertyMetadata(Status.Disable));

    public event EventHandler<DataDragDropEventArgs>? DataDragDrop;

    public string PackName
    {
        get
        {
            return (string)GetValue(PackNameProperty);
        }
        set
        {
            SetValue(PackNameProperty, value);
        }
    }

    public string ImagePath
    {
        get
        {
            return (string)GetValue(ImagePathProperty);
        }
        set
        {
            SetValue(ImagePathProperty, value);
        }
    }

    public Status Status
    {
        get { return (Status)GetValue(StatusProperty); }
        set { SetValue(StatusProperty, value); }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        DragOver += ModPackButton_DragOver;
        DragLeave += ModPackButton_DragLeave;
        Drop += ModPackButton_Drop;
    }

    #region NonPublic
    private void ModPackButton_DragOver(object sender, DragEventArgs e)
    {
        Point relatePos = e.GetPosition(this);
        if (relatePos.X <= ActualWidth / 2)
        {
            ShowTipBorder(Direction.Left);
        }
        else
        {
            ShowTipBorder(Direction.Right);
        }
    }
    private void ModPackButton_DragLeave(object sender, DragEventArgs e)
    {
        ResetTipBorder();
    }
    private void ModPackButton_Drop(object sender, DragEventArgs e)
    {
        Point relatePos = e.GetPosition(this);
        if (relatePos.X <= ActualWidth / 2)
        {
            DataDragDrop?.Invoke(this, new DataDragDropEventArgs(Direction.Left, e.Data));
        }
        else
        {
            DataDragDrop?.Invoke(this, new DataDragDropEventArgs(Direction.Right, e.Data));
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
                thickness.Top = 5;
                break;
            case Direction.Down:
                thickness.Bottom = 5;
                break;
            case Direction.Left:
                thickness.Left = 5;
                break;
            case Direction.Right:
                thickness.Right = 5;
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
    static ModPack()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ModPack), new FrameworkPropertyMetadata(typeof(ModPack)));
    }
    #endregion
}
