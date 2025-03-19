using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using WinRT.Interop;
using static Randomly_NT.UpdateService;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private UpdateService? _updateInstance;
        public MainWindow()
        {
            this.InitializeComponent();
            ResizeWindow();

        }



        private void ResizeWindow()
        {
            // ��ȡ���ھ����AppWindow
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // ��ȡ��ǰ��ʾ������Ϣ
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;

            // Ŀ��ߴ�ͱ���
            const double targetWidth = 750;
            const double targetHeight = 950;
            double targetRatio = targetWidth / targetHeight;

            // �����������ߴ磨ռ������90%��
            double maxWidth = workArea.Width * 0.9;
            double maxHeight = workArea.Height * 0.9;

            // ������ѳߴ磨���ֿ�߱ȣ�
            double actualWidth = maxWidth;
            double actualHeight = actualWidth / targetRatio;

            if (actualHeight > maxHeight)
            {
                actualHeight = maxHeight;
                actualWidth = actualHeight * targetRatio;
            }

            // ���ô��ڳߴ�
            appWindow.Resize(new Windows.Graphics.SizeInt32(
                (int)actualWidth,
                (int)actualHeight));

            // ���д���
            appWindow.Move(new Windows.Graphics.PointInt32(
                workArea.X + (int)((workArea.Width - actualWidth) / 2),
                workArea.Y + (int)((workArea.Height - actualHeight) / 2)));
        }

        private void NavView_SelectionChanged(NavigationView sender,
                                      NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                Type navPageType = Type.GetType(args.SelectedItemContainer.Tag.ToString() ?? "RandomlyNT.RandomNumberPage")!;
                NavView_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(
    Type navPageType,
    NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                ContentFrame.Navigate(navPageType, null, transitionInfo);
            }
        }

        private void mainNv_Loaded(object sender, RoutedEventArgs e)
        {
            _updateInstance = App.Host!.Services.GetRequiredService<UpdateService>();
            _updateInstance.NewVersionAvailable += OnUpdateAvailable;
            _ = _updateInstance.CheckUpdateAsync();

            // NavView doesn't load any page by default, so load home page.
            mainNv.SelectedItem = mainNv.MenuItems[0];
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate(typeof(RandomNumberPage), new EntranceNavigationTransitionInfo());
        }


        public void OnUpdateAvailable(object sender, NewVersionAvailableEventArgs e)
        {
            updateBadge.Visibility = Visibility.Visible;
        }
    }
}
