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
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Storage;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;

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
        private bool disableRepeat = false;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public RandomNumberPage()
        {
            this.InitializeComponent();
            
        }
        private async void StartDrawButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = false;
            // 禁止按钮再次触发
            StartDrawButton.IsEnabled = false;
            // 显示结果 ListView
            DrawResultListView.Visibility = Visibility.Visible;
            // 初始化进度条
            IndeterminateProgressBar.Visibility = Visibility.Visible;
            IndeterminateProgressBar.ShowPaused = false;
            IndeterminateProgressBar.ShowError = false;
            // 清空之前的结果
            numberResult.Clear();
            // 检查数值合法性并转换为 int
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
                            // 数值过大，显示警告信息
                            if (count > 50000) ShowWarningBar("生成的随机数过多，将启用分批处理，但仍可能导致UI线程卡顿。\n可能的结果框溢出系 WinUI 组件已知问题。");
                            else ShowWarningBar("生成的随机数过多，将启用分批处理，但仍可能导致UI线程卡顿。");
                        }
                        if (disableRepeat)
                        {
                            if (max - min + 1 < count)
                            {
                                // 数值范围不足，显示错误信息
                                ShowErrorBar("您已启用避免重复，最大值与最小值之间的数值范围不足以生成指定数量的随机数，\n请检查输入后重试。");
                                DrawResultListView.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                await StartDrawUniqueRandom(min, max, count, numberResult);
                                isSuccess = true;
                            }

                        }
                        else
                        {
                            // 生成随机数
                            await StartDrawRandom(min, max, count, numberResult);
                            isSuccess = true;
                        }

                    } catch (Exception ex)
                    {
                        isSuccess = false;
                        Debug.WriteLine("Ex:" + ex.ToString());
                        ShowErrorBar("发生未知的异常:\n" + ex.ToString());
                    }

                }
                else
                {
                    // 最小值大于最大值，显示错误信息
                    ShowErrorBar("最小值不能大于最大值，请检查输入后重试。");
                }
            }
            else
            {
                // 数值不合法，显示错误信息
                ShowErrorBar("无法将输入的数值转换为整数 (可能某个值超出 int 数据范围?)，请检查输入后重试。");
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
                ShowErrorBar("发生未知的异常:\n" + ex.ToString());
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
            // 清空结果
            numberResult.Clear();
            // 隐藏结果 ListView
            DrawResultListView.Visibility = Visibility.Collapsed;
        }

        private void DisableRepeatSwitch_Click(object sender, RoutedEventArgs e)
        {
            disableRepeat = DisableRepeatSwitch.IsChecked ?? false;
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
            animation.Duration = TimeSpan.FromMilliseconds(600); // 动画时长

            visual.StartAnimation("Translation", animation);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartTitleAnimation();
        }
    }

}
