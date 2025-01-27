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
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RandomNamePage : Page
    {
        public ObservableCollection<string> DrawingResultNames { get; set; } = new ();
        public ObservableCollection<string> OriginalNames { get; set; } = new();
        public RandomNamePage()
        {
            this.InitializeComponent();
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportButton.IsEnabled = false;

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.MainWindow;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".rsd");
            openPicker.CommitButtonText = "选择 Randomly students data (*.rsd) 文件";

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                ImportStudents(file.Path);
            }

            //re-enable the button
            ImportButton.IsEnabled = true;

        }

        private void ImportStudents(string filePath)
        {
            JObject students = JObject.Parse(File.ReadAllText(filePath));
            if (students["students"] is JArray studentNames) // 匹配 students 数组
            {
                foreach (var student in studentNames)
                {
                    OriginalNames.Add(student.ToString());
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
