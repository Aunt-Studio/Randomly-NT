using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
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
        /// <summary>
        /// ������������
        /// </summary>
        private ApplicationDataContainer LocalSettings;

        /// <summary>
        /// ���Ը����¼��ص�����
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// ���� UI չʾ�ĳ��򼯰汾
        /// </summary>
        public string AssemblyVersion { get; }

        /// <summary>
        /// �����ָ�� �ֶ�
        /// </summary>
        private int _randomizeIndex = 0;

        /// <summary>
        /// �����ָ�� ����
        /// </summary>
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

        /// <summary>
        /// �������Դǰ��չʾ�б� �ֶ�
        /// </summary>
        private ObservableCollection<RandomEntropyItem> _entropyItems = new();
        /// <summary>
        /// �������Դǰ��չʾ�б� ����
        /// </summary>
        public ObservableCollection<RandomEntropyItem> EntropyItems { 
            get => _entropyItems;
            set
            {
                _entropyItems = value;
                OnPropertyChanged(nameof(EntropyItems));
            } 
        }

        #region �������Դǰ��չʾ�б����

        private RandomEntropyItem clock = new()
        {
            Name = "Ĭ�����������",
            Description = "ϵͳ API �ṩ��Ĭ�����������, �� CPU ʱ�ӡ�",
            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
        };

        private RandomEntropyItem runtime = new()
        {
            Name = "����ʱ��������",
            Description = "���ܰ���ϵͳ״̬��Ϣ�����ڴ�ʹ������ȡ�",
            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
        };

        private RandomEntropyItem mouse = new()
        {
            Name = "���ָ��",
            Description = "���ָ���λ�á�",
            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
        };

        private RandomEntropyItem randomOrg = new()
        {
            Name = "��������ӿ� (��Ҫ��������)",
            Description = "�� Random.org ��ȡ�����ô����������ɵ������������ȡ���ؿ��ܻ����һ����ʱ�俪����\nӦ�ø��غ�������������Ϊ���������Ƶ����ȡ���ܻᵼ�� Random.org ���ơ�",
            DocumentUrl = "https://docs.microsoft.com/zh-cn/dotnet/api/system.random"
        };

        #endregion

        public SettingsPage()
        {
            this.InitializeComponent();
            // ��ȡ������������
            LocalSettings = ApplicationData.Current.LocalSettings;
            // ��ȡ�����ָ��
            _randomizeIndex = LocalSettings.Values.ContainsKey("RandomizeIndex") ? (int)LocalSettings.Values["RandomizeIndex"] : 1;

            // ��ȡ���򼯰汾
            AssemblyVersion = "Assembly Version ";
            AssemblyVersion += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "δ֪���򼯰汾";

            // ��������������
            this.DataContext = this;

            // �����������Դǰ���б�չʾ
            ConfigureRandomizationFactors();
        }

        private void OnPropertyChanged(string propertyName)
        {
            // ֪ͨǰ�����Ը���
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // ���������ָ�����ø���
            if (propertyName == nameof(RandomizeIndex))
            {
                LocalSettings.Values["RandomizeIndex"] = RandomizeIndex;
            }
        }

        /// <summary>
        /// �����������Դǰ��չʾ�б�
        /// </summary>
        private void ConfigureRandomizationFactors()
        {
            switch (RandomizeIndex)
            {
                case 0 or 1:
                    EntropyItems = [clock];
                    break;
                case 2:
                    EntropyItems = [clock, runtime];
                    break;
                case 3:
                    EntropyItems = [clock, runtime, mouse];
                    break;
                case 4:
                    EntropyItems = [clock, runtime, mouse, randomOrg];
                    break;
                default:
                    break;
            }
        }

    }

    /// <summary>
    /// �������Դǰ��չʾ�б���
    /// </summary>
    public class RandomEntropyItem
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }
    }
}
