using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace LauncherAppAvalonia.ViewModels;

public class OpenSettingsCommand : ICommand
{
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        FlyoutBase.ShowAttachedFlyout((parameter as Control)!);
    }
}