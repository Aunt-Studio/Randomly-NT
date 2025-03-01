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
            CenterWindow();

        }



        private void ResizeWindow()
        {
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(750, 950));
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

        public void OnUpdateAvailable(object sender, NewVersionAvailableEventArgs e)
        {
            updateBadge.Visibility = Visibility.Visible;
        }
    }
}
