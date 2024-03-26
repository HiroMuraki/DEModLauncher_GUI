using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DEModLauncher_GUI.View;

public partial class DEModPackSetter : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string PackName
    {
        get
        {
            return _packName;
        }
        set
        {
            _packName = value;
            OnPropertyChanged(nameof(PackName));
        }
    }

    public string Description
    {
        get
        {
            return _description;
        }
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    public string ImagePath
    {
        get
        {
            return _imagePath;
        }
        set
        {
            _imagePath = value;
            OnPropertyChanged(nameof(ImagePath));
        }
    }

    public DEModPackSetter()
    {
        InitializeComponent();
    }

    #region NonPublic
    private static string _preImagesDirectory = DOOMEternal.ModPackImagesDirectory;
    private string _imagePath = DOOMEternal.DefaultModPackImage;
    private string _description = "";
    private string _packName = "";
    private void ChangeImage_Click(object sender, MouseButtonEventArgs e)
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "图像文件|*.jpg;*.png;*.bmp;*.gif|JPG图片|*.jpg|PNG图片|*.png|BMP图片|*.bmp|GIF图片|*.gif";
        ofd.InitialDirectory = _preImagesDirectory;
        ofd.Title = "选择模组图片";
        if (ofd.ShowDialog() == true)
        {
            try
            {
                ImagePath = ofd.FileName;
                _preImagesDirectory = Path.GetDirectoryName(ofd.FileName) ?? _preImagesDirectory;
            }
            catch (Exception exp)
            {
                MessageBox.Show($"图片修改出错，原因：{exp.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
