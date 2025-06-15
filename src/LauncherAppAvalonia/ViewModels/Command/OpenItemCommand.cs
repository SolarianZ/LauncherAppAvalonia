using System;
using System.Windows.Input;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public class OpenItemCommand : ICommand
{
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        LauncherItemViewModel? launcherItemVM = parameter as LauncherItemViewModel;
        if (launcherItemVM == null)
        {
            Console.WriteLine($"Invalid command parameter: {parameter?.ToString() ?? "null"}");
            return;
        }

        Console.WriteLine($"TODO Open Item: [{launcherItemVM.Type}] {launcherItemVM.Path}");
    }
}