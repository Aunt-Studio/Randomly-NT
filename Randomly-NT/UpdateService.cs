using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Media.Devices;

namespace Randomly_NT
{
    public class UpdateService
    {
        private readonly string _url = "https://github.com/Aunt-Studio/Randomly-NT/raw/refs/heads/main/version.json";
        public bool HasNewVersion { get; private set; } = false;
        public NewVersionMeta? NewVersionMeta { get; private set; }
        public delegate void NewVersionAvailableEventHandler(object sender, NewVersionAvailableEventArgs e);
        public event NewVersionAvailableEventHandler? NewVersionAvailable;

        public UpdateService()
        {
            
        }
        //public UpdateService(string url)
        //{
        //    this._url = url;
        //}

        /// <summary>
        /// 从服务器下载远程版本元数据，检查是否有新版本，如果有则触发 NewVersionAvailable 事件。
        /// </summary>
        /// <returns></returns>
        public async Task CheckUpdateAsync()
        {
            try
            {
                // 下载最新版本元数据
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(_url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        // 解析元数据
                        JObject metaJson = JObject.Parse(content);
                        string? packageVer = metaJson["packageVer"]?.ToString();
                        string? assemblyVer = metaJson["assemblyVer"]?.ToString();
                        string? downloadUrl = metaJson["downloadUrl"]?.ToString();
                        string? hasScriptFlag = metaJson["hasScript"]?.ToString();

                        if (Version.TryParse(packageVer, out var remotePackageVer) 
                            && Version.TryParse(assemblyVer, out var remoteAssemblyVer)
                            && downloadUrl is not null
                            && hasScriptFlag is not null)
                        {
                            VersionMeta remoteVersionMeta = new(remotePackageVer, remoteAssemblyVer);
                            // 比较版本号
                            if (IsNewVersionAvailable(remoteVersionMeta))
                            {
                                HasNewVersion = true;
                                bool hasScript = false;
                                if (hasScriptFlag == "true")
                                {
                                    hasScript = true;
                                }
                                NewVersionMeta = new(remoteVersionMeta.PackageVersion, remoteVersionMeta.AssemblyVersion,
                                                     downloadUrl, hasScript);
                                // 触发事件，更不更新前端自己看着办
                                NewVersionAvailable?.Invoke(this, new(NewVersionMeta));
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("检查更新时引发的异常\n" + ex.Message);
            }


        }

        private bool IsNewVersionAvailable(VersionMeta remoteVersionMeta)
        {
            bool available = false;
                if (Package.Current is not null)
                {
                    // 若程序打包，获取包版本号
                    var version = Package.Current.Id.Version;
                    Version packageVersion = new Version(version.Major, version.Minor, version.Build, version.Revision);
                    available = remoteVersionMeta.PackageVersion > packageVersion;
                }
                else
                {
                    // 无法获取包版本号（程序可能已经被解包？），获取程序集版本号
                    var assembly = Assembly.GetExecutingAssembly();
                    Version? assemblyVersion = assembly.GetName().Version;
                    if (assemblyVersion is not null)
                    {
                        available = remoteVersionMeta.AssemblyVersion > assemblyVersion;
                    }
                }
            return available;
        }
        public class NewVersionAvailableEventArgs : EventArgs
        {
            public NewVersionMeta NewVersionMeta { get; }
            public NewVersionAvailableEventArgs(NewVersionMeta newVersionMeta)
            {
                NewVersionMeta = newVersionMeta;
            }
        }

    }

    /// <summary>
    /// 版本元数据，将包版本号和程序集版本号封装在一起
    /// </summary>
    public class VersionMeta
    {
        public Version PackageVersion { get; }
        public Version AssemblyVersion { get; }

        public VersionMeta(Version packageVersion, Version assemblyVersion)
        {
            PackageVersion = packageVersion;
            AssemblyVersion = assemblyVersion;
        }
    }


    /// <summary>
    /// 必须*确保*该类的所有实例都是新版本，否则不应实例化该类。
    /// </summary>
    public class NewVersionMeta : VersionMeta
    {
        string DownloadUrl { get; set; }
        bool HasScript { get; set; }
        public NewVersionMeta(Version packageVersion, Version assemblyVersion, string downloadUrl, bool hasScript) : base(packageVersion, assemblyVersion)
        {
            DownloadUrl = downloadUrl;
            HasScript = hasScript;
        }

        public void Update()
        {
        }
    }
}
