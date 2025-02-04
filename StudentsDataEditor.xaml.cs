using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StudentsDataEditor : Window
    {
        public bool Saved { get; set; } = false;
        public string STDFilePath { get; private set; } = string.Empty;
        public StudentsDataEditor()
        {
            this.InitializeComponent();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(600, 880));
            CenterWindow();
            this.AppWindow.Closing += AppWindow_Closing;
        }
        public StudentsDataEditor(string STDFilePath)
        {
            this.InitializeComponent();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(600, 880));
            CenterWindow();
            this.AppWindow.Closing += AppWindow_Closing;
            this.STDFilePath = STDFilePath;
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (Saved)
            {
                return;
            }
            else
            {
                args.Cancel = true; // 先取消关闭
                var currentPage = ContentFrame.Content as Page;
                ContentDialog dialog = new ContentDialog
                {
                    Title = "未保存的数据",
                    Content = "如果您不保存, 这些数据将会丢失。",
                    PrimaryButtonText = "返回保存",
                    SecondaryButtonText = "不保存离开",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = currentPage!.XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    this.Saved = true;
                    this.Close();   // 然后再强行关闭。。
                }
            }
        }

        private void CenterWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

            var size = this.AppWindow.Size;
            int x = (displayArea.WorkArea.Width - size.Width) / 2 + displayArea.WorkArea.X;
            int y = (displayArea.WorkArea.Height - size.Height) / 2 + displayArea.WorkArea.Y;

            this.AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
        }

        private void ContentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(StudentsDataEditorPage), this);    // 传入 this, 使得 StudentsDataEditorPage 可以访问本实例属性
        }
    }
}
