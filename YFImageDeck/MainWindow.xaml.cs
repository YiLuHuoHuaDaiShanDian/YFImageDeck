using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace YFImageDeck
{
    public partial class MainWindow : Window
    {
        private const double ZoomFactor = 1.1; // 每次缩放的倍率
        private const double MinZoom = 0.5;    // 最小缩放比例
        private const double MaxZoom = 5.0;   // 最大缩放比例

        private ScaleTransform scaleTransform;  // Image 组件缩放变换
        private TranslateTransform translateTransform;  // Image 组件平移变换

        private Point _lastMousePosition;      // 记录上次鼠标位置
        private bool _isDragging;             // 是否正在拖动图片

        public MainWindow()
        {
            // 初始化组件
            InitializeComponent();
            scaleTransform = (ScaleTransform)((TransformGroup)MainImage.RenderTransform).Children[0];
            translateTransform = (TranslateTransform)((TransformGroup)MainImage.RenderTransform).Children[1];

            // 获取命令行参数
            string[] args = Environment.GetCommandLineArgs();
#if DEBUG
            args = new string[] { args[0], @"C:\Users\Administrator\Desktop\untitled.png" };
#endif

            // 如果有文件路径参数，加载对应的图片
            if (args.Length > 1 && File.Exists(args[1]))
            {
                LoadImage(args[1]);
            }
            else
            {
                MessageBox.Show("未指定图片路径或图片不存在。请通过右键图片并选择此程序打开。",
                                "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                MainImage.Source = bitmap; // 设置 Image 控件的 Source
                this.Title = $"YFImageDeck - {Path.GetFileName(imagePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法加载图片: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var mousePosition = e.GetPosition(MainImage);

            // 获取当前的缩放和平移变换

            // 计算缩放倍率
            double scale = e.Delta > 0 ? ZoomFactor : 1 / ZoomFactor;
            double newScale = scaleTransform.ScaleX * scale;
            if (newScale < MinZoom) newScale = MinZoom;
            if (newScale > MaxZoom) newScale = MaxZoom;

            // 应用新的缩放比例
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            if (scaleTransform.ScaleX > 1)
            {
                // 使Image以鼠标为锚点缩放
                double relativeX = mousePosition.X / MainImage.ActualWidth;
                double relativeY = mousePosition.Y / MainImage.ActualHeight;

                // 计算鼠标位置对应的平移量
                double offsetX = relativeX * MainImage.ActualWidth;
                double offsetY = relativeY * MainImage.ActualHeight;

                // 调整平移以实现以鼠标为中心的缩放
                translateTransform.X -= mousePosition.X * (scale - 1);
                translateTransform.Y -= mousePosition.Y * (scale - 1);
            }
            else
            {
                // 始终保证Image在窗口正中央
                translateTransform.X = 0;
                translateTransform.Y = 0;
            }
        }

        // 鼠标左键按下 - 开始拖动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 图片必须要比窗口大才能拖动
            if (scaleTransform.ScaleX > 1)
            {
                _isDragging = true; // 标记拖动状态
                _lastMousePosition = e.GetPosition(this); // 记录鼠标当前位置
                MainImage.CaptureMouse(); // 捕获鼠标，确保拖动过程中的事件
            }
        }

        // 鼠标移动 - 拖动图片
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var currentMousePosition = e.GetPosition(this); // 获取当前鼠标位置

                // 更新图片的平移量
                translateTransform.X += currentMousePosition.X - _lastMousePosition.X;
                translateTransform.Y += currentMousePosition.Y - _lastMousePosition.Y;

                // 更新记录的鼠标位置
                _lastMousePosition = currentMousePosition;
            }
        }

        // 鼠标左键释放 - 停止拖动
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false; // 取消拖动状态
            MainImage.ReleaseMouseCapture(); // 释放鼠标捕获
        }
    }
}
