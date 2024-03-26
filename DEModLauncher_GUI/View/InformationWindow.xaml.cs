using System.ComponentModel;
using System.Windows;

namespace DEModLauncher_GUI.View;

public partial class InformationWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Message
    {
        get
        {
            return _message.Text;
        }
        set
        {
            _message.Text = value;
            OnPropertyChanged(nameof(Message));
        }
    }

    public string Caption
    {
        get
        {
            return Title;
        }
        set
        {
            Title = value;
        }
    }

    public InformationWindow(string message, string caption)
    {
        InitializeComponent();
        Caption = caption;
        Message = message;
    }

    public static void Show(string message, string caption, Window parentWindow)
    {
        ShowCore(message, caption, parentWindow);
    }

    #region NonPublic
    private static void ShowCore(string message, string caption, Window parentWindow)
    {
        var window = new InformationWindow(message, caption);
        if (parentWindow != null)
        {
            window.Owner = parentWindow;
        }
        window.Show();
    }
    #endregion
}
