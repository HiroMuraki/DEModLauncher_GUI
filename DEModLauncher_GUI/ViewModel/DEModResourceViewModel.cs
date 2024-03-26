using System;

namespace DEModLauncher_GUI.ViewModel;

internal class DEModResourceViewModel : ViewModelBase, IComparable<DEModResourceViewModel>, IDeepCloneable<DEModResourceViewModel>
{
    public DEModResourceViewModel(string path)
    {
        _path = path;
        _status = Status.Enable;
        try
        {
            _information = DEModInfoViewModel.Read($"{DOOMEternal.ModPacksDirectory}\\{path}");
        }
        catch
        {
            _information = new DEModInfoViewModel();
        }
    }

    public string Name
    {
        get
        {
            if (_path.EndsWith(".zip"))
            {
                return _path[0..(_path.Length - 4)];
            }
            return _path;
        }
    }

    public string Path
    {
        get
        {
            return _path;
        }
        set
        {
            _path = value;
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(Name));
            try
            {
                _information = DEModInfoViewModel.Read($"{DOOMEternal.ModPacksDirectory}\\{_path}");
            }
            catch (Exception)
            {
                _information = new DEModInfoViewModel();
            }
            OnPropertyChanged(nameof(Information));
        }
    }

    public Status Status
    {
        get
        {
            return _status;
        }
    }

    public DEModInfoViewModel Information
    {
        get
        {
            return _information;
        }
    }

    public void Toggle()
    {
        if (_status == Status.Disable)
        {
            _status = Status.Enable;
        }
        else
        {
            _status = Status.Disable;
        }
        OnPropertyChanged(nameof(Status));
    }

    public DEModResourceViewModel GetDeepClone()
    {
        return new DEModResourceViewModel
        {
            _path = _path,
            _status = _status,
            _information = _information.GetDeepClone()
        };
    }

    public override string ToString()
    {
        return Name;
    }

    public int CompareTo(DEModResourceViewModel? other)
    {
        return _path.CompareTo(other?._path);
    }

    #region NonPublic
    private DEModResourceViewModel() { }
    private string _path = "";
    private Status _status = Status.Enable;
    private DEModInfoViewModel _information = new();
    #endregion
}
