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

        private HashSet<int> numberResult = new();

        private string STDFilePath { get; set; } = string.Empty;
        public RandomNamePage()
        {
            this.InitializeComponent();
            bool SaveRNameDataPath = localSettings.Values.ContainsKey("SaveRNameDataPath") ? (bool)localSettings.Values["SaveRNameDataPath"] : true;
            bool SaveRNameDraw = localSettings.Values.ContainsKey("SaveRNameDraw") ? (bool)localSettings.Values["SaveRNameDraw"] : false;
            if (SaveRNameDataPath)
            {
                STDFilePath = localSettings.Values.ContainsKey("NameDataPath") ? (string)localSettings.Values["NameDataPath"] : string.Empty;
                if (!string.IsNullOrEmpty(STDFilePath))
                {
                    LoadStudentDataFile(STDFilePath);
                }
            }
            if (SaveRNameDraw)
            {
                int NameDrawNumber = localSettings.Values.ContainsKey("NameDrawNumber") ? (int)localSettings.Values["NameDrawNumber"] : 5;
                DrawNumber.Text = NameDrawNumber.ToString();
            }

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
                openPicker.CommitButtonText = "ѡ�� Randomly students data (*.rsd) �ļ�";
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    STDFilePath = file.Path;
                    localSettings.Values["NameDataPath"] = STDFilePath;
                    await LoadStudentDataFileAsync(STDFilePath);
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar($"�����ļ�ʱ�����쳣:\n{ex.ToString()}");
            }
            finally
            {
                ImportButton.IsEnabled = true;
            }


        }

        private void LoadStudentDataFile(string filePath)
        {
            try
            {
                JObject studentsDataJObject = JObject.Parse(File.ReadAllText(filePath));
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
        private async Task LoadStudentDataFileAsync(string filePath)
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
            DrawInSelectionButton.IsEnabled = false;
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
            numberResult.Clear();
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
                        localSettings.Values["NameDrawNumber"] = count;
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
                            ShowErrorBar($"�ڳ��Է��������ʱ�����쳣:\n{hrEx.Message}\n�����������Ӳ��Ժ����ԡ�\n����״̬�����������н������ָ������3��������ʹ���򲻴� random.org �����Դ��");
                        }
                        else
                        {
                            ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
                        }
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
            DrawInSelectionButton.IsEnabled = true;

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
            catch
            {
                throw;
            }

        }

        private async Task StartDrawUniqueRandomNameInRange(int min, int max, int count, IList<string> nameList)
        {

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
                    DrawingResultNames.Add(nameList[number - 1]);
                }
            }
            catch
            {
                throw;
            }

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

        private async void DrawInSelectionButton_Click(object sender, RoutedEventArgs e)
        {

            bool isSuccess = false;
            // ��ֹ��ť�ٴδ���
            DrawButton.IsEnabled = false;
            DrawInSelectionButton.IsEnabled = false;
            // ��ʾ��� ListView
            DrawResultSP.Visibility = Visibility.Visible;
            // �趨��ȡ��Χ
            List<string> names = OriginalNamesListView.SelectedItems.Cast<string>().ToList();
            // ��ȡ�������Χ
            int min = 1;
            int max = names.Count;
            // ��ʼ��������
            IndeterminateProgressBar.Visibility = Visibility.Visible;
            IndeterminateProgressBar.ShowPaused = false;
            IndeterminateProgressBar.ShowError = false;
            // ���֮ǰ�Ľ��
            DrawingResultNames.Clear();
            numberResult.Clear();
            // �����ֵ�Ϸ��Բ�ת��Ϊ int
            if (int.TryParse(DrawNumber.Text, out int count))
            {
                try
                {
                    if (max - min + 1 < count)
                    {
                        // ��ֵ��Χ���㣬��ʾ������Ϣ
                        ShowErrorBar($"��ǰ��ѡ����������({max})�����Գ�ȡĿ���ȡ��({count})��\n����ٳ�ȡ��������ѡ������");
                        DrawResultSP.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        localSettings.Values["NameDrawNumber"] = count;
                        await StartDrawUniqueRandomNameInRange(min, max, count, names);
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
                            ShowErrorBar($"�ڳ��Է��������ʱ�����쳣:\n{hrEx.Message}\n�����������Ӳ��Ժ����ԡ�\n����״̬�����������н������ָ������3��������ʹ���򲻴� random.org �����Դ��");
                        }
                        else
                        {
                            ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
                        }
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
            DrawInSelectionButton.IsEnabled = true;

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

        private void OriginalNamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OriginalNamesListView.SelectedItems.Count > 1)
            {
                DrawInSelectionButton.Visibility = Visibility.Visible;
            }
            else
            {
                DrawInSelectionButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
