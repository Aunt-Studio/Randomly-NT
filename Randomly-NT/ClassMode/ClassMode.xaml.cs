using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Randomly_NT.ClassMode.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT.ClassMode
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassMode : Window
    {
        public ClassMetadata ClassMetadata { get;  }
        public Question CurrentQuestion { get; private set; } = new();
        public List<Student> Students { get; } = new();
        public List<Question> Questions { get; } = new();
        public StudentSelector StudentSelector { get; } 
        public ClassMode(ClassMetadata metadata, List<Question> questions, List<Student> students)
        {
            this.InitializeComponent();
            ClassMetadata = metadata;
            Students = students;
            Questions = questions;
            StudentSelector = new StudentSelector(students);
            mainNv.MenuItems.Add(new NavigationViewItem
            {
                Content = "Welcome",
                Icon = new SymbolIcon(Symbol.Home),
                Tag = typeof(ClassWelcomePage).FullName
            });
            for (int i = 0; i < Questions.Count; i++)
            {
                var navItem = new NavigationViewItem
                {
                    Content = $"问题 {i+1}",
                    Icon = new SymbolIcon(Symbol.Document),
                    Tag = typeof(ClassQuestionPage).FullName
                };
                mainNv.MenuItems.Add(navItem);
            }
        }
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // NavView doesn't load any page by default, so load home page.
            mainNv.SelectedItem = mainNv.MenuItems[0];
        }
        private void NavView_SelectionChanged(NavigationView sender,
                                      NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                Type navPageType = Type.GetType(args.SelectedItemContainer.Tag.ToString() ?? "Pages.ClassWelcomePage")!;
                if (mainNv.MenuItems.IndexOf(args.SelectedItemContainer) > 0
                    && mainNv.MenuItems.IndexOf(args.SelectedItemContainer) <= Questions.Count)
                {
                    CurrentQuestion = Questions[mainNv.MenuItems.IndexOf(args.SelectedItemContainer) - 1];
                }
                
                NavView_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }
        private void NavView_Navigate(Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = contentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null)
            {
                contentFrame.Navigate(navPageType, this, transitionInfo);
            }
        }
        /// <summary>
        /// 通过索引导航到问题。将 <paramref name="index"/> 设为 0 以导航到欢迎页。
        /// </summary>
        /// <param name="index"></param>
        public void NavToQuestion(int index)
        {
            if (index >= 0 && index < Questions.Count + 1)
            {
                mainNv.SelectedItem = mainNv.MenuItems[index];
            }
        }
    }
}
