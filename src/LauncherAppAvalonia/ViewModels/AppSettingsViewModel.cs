using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels;

public partial class AppSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private AppSettings _appSettings;

    [ObservableProperty]
    private IBrush? _viewBackground;


    private readonly Action _onClose;
    private readonly Action<AppSettings> _onSave;


    // For Preview
    public AppSettingsViewModel() : this(new AppSettings(), () => { }, _ => { }) { }

    public AppSettingsViewModel(AppSettings appSettings, Action onClose, Action<AppSettings> onSave)
    {
        _appSettings = appSettings;

        _onClose = onClose;
        _onSave = onSave;
    }


    #region Command

    [RelayCommand]
    private void SaveSettings() => _onSave(AppSettings);

    [RelayCommand]
    private void CloseSettings() => _onClose();

    [RelayCommand]
    private void ClearItems()
    {
        // TODO
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        string dataFolder = DataService.UserDataFolder;
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        Utility.OpenFolder(dataFolder);
    }

    #endregion
}