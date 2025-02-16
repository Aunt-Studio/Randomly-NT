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
using Windows.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

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
        public string Version { get; }

        /// <summary>
        /// ���ڸ�����ʾ�ĵ�ǰ�汾
        /// </summary>
        public string CurrentVersion { get; }

        private string? _remoteVersion;
        public string? RemoteVersion
        {
            get => _remoteVersion;
            private set
            {
                if (_remoteVersion != value)
                {
                    _remoteVersion = value;
                    OnPropertyChanged(nameof(RemoteVersion));
                }
            }
        }

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
            } 
        }

        /// <summary>
        /// ���·�����ʵ��
        /// </summary>
        private readonly UpdateService _updateServiceInstance;

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
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#mo-ren-sui-ji-shu-zhong-zi"
        };

        private RandomEntropyItem runtime = new()
        {
            Name = "����ʱ��������",
            Description = "���ܰ���ϵͳ״̬��Ϣ�����ڴ�ʹ������ȡ�",
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#yun-xing-shi-huan-jing-zao-sheng"
        };

        private RandomEntropyItem mouse = new()
        {
            Name = "���ָ��",
            Description = "���ָ���λ�á�",
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#shu-biao-zhi-zhen"
        };

        private RandomEntropyItem randomOrg = new()
        {
            Name = "��������ӿ� (��Ҫ��������)",
            Description = "�� Random.org ��ȡ�����ô����������ɵ������������ȡ���ؿ��ܻ����һ����ʱ�俪����\nӦ�ø��غ�������������Ϊ���������Ƶ����ȡ���ܻᵼ�� Random.org ���ơ�",
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#zhen-sui-ji-shu-jie-kou-xu-yao-fang-wen-wang-luo"
        };
        

        #endregion

        public SettingsPage()
        {
            this.InitializeComponent();

            _updateServiceInstance = App.Host!.Services.GetRequiredService<UpdateService>();
            if (_updateServiceInstance.HasNewVersion && _updateServiceInstance.NewVersionMeta is not null)
            {
                OnUpdateAvailable(this, new(_updateServiceInstance.NewVersionMeta));
            }
            _updateServiceInstance.NewVersionAvailable += OnUpdateAvailable;

            // ��ȡ������������
            LocalSettings = ApplicationData.Current.LocalSettings;
            // ��ȡ�����ָ��
            _randomizeIndex = LocalSettings.Values.ContainsKey("RandomizeIndex") ? (int)LocalSettings.Values["RandomizeIndex"] : 1;

            // ��ȡ���򼯰汾
            if (Package.Current is not null)
            {
                var version = Package.Current.Id.Version;
                Version = $"Package Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

                if (!string.IsNullOrEmpty(infoVersion))
                {
                    Version = $"Assembly Info Version {infoVersion} (Unpackaged)";
                }
                else
                {
                    var assemblyVer = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "δ֪���򼯰汾";
                    Version = $"Assembly Version {assemblyVer} (Unpackaged)";
                }
            }
            CurrentVersion = "��ǰ�汾: " + Version;
#if DEBUG
            Version += " Debug";
#endif

            // ��������������
            this.DataContext = this;

            // �����������Դǰ���б�չʾ
            ConfigureRandomizationFactors();
        }

        public void OnUpdateAvailable(object sender, UpdateService.NewVersionAvailableEventArgs e)
        {
            NewVersionSE.Visibility = Visibility.Visible;
            RemoteVersion = "���°汾: " + e.NewVersionMeta.PackageVersion.ToString();
            UpdateButtonContent(e.NewVersionMeta);

        }

        private void UpdateButtonContent(NewVersionMeta newVersionMeta)
        {
            switch (newVersionMeta.Status)
            {
                case NewVersionMeta.UpdateStatus.None:
                    ApplyUpdateButton.IsEnabled = true;
                    ApplyUpdateButton.Content = "���ظ��°�";
                    break;
                case NewVersionMeta.UpdateStatus.Downloading:
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "������...";
                    break;
                case NewVersionMeta.UpdateStatus.Downloaded:
                    ApplyUpdateButton.IsEnabled = true;
                    ApplyUpdateButton.Content = "��װ���� (���رճ���)";
                    break;
                case NewVersionMeta.UpdateStatus.Updating:
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "��װ��...";
                    break;
                case NewVersionMeta.UpdateStatus.Error:
                    ApplyUpdateButton.IsEnabled = true;
                    ApplyUpdateButton.Content = "���³�������";
                    break;
            }
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

        private async void ApplyUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_updateServiceInstance.NewVersionMeta is not null)
            {
                if (_updateServiceInstance.NewVersionMeta.Status == NewVersionMeta.UpdateStatus.None
                 || _updateServiceInstance.NewVersionMeta.Status == NewVersionMeta.UpdateStatus.Error)
                {
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "������...";
                    DownloadingSC.Visibility = Visibility.Visible;
                    _ = _updateServiceInstance.NewVersionMeta.Download();
                    _updateServiceInstance.NewVersionMeta.UpdateErrorOccurred += (sender, e) =>
                    {
                        ErrorMessageTB.Text = e.Message;
                        ErrorDetailsTB.Text = e.Exception.ToString();
                        DownloadingErrorSC.Visibility = Visibility.Visible;
                        ApplyUpdateButton.IsEnabled = true;
                        ApplyUpdateButton.Content = "���³�������";
                    };
                    while (_updateServiceInstance.NewVersionMeta.FileDownloader is null)
                    {
                        
                    }

                    _updateServiceInstance.NewVersionMeta.FileDownloader.ProgressChanged += (sender, e) =>
                    {
                        UpdateButtonContent(_updateServiceInstance.NewVersionMeta);
                        BytesReceived.Text = $"{((int)e.ProgressPercentage)} %";
                        DownloadProgressBar.Value = e.ProgressPercentage;
                        Debug.WriteLine(e.ProgressPercentage);
                    };
                    _updateServiceInstance.NewVersionMeta.FileDownloader.DownloadCompleted += (sender, e) =>
                    {
                        Debug.WriteLine(e.FilePath);
                        ApplyUpdateButton.Content = "��װ���� (���رճ���)";
                        ApplyUpdateButton.IsEnabled = true;
                        DownloadingSC.Visibility = Visibility.Collapsed;
                    };

                }
                else if (_updateServiceInstance.NewVersionMeta.Status == NewVersionMeta.UpdateStatus.Downloaded)
                {
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "��װ��...";
                    await _updateServiceInstance.NewVersionMeta.Update();
                }
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
