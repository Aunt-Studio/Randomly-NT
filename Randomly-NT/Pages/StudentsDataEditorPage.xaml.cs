using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StudentsDataEditorPage : Page
    {
        public ObservableCollection<StudentDataItem> studentDataItems = new();
        public StudentsDataEditor? dataEditor;
        public string dataPath = string.Empty;

        public StudentsDataEditorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is StudentsDataEditor dataEditorWindow)
            {
                this.dataEditor = dataEditorWindow;
                this.dataPath = dataEditor.STDFilePath;
                dataEditor.Saved = true;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartTitleAnimation();
            if (!string.IsNullOrEmpty(dataPath))
            {
                await LoadFile(dataPath);
            }
            else
            {
                studentDataItems.Add(new StudentDataItem { Name = "请在底部导入已编辑好的学生列表文件" });
                studentDataItems.Add(new StudentDataItem { Name = "或右键删除这两行开始输入您的学生" });
            }
        }

        private void StartTitleAnimation()
        {
            var visual = ElementCompositionPreview.GetElementVisual(TitleSP);
            var compositor = visual.Compositor;

            ElementCompositionPreview.SetIsTranslationEnabled(TitleSP, true);
            visual.Properties.InsertVector3("Translation", new Vector3(36, -100, 0)); // 初始位置

            var easingFunc = compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.1f, 0.8f), new Vector2(0.2f, 1.0f));  // 缓动函数
            var animation = compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, new Vector3(36, 48, 0), easingFunc); // 目标位置
            animation.Duration = TimeSpan.FromMilliseconds(800); // 动画时长

            visual.StartAnimation("Translation", animation);
        }

        private void MenuFlyoutDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            dataEditor!.Saved = false;
            var item = (sender as FrameworkElement)?.DataContext as StudentDataItem;
            if (item != null)
            {
                studentDataItems.Remove(item);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            dataEditor!.Saved = false;
            var studentName = NameAddTB.Text;
            if (!string.IsNullOrWhiteSpace(studentName))
            {
                studentDataItems.Add(new StudentDataItem { Name = studentName });
            }
        }

        private void ConfirmEditButton_Click(object sender, RoutedEventArgs e)
        {
            dataEditor!.Saved = false;
            var studentName = NameEditTB.Text;
            if (!string.IsNullOrEmpty(studentName))
            {
                var item = StudentDataListView.SelectedItem as StudentDataItem;
                if (item != null)
                {
                    item.Name = studentName;
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var item = e.AddedItems[0] as StudentDataItem;
                if (item != null)
                {
                    EditSP.Visibility = Visibility.Visible;
                    NameEditTB.Text = item.Name;
                }
            }
            else if (e.AddedItems.Count == 0)
            {
                EditSP.Visibility = Visibility.Collapsed;
                AddSP.Visibility = Visibility.Visible;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            try
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                var window = dataEditor;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Randomly Students Data", new List<string>() { ".rsd" });
                savePicker.SuggestedFileName = "students";

                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);

                    await SaveFile(file.Path);

                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        dataEditor!.Saved = true;
                        ShowSuccessBar($"文件已于{DateTime.Now}保存");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar("保存文件时发生错误: " + ex.ToString());
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadFileButton.IsEnabled = false;
                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                var window = dataEditor;
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
                    await LoadFile(file.Path);
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar($"载入文件时出现异常:\n{ex.ToString()}");
            }
            finally
            {
                LoadFileButton.IsEnabled = true;
            }
        }

        private async Task SaveFile(string targetPath)
        {
            try
            {
                // File Version = 1.0

                var jsonData = new
                {
                    version = "1.0",
                    students = studentDataItems.Select(item => item.Name).ToArray()
                };

                string jsonString = JsonConvert.SerializeObject(jsonData,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                    });

                await File.WriteAllTextAsync(targetPath, jsonString);
            }
            catch
            {
                throw;
            }

        }

        private async Task LoadFile(string filePath)
        {
            try
            {
                JObject studentsDataJObject = JObject.Parse(await File.ReadAllTextAsync(filePath));
                var fileVersion = studentsDataJObject["version"]?.ToString() ?? "null";
                if (fileVersion == "1.0")
                {
                    if (studentsDataJObject["students"] is JArray studentNames) // 匹配 studentsDataJObject 数组
                    {
                        studentDataItems.Clear();
                        foreach (var student in studentNames)
                        {
                            studentDataItems.Add(new StudentDataItem { Name = student.ToString() });
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


    public class StudentDataItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
    }

}
