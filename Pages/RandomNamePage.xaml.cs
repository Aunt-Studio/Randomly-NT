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
                openPicker.CommitButtonText = "ѡ�� Randomly students data (*.rsd) �ļ�";
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Open the picker for the user to pick a file
                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    STDFilePath = file.Path;
                    await LoadStudentDataFile(STDFilePath);
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar($"�����ļ�ʱ�����쳣:\n{ex.ToString()}");
            }
            finally
            {
                //re-enable the button
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
                    if (studentsDataJObject["students"] is JArray studentNames) // ƥ�� studentsDataJObject ����
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
                    throw new InvalidDataException($"������ļ��汾({fileVersion})����֧��, Ԥ���ļ��汾Ϊ\"1.0\"��");
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
                // �ж��Ƿ����ڵ�ǰҳ�������
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
                ShowErrorBar($"�����༭��ʱ�����쳣:\n{ex.ToString()}");
            }
        }



        private async void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = false;
            // ��ֹ��ť�ٴδ���
            DrawButton.IsEnabled = false;
            AddedDrawButton.IsEnabled = false;
            // ��ʾ��� ListView
            DrawResultSP.Visibility = Visibility.Visible;
            // ��ȡ�������Χ
            int min = 1;
            int max = OriginalNames.Count;
            // ��ʼ��������
            IndeterminateProgressBar.Visibility = Visibility.Visible;
            IndeterminateProgressBar.ShowPaused = false;
            IndeterminateProgressBar.ShowError = false;
            // ���֮ǰ�Ľ��
            DrawingResultNames.Clear();
            // �����ֵ�Ϸ��Բ�ת��Ϊ int
            if (int.TryParse(DrawNumber.Text, out int count))
            {
                try
                {
                    if (max - min + 1 < count)
                    {
                        // ��ֵ��Χ���㣬��ʾ������Ϣ
                        ShowErrorBar($"��ǰ��������({max})�����Գ�ȡĿ���ȡ��({count})��\n����ٳ�ȡ����");
                        DrawResultSP.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        await StartDrawUniqueRandomName(min, max, count);
                        isSuccess = true;
                    }

                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    Debug.WriteLine("Ex:" + ex.ToString());
                    ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
                }
            }
            else
            {
                // ��ֵ���Ϸ�����ʾ������Ϣ
                ShowErrorBar("�޷�����ȡ����ֵת��Ϊ���� (����ֵ���� int ���ݷ�Χ?)��������������ԡ�");
            }
            // �ָ���ť״̬
            DrawButton.IsEnabled = true;
            AddedDrawButton.IsEnabled = true;

            // ��������ʾ״̬
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
                ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
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
