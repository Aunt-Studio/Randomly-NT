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
using Windows.Storage;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassQuestionPage : Page
    {

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private ClassMode classModeWindow;
        public Question Question { get; private set; }
        public List<Student> Students { get; private set; }
        private int currentIndex;
        public ClassQuestionPage()
        {
            this.InitializeComponent();

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ClassMode classModeWindow)
            {
                this.classModeWindow = classModeWindow;
                Question = classModeWindow.CurrentQuestion;
                Students = classModeWindow.Students;
                this.QuestionTB.Text = $"{Question.Title}";
                this.QuestionDescTB.Text = $"{Question.Description}";
                currentIndex = classModeWindow.Questions.IndexOf(Question) + 1;
                if (currentIndex >= classModeWindow.Questions.Count)
                {
                    NextButton.IsEnabled = false;
                }
                else if (currentIndex <= 1)
                {
                    PrevButton.IsEnabled = false;
                }
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            
            classModeWindow.NavToQuestion(currentIndex + 1);
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = classModeWindow.Questions.IndexOf(Question) + 1;
            classModeWindow.NavToQuestion(currentIndex - 1);
        }

        private async void StartDrawButton_Click(object sender, RoutedEventArgs e)
        {
            StartDrawButton.IsEnabled = false;
            // ��ʼ��������
            IndeterminateProgressBar.Visibility = Visibility.Visible;
            IndeterminateProgressBar.ShowPaused = false;
            IndeterminateProgressBar.ShowError = false;
            try
            {
                Student resultStudent = await DrawStudent();
                DrawResultTB.Text = $"���� {resultStudent.Name} ͬѧ�ش�~";
                IndeterminateProgressBar.Visibility = Visibility.Collapsed;
            }
            catch (AggregateException ae)
            {
                var flattenedExceptions = ae.Flatten().InnerExceptions;
                foreach (var ex in flattenedExceptions)
                {
                    if (ex is HttpRequestException hrEx)
                    {
                        ShowErrorBar($"�ڳ��Է��������ʱ�����쳣:\n{hrEx.Message}\n�����������Ӳ��Ժ����ԡ�\n����״̬�����������н������ָ������3�����£����������������ã���ʹ���򲻴� random.org �����Դ��");
                        IndeterminateProgressBar.ShowError = true;
                    }
                    else
                    {
                        ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
                        IndeterminateProgressBar.ShowError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorBar("����δ֪���쳣:\n" + ex.ToString());
                IndeterminateProgressBar.ShowError = true;
            }
            finally
            {
                StartDrawButton.IsEnabled = true;
                if (!IndeterminateProgressBar.ShowError)
                {
                    IndeterminateProgressBar.Visibility = Visibility.Collapsed;
                }
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

        private async Task<Student> DrawStudent()
        {
            try
            {
                Student student = new();
                int randomizeIndex = localSettings.Values.ContainsKey("RandomizeIndex") ? (int)localSettings.Values["RandomizeIndex"] : 1;
                await Task.Run(() =>
                {
                    switch (randomizeIndex)
                    {
                        case 1:
                            student = classModeWindow.StudentSelector.Select(Question.Difficulty, RandomEntropySource.SystemClock);
                            break;
                        case 2:
                            student = classModeWindow.StudentSelector.Select(Question.Difficulty, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise);
                            break;
                        case 3:
                            student = classModeWindow.StudentSelector.Select(Question.Difficulty, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint);
                            break;
                        case 4:
                            student = classModeWindow.StudentSelector.Select(Question.Difficulty, RandomEntropySource.SystemClock, RandomEntropySource.RuntimeNoise, RandomEntropySource.MousePoint, RandomEntropySource.RandomOrg);
                            break;
                        default:
                            student = classModeWindow.StudentSelector.Select(Question.Difficulty);
                            break;
                    }
                });

                return student;
            }
            catch
            {
                throw;
            }
        }
    }
}
