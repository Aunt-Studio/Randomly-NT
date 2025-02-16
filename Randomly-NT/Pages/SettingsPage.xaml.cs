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
        /// 本地设置容器
        /// </summary>
        private ApplicationDataContainer LocalSettings;

        /// <summary>
        /// 属性更改事件回调函数
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 用以 UI 展示的程序集版本
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 用于更新显示的当前版本
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
        /// 随机性指数 字段
        /// </summary>
        private int _randomizeIndex = 0;

        /// <summary>
        /// 随机性指数 属性
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
        /// 更新服务单例实例
        /// </summary>
        private readonly UpdateService _updateServiceInstance;

        /// <summary>
        /// 随机性熵源前端展示列表 字段
        /// </summary>
        private ObservableCollection<RandomEntropyItem> _entropyItems = new();
        /// <summary>
        /// 随机性熵源前端展示列表 属性
        /// </summary>
        public ObservableCollection<RandomEntropyItem> EntropyItems { 
            get => _entropyItems;
            set
            {
                _entropyItems = value;
                OnPropertyChanged(nameof(EntropyItems));
            } 
        }

        #region 随机性熵源前端展示列表项定义

        private RandomEntropyItem clock = new()
        {
            Name = "默认随机数种子",
            Description = "系统 API 提供的默认随机数种子, 即 CPU 时钟。",
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#mo-ren-sui-ji-shu-zhong-zi"
        };

        private RandomEntropyItem runtime = new()
        {
            Name = "运行时环境噪声",
            Description = "可能包含系统状态信息，如内存使用情况等。",
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#yun-xing-shi-huan-jing-zao-sheng"
        };

        private RandomEntropyItem mouse = new()
        {
            Name = "鼠标指针",
            Description = "鼠标指针的位置。",
            DocumentUrl = "https://docs.auntstudio.com/randomly-nt/concepts/entropy-sources#shu-biao-zhi-zhen"
        };

        private RandomEntropyItem randomOrg = new()
        {
            Name = "真随机数接口 (需要访问网络)",
            Description = "从 Random.org 获取的利用大气噪声生成的真随机数。获取该熵可能会产生一定的时间开销。\n应用该熵后，随机数结果可认为是真随机。频繁抽取可能会导致 Random.org 限制。",
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

            // 获取本地设置容器
            LocalSettings = ApplicationData.Current.LocalSettings;
            // 读取随机性指数
            _randomizeIndex = LocalSettings.Values.ContainsKey("RandomizeIndex") ? (int)LocalSettings.Values["RandomizeIndex"] : 1;

            // 获取程序集版本
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
                    var assemblyVer = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "未知程序集版本";
                    Version = $"Assembly Version {assemblyVer} (Unpackaged)";
                }
            }
            CurrentVersion = "当前版本: " + Version;
#if DEBUG
            Version += " Debug";
#endif

            // 设置数据上下文
            this.DataContext = this;

            // 配置随机性熵源前端列表展示
            ConfigureRandomizationFactors();
        }

        public void OnUpdateAvailable(object sender, UpdateService.NewVersionAvailableEventArgs e)
        {
            NewVersionSE.Visibility = Visibility.Visible;
            RemoteVersion = "最新版本: " + e.NewVersionMeta.PackageVersion.ToString();
            UpdateButtonContent(e.NewVersionMeta);

        }

        private void UpdateButtonContent(NewVersionMeta newVersionMeta)
        {
            switch (newVersionMeta.Status)
            {
                case NewVersionMeta.UpdateStatus.None:
                    ApplyUpdateButton.IsEnabled = true;
                    ApplyUpdateButton.Content = "下载更新包";
                    break;
                case NewVersionMeta.UpdateStatus.Downloading:
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "下载中...";
                    break;
                case NewVersionMeta.UpdateStatus.Downloaded:
                    ApplyUpdateButton.IsEnabled = true;
                    ApplyUpdateButton.Content = "安装更新 (将关闭程序)";
                    break;
                case NewVersionMeta.UpdateStatus.Updating:
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "安装中...";
                    break;
                case NewVersionMeta.UpdateStatus.Error:
                    ApplyUpdateButton.IsEnabled = true;
                    ApplyUpdateButton.Content = "重新尝试下载";
                    break;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            // 通知前端属性更改
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // 保存随机性指数设置更改
            if (propertyName == nameof(RandomizeIndex))
            {
                LocalSettings.Values["RandomizeIndex"] = RandomizeIndex;
            }
        }

        /// <summary>
        /// 配置随机性熵源前端展示列表
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
                    ApplyUpdateButton.Content = "下载中...";
                    DownloadingSC.Visibility = Visibility.Visible;
                    _ = _updateServiceInstance.NewVersionMeta.Download();
                    _updateServiceInstance.NewVersionMeta.UpdateErrorOccurred += (sender, e) =>
                    {
                        ErrorMessageTB.Text = e.Message;
                        ErrorDetailsTB.Text = e.Exception.ToString();
                        DownloadingErrorSC.Visibility = Visibility.Visible;
                        ApplyUpdateButton.IsEnabled = true;
                        ApplyUpdateButton.Content = "重新尝试下载";
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
                        ApplyUpdateButton.Content = "安装更新 (将关闭程序)";
                        ApplyUpdateButton.IsEnabled = true;
                        DownloadingSC.Visibility = Visibility.Collapsed;
                    };

                }
                else if (_updateServiceInstance.NewVersionMeta.Status == NewVersionMeta.UpdateStatus.Downloaded)
                {
                    ApplyUpdateButton.IsEnabled = false;
                    ApplyUpdateButton.Content = "安装中...";
                    await _updateServiceInstance.NewVersionMeta.Update();
                }
            }
        }
    }

    /// <summary>
    /// 随机性熵源前端展示列表项
    /// </summary>
    public class RandomEntropyItem
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }
    }
}
