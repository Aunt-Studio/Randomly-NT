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
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReleasePage : Page
    {
        private ClassEditor? classEditorWindow;
        private bool isReady = true;
        public ReleasePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ClassEditor classEditorWindow)
            {
                this.classEditorWindow = classEditorWindow;
                if (string.IsNullOrWhiteSpace(classEditorWindow.ClassMetadata.ClassName) || classEditorWindow.QuestionItems.Count == 0 || classEditorWindow.Students.Count == 0)
                {
                    isReady = false;
                    OutputButton.IsEnabled = false;
                    ShowErrorBar("请先完成所有先前步骤。");
                }
                else
                {
                    string metaDisplayText = $"""
                        课程名称: {classEditorWindow.ClassMetadata.ClassName}
                        问题数量: {classEditorWindow.QuestionItems.Count}
                        学生数量: {classEditorWindow.Students.Count}
                        """;
                    MetaDataTB.Text = metaDisplayText;
                    OutputButton.IsEnabled = true;
                }
            }

        }
        private async void Output_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                var window = classEditorWindow;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Randomly Class Data", new List<string>() { ".rcd" });
                savePicker.SuggestedFileName = classEditorWindow.ClassMetadata.ClassName;

                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file is not null)
                {
                    CachedFileManager.DeferUpdates(file);

                    SaveFile(file.Path);

                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        ShowSuccessBar($"文件已于{DateTime.Now}保存");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar("保存文件时发生错误: " + ex.ToString());
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
        private void ShowSuccessBar(string message)
        {
            if (infoBarStack.Children.Count > 0)
            {
                infoBarStack.Children.Remove(infoBarStack.Children[0]);
            }
            InfoBar infoBar = new InfoBar()
            {
                Message = message,
                Severity = InfoBarSeverity.Success,
                IsOpen = true
            };
            infoBarStack.Children.Add(infoBar);
        }
        private void SaveFile(string path)
        {
            // 将QuestionItem 转换为 Question
            List<Question> questions = new();
            foreach (var item in classEditorWindow!.QuestionItems)
            {
                questions.Add(new Question()
                {
                    Difficulty = item.Difficulty,
                    Title = item.Question!,
                    Description = item.Description
                });
            }
            // 封装课堂
            SingleClass singleClass = new()
            {
                ClassMetadata = classEditorWindow!.ClassMetadata,
                Questions = questions,
                Students = classEditorWindow!.Students
            };
            string json = JsonConvert.SerializeObject(singleClass);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }
    }
}
