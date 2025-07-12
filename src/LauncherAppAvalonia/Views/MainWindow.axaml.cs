using Avalonia.Controls;
using Avalonia.Input;
using LauncherAppAvalonia.ViewModels;

namespace LauncherAppAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnItemListBoxItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        LauncherItemViewModel? itemVM = (sender as ListBox)?.SelectedItem as LauncherItemViewModel;
        if (itemVM == null)
            return;

        if (DataContext is MainWindowViewModel viewModel)
            viewModel.OpenItemCommand.Execute(itemVM);
    }
}