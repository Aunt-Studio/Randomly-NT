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
using System.ComponentModel;
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
    public sealed partial class ClassEditor : Window
    {
        public ClassData ClassData { get; set; }
        public List<QuestionItem> QuestionItems { get; set; } = new();
        public List<Student> Students { get; set; } = new();
        public ClassEditor()
        {
            this.InitializeComponent();

            ClassData = new() {
                ClassName = ""
            };
        }
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // NavView doesn't load any page by default, so load home page.
            mainNv.SelectedItem = mainNv.MenuItems[0];
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate(typeof(WelcomePage), new EntranceNavigationTransitionInfo());
        }
        private void NavView_SelectionChanged(NavigationView sender,
                                      NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                Type navPageType = Type.GetType(args.SelectedItemContainer.Tag.ToString() ?? "Pages.WelcomePage")!;
                NavView_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }
        private void NavView_Navigate(Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = contentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                contentFrame.Navigate(navPageType, this, transitionInfo);
                
            }
        }
        public void NavTo(int index)
        {
            if (index < mainNv.MenuItems.Count) mainNv.SelectedItem = mainNv.MenuItems[index];
        }
        public void NewClassData()
        {
            ClassData = new()
            {
                ClassName = ""
            };
        }
    }

    public class ClassData
    {
        public string? ClassName { get; set; }
        public string? Teacher { get; set; }
        public string? Course { get; set; }

        
    }
    public class QuestionItem : INotifyPropertyChanged
    {
        private string? question;
        private string? description;

        public event PropertyChangedEventHandler? PropertyChanged;
        public string? Question
        {
            get => question;
            set
            {
                if (question != value)
                {
                    question = value;
                    PropertyChanged?.Invoke(this, new(nameof(Question)));
                }
            }
        }
        public string? Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    PropertyChanged?.Invoke(this, new(nameof(Description)));
                }
            }
        }
    }
}
