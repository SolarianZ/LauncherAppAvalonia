using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels
{
    public class EditItemViewModel : ViewModelBase
    {
        private readonly DataService _dataService;
        private readonly ItemHandlerService _itemHandlerService;
        private readonly LocalizationService _localizationService;
        private readonly Window _parentWindow;

        private string _path = string.Empty;
        private string _name = string.Empty;
        private PathType _selectedType = PathType.File;
        private bool _isEditMode;
        private int _editingItemIndex = -1;
        private bool _isCommandTipVisible;

        public string Path
        {
            get => _path;
            set
            {
                if (SetProperty(ref _path, value))
                {
                    DetectItemType();
                    UpdateSaveButtonState();
                }
            }
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public PathType SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    UpdateCommandTipVisibility();
                }
            }
        }

        public bool IsCommandTipVisible
        {
            get => _isCommandTipVisible;
            set => SetProperty(ref _isCommandTipVisible, value);
        }

        /// <summary>
        /// 检查是否可以保存
        /// </summary>
        public bool CanSave => !string.IsNullOrEmpty(Path);

        public bool IsEditMode => _isEditMode;

        public Array PathTypes => Enum.GetValues(typeof(PathType));

        // 命令
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SelectFileCommand { get; }
        public ICommand SelectFolderCommand { get; }

        public EditItemViewModel(DataService dataService, ItemHandlerService itemHandlerService,
            LocalizationService localizationService, Window parentWindow)
        {
            _dataService = dataService;
            _itemHandlerService = itemHandlerService;
            _localizationService = localizationService;
            _parentWindow = parentWindow;

            // 初始化命令
            SaveCommand = new RelayCommand(SaveItem, CanSaveInternal);
            CancelCommand = new RelayCommand(Cancel);
            SelectFileCommand = new RelayCommand(SelectFile);
            SelectFolderCommand = new RelayCommand(SelectFolder);

            // 初始化UI状态
            UpdateCommandTipVisibility();
        }

        /// <summary>
        /// 设置编辑模式
        /// </summary>
        public void SetEditMode(LauncherItem item, int index)
        {
            _isEditMode = true;
            _editingItemIndex = index;

            Path = item.Path;
            SelectedType = item.Type;
            Name = item.Name ?? string.Empty;

            UpdateCommandTipVisibility();
        }

        private bool CanSaveInternal() => CanSave;

        /// <summary>
        /// 更新保存按钮状态
        /// </summary>
        private void UpdateSaveButtonState()
        {
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 更新命令提示可见性
        /// </summary>
        private void UpdateCommandTipVisibility()
        {
            IsCommandTipVisible = SelectedType == PathType.Command;
        }

        /// <summary>
        /// 检测项目类型
        /// </summary>
        private async void DetectItemType()
        {
            if (!string.IsNullOrWhiteSpace(Path))
            {
                PathType detectedType = _itemHandlerService.GetItemType(Path);
                SelectedType = detectedType;
            }
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        private async void SelectFile()
        {
            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "选择文件",
                AllowMultiple = false
            };

            var result = await _parentWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result.Count > 0)
            {
                Path = result[0].Path.LocalPath;
                SelectedType = PathType.File;
            }
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        private async void SelectFolder()
        {
            var folderPickerOptions = new FolderPickerOpenOptions
            {
                Title = "选择文件夹",
                AllowMultiple = false
            };

            var result = await _parentWindow.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);

            if (result.Count > 0)
            {
                Path = result[0].Path.LocalPath;
                SelectedType = PathType.Folder;
            }
        }

        /// <summary>
        /// 保存项目
        /// </summary>
        private void SaveItem()
        {
            if (string.IsNullOrWhiteSpace(Path))
                return;

            var item = new LauncherItem(
                Path,
                SelectedType,
                string.IsNullOrWhiteSpace(Name) ? null : Name
            );

            if (_isEditMode)
            {
                _dataService.UpdateItem(_editingItemIndex, item);
            }
            else
            {
                _dataService.AddItem(item);
            }

            // 关闭窗口
            _parentWindow.Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
            _parentWindow.Close();
        }
    }
}