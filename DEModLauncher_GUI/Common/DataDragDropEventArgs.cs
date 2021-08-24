using System;
using System.Windows;

namespace DEModLauncher_GUI {
    public class DataDragDropEventArgs : EventArgs {
        public Direction Direction { get; }
        public IDataObject Data { get; }


        public DataDragDropEventArgs(Direction direction, IDataObject data) {
            Direction = direction;
            Data = data;
        }
    }
}
