using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RandomNumberPage : Page
    {
        private ObservableCollection<int> numberResult = new();
        private bool avoidRepeat = false;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public RandomNumberPage()
        {
            this.InitializeComponent();
            bool saveRNumRange = localSettings.Values.ContainsKey("SaveRNumRange") ? (bool)localSettings.Values["SaveRNumRange"] : true;
            bool saveRNumDraw = localSettings.Values.ContainsKey("SaveRNumDraw") ? (bool)localSettings.Values["SaveRNumDraw"] : true;
            int min = localSettings.Values.ContainsKey("MinNumber") ? (int)localSettings.Values["MinNumber"] : 1;
            int max = localSettings.Values.ContainsKey("MaxNumber") ? (int)localSettings.Values["MaxNumber"] : 50;

            int count = localSettings.Values.ContainsKey("NumberCount") ? (int)localSettings.Values["NumberCount"] : 10;
            bool avoidRepeat = localSettings.Values.ContainsKey("AvoidRepeat") ? (bool)localSettings.Values["AvoidRepeat"] : false;
            if (saveRNumRange)
            {
                MinNumber.Text = min.ToString();
                MaxNumber.Text = max.ToString();
            }
            else
            {
                MinNumber.Text = "1";
                MaxNumber.Text = "50";
            }
            if (saveRNumDraw)
            {
                Number.Text = count.ToString();
                DisableRepeatSwitch.IsChecked = avoidRepeat;
            }
            else
            {
                Number.Text = "10";
                DisableRepeatSwitch.IsChecked = false;
            }
        }
        private async void StartDrawButton_Click(object sender, RoutedEventArgs e)
        {
            // ��� flag ֻ����Ϊ�ο���ʵ����ֻ���ƽ������Ƿ���ʾ����
            bool isSuccess = false;
            // ��ֹ��ť�ٴδ���
            StartDrawButton.IsEnabled = false;
            // ��ʾ��� ListView
            DrawResultListView.Visibility = Visibility.Visible;
            // ��ʼ��������
            IndeterminateProgressBar.Visibility = Visibility.Visible;
            IndeterminateProgressBar.ShowPaused = false;
            IndeterminateProgressBar.ShowError = false;
            // ���֮ǰ�Ľ��
            numberResult.Clear();
            // �����ֵ�Ϸ��Բ�ת��Ϊ int
            if (int.TryParse(MinNumber.Text, out int min)
                && int.TryParse(MaxNumber.Text, out int max)
                && int.TryParse(Number.Text, out int count))
            {
                if (min <= max)
                {
                    try
                    {
                        if (count > 1000)
                        {
                            // ��ֵ������ʾ������Ϣ
                            if (count > 50000) ShowWarningBar("���ɵ���������࣬�����÷����������Կ��ܵ���UI�߳̿��١�\n���ܵĽ�������ϵ WinUI �����֪���⡣");
                            else ShowWarningBar("���ɵ���������࣬�����÷����������Կ��ܵ���UI�߳̿��١�");
                        }
                        if (avoidRepeat)
                        {
                            if (max - min + 1 < count)
                            {
                                // ��ֵ��Χ���㣬��ʾ������Ϣ
                                ShowErrorBar("�������ñ����ظ������ֵ����Сֵ֮�����ֵ��Χ����������ָ���������������\n������������ԡ�");
                                DrawResultListView.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                // �������
                                SaveRandomNumberSettings(min, max, count);
                                // ���������
                                await StartDrawUniqueRandom(min, max, count, numberResult);
                                isSuccess = true;
                            }

                        }
                        else
                        {
                            // �������
                            SaveRandomNumberSettings(min, max, count);
                            // ���������
                            await StartDrawRandom(min, max, count, numberResult);
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
                    // ��Сֵ�������ֵ����ʾ������Ϣ
                    ShowErrorBar("��Сֵ���ܴ������ֵ��������������ԡ�");
                }
            }
            else
            {
                // ��ֵ���Ϸ�����ʾ������Ϣ
                ShowErrorBar("�޷����������ֵת��Ϊ���� (����ĳ��ֵ���� int ���ݷ�Χ?)��������������ԡ�");
            }
            StartDrawButton.IsEnabled = true;
            if (!isSuccess)
            {
                IndeterminateProgressBar.ShowError = true;
            }
            else
            {
                IndeterminateProgressBar.Visibility = Visibility.Collapsed;
            }

        }

        private void SaveRandomNumberSettings(int min, int max, int count)
        {
            bool saveRNumRange = localSettings.Values.ContainsKey("SaveRNumRange") ? (bool)localSettings.Values["SaveRNumRange"] : true;
            bool saveRNumDraw = localSettings.Values.ContainsKey("SaveRNumDraw") ? (bool)localSettings.Values["SaveRNumDraw"] : true;
            if (saveRNumRange)
            {
                Debug.WriteLine("Saving RNumRange.");
                localSettings.Values["MinNumber"] = min;
                localSettings.Values["MaxNumber"] = max;
                Debug.WriteLine("Saved RNumRange.");
            }
            if (saveRNumDraw)
            {
                Debug.WriteLine("Saving RNumDraw.");
                localSettings.Values["NumberCount"] = count;
                localSettings.Values["AvoidRepeat"] = avoidRepeat;
                Debug.WriteLine("Saved RNumRange.");
            }
        }

        private async Task StartDrawRandom(int min, int max, int count, ObservableCollection<int> numberResult)
        {
            int randomizeIndex = localSettings.Values.ContainsKey("RandomizeIndex") ? (int)localSettings.Values["RandomizeIndex"] : 1;
            switch (randomizeIndex)
            {
                case 1:
                    await RandomDrawer.DrawRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock);
                    break;
                case 2:
                    await RandomDrawer.DrawRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise);
                    break;
                case 3:
                    await RandomDrawer.DrawRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint);
                    break;
                case 4:
                    await RandomDrawer.DrawRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint, RandomEntropySource.RandomOrg);
                    break;
                default:
                    await RandomDrawer.DrawRandomIntAsync(min, max, count, numberResult);
                    break;
            }
        }
        private async Task StartDrawUniqueRandom(int min, int max, int count, ObservableCollection<int> numberResult)
        {
            try
            {
                int randomizeIndex = localSettings.Values.ContainsKey("RandomizeIndex") ? (int)localSettings.Values["RandomizeIndex"] : 1;
                switch (randomizeIndex)
                {
                    case 1:
                        await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock);
                        break;
                    case 2:
                        await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise);
                        break;
                    case 3:
                        await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint);
                        break;
                    case 4:
                        await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, numberResult, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint, RandomEntropySource.RandomOrg);
                        break;
                    default:
                        await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, numberResult);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ex:" + ex.ToString());
                ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
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

        private void ResetResultsButton_Click(object sender, RoutedEventArgs e)
        {
            // ��ս��
            numberResult.Clear();
            // ���ؽ�� ListView
            DrawResultListView.Visibility = Visibility.Collapsed;
        }

        private void DisableRepeatSwitch_Click(object sender, RoutedEventArgs e)
        {
            // �����������ݰ󶨻᲻����ã����ҽ�������д��
            avoidRepeat = DisableRepeatSwitch.IsChecked ?? false;
        }


        private void StartTitleAnimation()
        {
            var visual = ElementCompositionPreview.GetElementVisual(TitleSP);
            var compositor = visual.Compositor;

            ElementCompositionPreview.SetIsTranslationEnabled(TitleSP, true);
            visual.Properties.InsertVector3("Translation", new Vector3(36, -100, 0)); // ��ʼλ��

            var easingFunc = compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.1f, 0.8f), new Vector2(0.2f, 1.0f));  // ��������
            var animation = compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, new Vector3(36, 48, 0), easingFunc); // Ŀ��λ��
            animation.Duration = TimeSpan.FromMilliseconds(600); // ����ʱ��

            visual.StartAnimation("Translation", animation);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartTitleAnimation();
        }
    }

}
