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
        public RandomNumberPage()
        {
            this.InitializeComponent();
            
        }

        private async void StartDrawButton_Click(object sender, RoutedEventArgs e)
        {
            // ��ֹ��ť�ٴδ���
            StartDrawButton.IsEnabled = false;
            DrawResultListView.Visibility = Visibility.Visible;
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
                            ShowWarningBar("���ɵ���������࣬�����÷����������Կ��ܵ���UI�߳̿��١�");
                        }
                        if (disableRepeat)
                        {
                            if (max - min + 1 < count)
                            {
                                // ��ֵ��Χ���㣬��ʾ������Ϣ
                                ShowErrorBar("�������ñ����ظ������ֵ����Сֵ֮�����ֵ��Χ����������ָ���������������\n������������ԡ�");
                                DrawResultListView.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                // ����Ψһ�����
                                await RandomDrawer.DrawUniqueRandomIntAsync(min, max, count, numberResult);
                            }

                        }
                        else
                        {
                            // ���������
                            await RandomDrawer.DrawRandomIntAsync(min, max, count, numberResult);
                        }

                    } catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
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
                ShowErrorBar("�޷����������ֵת��Ϊ������������������ԡ�");
            }
            StartDrawButton.IsEnabled = true;
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
            disableRepeat = DisableRepeatSwitch.IsChecked ?? false;
        }
    }
}
