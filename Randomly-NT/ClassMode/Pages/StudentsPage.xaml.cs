using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StudentsPage : Page
    {
        public List<string> OriginalNames { get; private set; }
        public string STDFilePath { get; private set; }

        public StudentsPage()
        {
            this.InitializeComponent();
        }

        #region 从 RSD 导入学生
        private async void ImportRSDButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportRSDButton.IsEnabled = false;

                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                var window = App.MainWindow;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.FileTypeFilter.Clear();
                openPicker.FileTypeFilter.Add(".rsd");
                openPicker.CommitButtonText = "选择 Randomly students data (*.rsd) 文件";
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    STDFilePath = file.Path;
                    await LoadStudentDataFileAsync(STDFilePath);
                }
                if (OriginalNames.Count > 0)
                {
                    ImportNameSP.Visibility = Visibility.Collapsed;
                    ScoreSP.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar($"载入文件时出现异常:\n{ex.ToString()}");
            }
            finally
            {
                ImportRSDButton.IsEnabled = true;
            }
        }
        private async Task LoadStudentDataFileAsync(string filePath)
        {
            try
            {
                JObject studentsDataJObject = JObject.Parse(await File.ReadAllTextAsync(filePath));
                var fileVersion = studentsDataJObject["version"]?.ToString() ?? "null";
                if (fileVersion == "1.0")
                {
                    if (studentsDataJObject["students"] is JArray studentNames) // 匹配 studentsDataJObject 数组
                    {
                        OriginalNames.Clear();
                        foreach (var student in studentNames)
                        {
                            OriginalNames.Add(student.ToString());
                        }
                    }

                }
                else
                {
                    throw new InvalidDataException($"载入的文件版本({fileVersion})不受支持, 预期文件版本为\"1.0\"。");
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion
        private void ShowErrorBar(string message)
        {
            if (infoBarStack.Children.Count > 1)
            {
                infoBarStack.Children.Remove(infoBarStack.Children[0]);
            }
            InfoBar infoBar = new InfoBar()
            {
                Message = message,
                Severity = InfoBarSeverity.Error,
                IsOpen = true
            };
            infoBarStack.Children.Add(infoBar);
        }

        private void ImportFormTextButton_Click(object sender, RoutedEventArgs e)
        {
            // 以换行符分割学生
            OriginalNames = new List<string>(StudentListTextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
            if (OriginalNames.Count > 0)
            {
                ImportNameSP.Visibility = Visibility.Collapsed;
                ScoreSP.Visibility = Visibility.Visible;
            }
        }
    }
}
