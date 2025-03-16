using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuestionsPage : Page
    {
        private ClassEditor? classEditorWindow;
        public QuestionItem QuestionItem { get; set; } = new();
        public ObservableCollection<QuestionItem> QuestionItems { get; set; } = new();
        public QuestionsPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ClassEditor classEditorWindow)
            {
                this.classEditorWindow = classEditorWindow;
            }
        }
        private void QuestionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is not null)
            {
                QuestionItem = QuestionsListView.SelectedItem as QuestionItem;
                QuestionTitle.Text = QuestionItem.Question;
                QuestionDesc.Text = QuestionItem.Description;
                ConfirmEditButton.Visibility = Visibility.Visible;
            }
            
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(QuestionTitle.Text))
            {
                QuestionItem.Question = QuestionTitle.Text;
                if (!string.IsNullOrWhiteSpace(QuestionDesc.Text))
                {
                    QuestionItem.Description = QuestionDesc.Text;
                }
                QuestionItems.Add(QuestionItem);
                QuestionItem = new();
                QuestionTitle.Text = "";
                QuestionDesc.Text = "";
                if (QuestionItems.Count > 0)
                {
                    NextButton.IsEnabled = true;
                }
            }

            classEditorWindow.QuestionItems = QuestionItems.ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is QuestionItem item)
            {
                QuestionItems.Remove(item);
                if (QuestionItems.Count == 0)
                {
                    NextButton.IsEnabled = false;
                }
            }
            classEditorWindow.QuestionItems = QuestionItems.ToList();
        }


        private void ConfirmEditButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmEditButton.Visibility = Visibility.Collapsed;
            QuestionItem.Question = QuestionTitle.Text;
            QuestionItem.Description = QuestionDesc.Text;
            QuestionTitle.Text = "";
            QuestionDesc.Text = "";
            classEditorWindow.QuestionItems = QuestionItems.ToList();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            classEditorWindow?.NavTo(1);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            classEditorWindow?.NavTo(3);
        }
    }

}
