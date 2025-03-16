using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BasicDataPage : Page
    {
        private ClassEditor? classEditorWindow;
        public BasicDataPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ClassEditor classEditorWindow)
            {
                this.classEditorWindow = classEditorWindow;
                ClassNameTB.Text = this.classEditorWindow?.ClassData.ClassName;
                TeacherNameTB.Text = this.classEditorWindow?.ClassData.Teacher;
                CourseNameTB.Text = this.classEditorWindow?.ClassData.Course;
            }
        }
        private void ClassNameChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    NextButton.IsEnabled = true;
                    if (classEditorWindow != null)
                        classEditorWindow.ClassData.ClassName = textBox.Text;
                }
            }
        }
        private void TeacherNameChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (classEditorWindow != null)
                        classEditorWindow.ClassData.Teacher = textBox.Text;
                }
            }
        }
        private void CourseNameChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (classEditorWindow != null)
                        classEditorWindow.ClassData.Course = textBox.Text;
                }
            }
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            classEditorWindow?.NavTo(0);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            classEditorWindow?.NavTo(2);
        }
    }
}
