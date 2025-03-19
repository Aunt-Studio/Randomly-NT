using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Randomly_NT.ClassMode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassPreparePage : Page
    {
        public string ClassFilePath { get; set; } = string.Empty;
        private StudentSelector? selector = null;
        public ClassPreparePage()
        {
            this.InitializeComponent();
        }
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportButton.IsEnabled = false;

                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                var window = App.MainWindow;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.FileTypeFilter.Clear();
                openPicker.FileTypeFilter.Add(".rcd");
                openPicker.CommitButtonText = "选择 Randomly class data (*.rcd) 文件";
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    ClassFilePath = file.Path;
                    // 解析元数据
                    string jsonStr = File.ReadAllText(ClassFilePath);
                    SingleClass? singleClass = JsonConvert.DeserializeObject<SingleClass>(jsonStr);
                    if (singleClass is not null)
                    {
                        ClassMode.ClassMode classMode = new(singleClass.ClassMetadata, singleClass.Questions, singleClass.Students);
                        classMode.Activate();
                    }
                    else
                    {
                        ShowErrorBar("无法解析课程文件。请确保文件完整、有效。");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar($"载入文件时出现异常:\n{ex.ToString()}");
            }
            finally
            {
                ImportButton.IsEnabled = true;
            }
        }
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


        private void OpenEditorButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ClassMode.ClassEditor();
            editor.Activate();
        }
    }
}
