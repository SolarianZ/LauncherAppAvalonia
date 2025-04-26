using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using LauncherAppAvalonia.ViewModels;

namespace LauncherAppAvalonia.Services
{
    /// <summary>
    /// Toast提示服务，用于显示临时弹出提示信息
    /// </summary>
    public class ToastService
    {
        private static Window? _ownerWindow;
        private static Control? _toastControl;
        private static DispatcherTimer? _timer;

        /// <summary>
        /// 初始化Toast服务
        /// </summary>
        public static void Initialize(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2000)
            };
            _timer.Tick += (s, e) =>
            {
                _timer?.Stop();
                HideToast();
            };
        }

        /// <summary>
        /// 显示Toast提示
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <param name="durationMs">显示时间(毫秒)</param>
        public static void Show(string message, int durationMs = 2000)
        {
            if (_ownerWindow == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                // 隐藏之前的Toast
                HideToast();

                // 创建新的Toast控件
                var toastBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 8),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 30),
                    Opacity = 0.9,
                    BoxShadow = new BoxShadows(new BoxShadow
                    {
                        OffsetX = 0,
                        OffsetY = 2,
                        Blur = 8,
                        Color = Color.FromArgb(128, 0, 0, 0)
                    })
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                toastBorder.Child = textBlock;

                // 添加到窗口中
                var overlay = new Panel
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Children = { toastBorder },
                    ZIndex = 1000,
                    IsHitTestVisible = false
                };

                if (_ownerWindow.Content is Panel mainPanel)
                {
                    mainPanel.Children.Add(overlay);
                    _toastControl = overlay;
                }
                else if (_ownerWindow.Content is Control control)
                {
                    var originalContent = control;
                    var newPanel = new Panel
                    {
                        Children = { originalContent, overlay }
                    };
                    _ownerWindow.Content = newPanel;
                    _toastControl = overlay;
                }

                // 设置定时器
                _timer?.Stop();
                _timer!.Interval = TimeSpan.FromMilliseconds(durationMs);
                _timer.Start();
            });
        }

        /// <summary>
        /// 隐藏Toast提示
        /// </summary>
        private static void HideToast()
        {
            if (_toastControl != null)
            {
                if (_toastControl.Parent is Panel panel)
                {
                    panel.Children.Remove(_toastControl);
                }
                _toastControl = null;
            }
        }

        /// <summary>
        /// 在应用关闭时释放资源
        /// </summary>
        public static void Dispose()
        {
            _timer?.Stop();
            _timer = null;
            _toastControl = null;
            _ownerWindow = null;
        }
    }
}