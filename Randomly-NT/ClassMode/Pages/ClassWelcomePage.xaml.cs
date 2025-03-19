using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassWelcomePage : Page
    {
        private ClassMode? classModeWindow;
        public ClassWelcomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ClassMode classModeWindow)
            {
                this.classModeWindow = classModeWindow;
                this.ClassWelcomeTB.Text = $"{classModeWindow.ClassMetadata.ClassName}";
                this.ClassDescTB.Text = $"欢迎来到{classModeWindow.ClassMetadata.Teacher}老师的{classModeWindow.ClassMetadata.Course}课堂!";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            classModeWindow?.NavToQuestion(1);
        }
    }
}
