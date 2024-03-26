using System;
using System.Windows;

namespace DEModLauncher_GUI;

internal class DataDragDropEventArgs : EventArgs
{
    public Direction Direction { get; }
    public IDataObject Data { get; }

    public DataDragDropEventArgs(Direction direction, IDataObject data)
    {
        Direction = direction;
        Data = data;
    }
}
