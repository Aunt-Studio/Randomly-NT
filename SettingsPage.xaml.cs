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
using System.ComponentModel;
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
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string AssemblyVersion { get; }
        private int _randomizeIndex = 0;
        public int RandomizeIndex { 
            get => _randomizeIndex;
            set
            {
                if (_randomizeIndex != value) 
                {
                    _randomizeIndex = value;
                    OnPropertyChanged(nameof(RandomizeIndex));
                    ConfigureRandomizationFactors();
                }
            } }
        private ObservableCollection<RandomizationFactorItem> _randomizationFactorItems = new();
        public ObservableCollection<RandomizationFactorItem> RandomizationFactorItems { 
            get => _randomizationFactorItems;
            set
            {
                _randomizationFactorItems = value;
                OnPropertyChanged(nameof(RandomizationFactorItems));
            } 
        }
        public SettingsPage()
        {
            this.InitializeComponent();
            AssemblyVersion = "Assembly Version ";
            AssemblyVersion += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "δ֪���򼯰汾";
            this.DataContext = this;
            ConfigureRandomizationFactors();
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ConfigureRandomizationFactors()
        {
            switch (RandomizeIndex)
            {
                case 0 or 1:
                    RandomizationFactorItems = new ObservableCollection<RandomizationFactorItem>
                    {
                        new RandomizationFactorItem
                        {
                            Name = "Ĭ�����������",
                            Description = "ϵͳ API �ṩ��Ĭ�����������, �� CPU ʱ�ӡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        }
                    };
                    break;
                case 2:
                    RandomizationFactorItems = new ObservableCollection<RandomizationFactorItem>
                    {
                        new RandomizationFactorItem
                        {
                            Name = "Ĭ�����������",
                            Description = "ϵͳ API �ṩ��Ĭ�����������, �� CPU ʱ�ӡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        },
                        new RandomizationFactorItem
                        {
                            Name = "����ʱ��������",
                            Description = "���ܰ���ϵͳ״̬��Ϣ�����ڴ�ʹ������ȡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        }
                    };
                    break;
                case 3:
                    RandomizationFactorItems = new ObservableCollection<RandomizationFactorItem>
                    {
                        new RandomizationFactorItem
                        {
                            Name = "Ĭ�����������",
                            Description = "ϵͳ API �ṩ��Ĭ�����������, �� CPU ʱ�ӡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        },
                        new RandomizationFactorItem
                        {
                            Name = "����ʱ��������",
                            Description = "���ܰ���ϵͳ״̬��Ϣ�����ڴ�ʹ������ȡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        },
                        new RandomizationFactorItem
                        {
                            Name = "���ָ��",
                            Description = "���ָ���λ�á�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        }
                    };
                    break;
                case 4:
                    RandomizationFactorItems = new ObservableCollection<RandomizationFactorItem>
                    {
                        new RandomizationFactorItem
                        {
                            Name = "Ĭ�����������",
                            Description = "ϵͳ API �ṩ��Ĭ�����������, �� CPU ʱ�ӡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        },
                        new RandomizationFactorItem
                        {
                            Name = "����ʱ��������",
                            Description = "���ܰ���ϵͳ״̬��Ϣ�����ڴ�ʹ������ȡ�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        },
                        new RandomizationFactorItem
                        {
                            Name = "���ָ��",
                            Description = "���ָ���λ�á�",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        },
                        new RandomizationFactorItem
                        {
                            Name = "��������ӿ� (��Ҫ��������)",
                            Description = "�� Random.org ��ȡ�����ô����������ɵ������������ȡ���ؿ��ܻ����һ����ʱ�俪����",
                            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
                        }
                    };
                    break;
                default:
                    break;
            }
        }

        //private void RandomizeIndexSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
            
        //}
    }

    public class RandomizationFactorItem
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }
    }
}
