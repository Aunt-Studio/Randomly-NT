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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Randomly_NT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public string AssemblyVersion { get; }
        public int RandomizeIndex { get; set; } = 0;
        public ObservableCollection<RandomizationFactorItem> RandomizationFactorItems = [];
        public SettingsPage()
        {
            this.InitializeComponent();
            AssemblyVersion = "Assembly Version ";
            AssemblyVersion += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "δ֪���򼯰汾";
            this.DataContext = this;
            // ��������
            RandomizationFactorItems.Add(new RandomizationFactorItem
            {
                Name = "�Զ����������",
                Description = "ϵͳ API �ṩ���Զ����������, �� CPU ʱ��",
                DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
            });
        }

        private void RandomizeIndexSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Debug.WriteLine(RandomizeIndex);
        }
    }

    public class RandomizationFactorItem
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }
    }
}
