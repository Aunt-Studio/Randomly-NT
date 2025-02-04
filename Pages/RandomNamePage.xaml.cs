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
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Numerics;
using Windows.Storage;
using System.Net.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RandomNamePage : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public ObservableCollection<string> DrawingResultNames { get; set; } = new();
        public ObservableCollection<string> OriginalNames { get; set; } = new();

        private string STDFilePath { get; set; } = string.Empty;
        public RandomNamePage()
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
                openPicker.FileTypeFilter.Add(".rsd");
                openPicker.CommitButtonText = "选择 Randomly students data (*.rsd) 文件";
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    STDFilePath = file.Path;
                    await LoadStudentDataFile(STDFilePath);
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

        private async Task LoadStudentDataFile(string filePath)
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
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StudentsDataEditor studentsDataEditor;
                // 判定是否已在当前页面打开数据
                if (string.IsNullOrEmpty(STDFilePath))
                {
                    studentsDataEditor = new StudentsDataEditor();
                }
                else
                {
                    studentsDataEditor = new StudentsDataEditor(STDFilePath);
                }
                studentsDataEditor.Activate();
            }
            catch (Exception ex)
            {
                ShowErrorBar($"启动编辑器时出现异常:\n{ex.ToString()}");
            }
        }



        private async void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = false;
            // 禁止按钮再次触发
            DrawButton.IsEnabled = false;
            AddedDrawButton.IsEnabled = false;
            // 显示结果 ListView
            DrawResultSP.Visibility = Visibility.Visible;
            // 获取随机数范围
            int min = 1;
            int max = OriginalNames.Count;
            // 初始化进度条
            IndeterminateProgressBar.Visibility = Visibility.Visible;
            IndeterminateProgressBar.ShowPaused = false;
            IndeterminateProgressBar.ShowError = false;
            // 清空之前的结果
            DrawingResultNames.Clear();
            // 检查数值合法性并转换为 int
            if (int.TryParse(DrawNumber.Text, out int count))
            {
                try
                {
                    if (max - min + 1 < count)
                    {
                        // 数值范围不足，显示错误信息
                        ShowErrorBar($"当前姓名总数({max})不足以抽取目标抽取数({count})。\n请减少抽取数。");
                        DrawResultSP.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        await StartDrawUniqueRandomName(min, max, count);
                        isSuccess = true;
                    }

                }
                catch (AggregateException ae)
                {
                    isSuccess = false;
                    var flattenedExceptions = ae.Flatten().InnerExceptions;
                    foreach (var ex in flattenedExceptions)
                    {
                        if (ex is HttpRequestException hrEx)
                        {
                            ShowErrorBar($"在尝试发送请求包时出现异常:\n{hrEx.Message}\n请检查网络连接并稍后再试。\n无网状态下请在设置中将随机化指数降至3及以下以使程序不从 random.org 获得熵源。");
                        }
                        else
                        {
                            ShowErrorBar("发生未知的异常:\n" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    Debug.WriteLine("Ex:" + ex.ToString());
                    ShowErrorBar("发生未知的异常:\n" + ex.ToString());
                }
            }
            else
            {
                // 数值不合法，显示错误信息
                ShowErrorBar("无法将抽取数数值转换为整数 (可能值超出 int 数据范围?)，请检查输入后重试。");
            }
            // 恢复按钮状态
            DrawButton.IsEnabled = true;
            AddedDrawButton.IsEnabled = true;

            // 进度条显示状态
            if (!isSuccess)
            {
                IndeterminateProgressBar.ShowError = true;
            }
            else
            {
                IndeterminateProgressBar.Visibility = Visibility.Collapsed;
            }
        }
        private async Task StartDrawUniqueRandomName(int min, int max, int count)
        {
            HashSet<int> numberResult = new();
            try
            {
                int randomizeIndex = localSettings.Values.ContainsKey("RandomizeIndex") ? (int)localSettings.Values["RandomizeIndex"] : 1;
                switch (randomizeIndex)
                {
                    case 1:
                        numberResult = await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, RandomEntropySource.SystemClock);
                        break;
                    case 2:
                        numberResult = await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise);
                        break;
                    case 3:
                        numberResult = await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint);
                        break;
                    case 4:
                        numberResult = await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint, RandomEntropySource.RandomOrg);
                        break;
                    default:
                        numberResult = await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count);
                        break;
                }

                foreach (var number in numberResult)
                {
                    DrawingResultNames.Add(OriginalNames[number - 1]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ex:" + ex.ToString());
                ShowErrorBar("发生未知的异常:\n" + ex.ToString());
            }

        }
        private void AddedDrawButton_Click(object sender, RoutedEventArgs e)
        {

        }


        private void ShowWarningBar(string message)
        {
            if (infoBarStack.Children.Count > 1)
            {
                infoBarStack.Children.Remove(infoBarStack.Children[0]);
            }
            InfoBar infoBar = new InfoBar()
            {
                Message = message,
                Severity = InfoBarSeverity.Warning,
                IsOpen = true
            };
            infoBarStack.Children.Add(infoBar);
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
    }
}
