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
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Concurrent;

namespace Randomly_NT
{
    public class UpdateService
    {
        private readonly string _url = "https://github.com/Aunt-Studio/Randomly-NT/raw/refs/heads/main/version.json";
        public bool HasNewVersion { get; private set; } = false;
        public NewVersionMeta? NewVersionMeta { get; private set; }
        public delegate void NewVersionAvailableEventHandler(object sender, NewVersionAvailableEventArgs e);
        public event NewVersionAvailableEventHandler? NewVersionAvailable;
        private int _retryTimes = 0;

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
                        string? hash = metaJson["hash"]?.ToString();
                        string? hasScriptFlag = metaJson["hasScript"]?.ToString();

                        if (Version.TryParse(packageVer, out var remotePackageVer)
                            && Version.TryParse(assemblyVer, out var remoteAssemblyVer)
                            && downloadUrl is not null
                            && hash is not null
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
                                                     downloadUrl, hash, hasScript);
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
                if (_retryTimes < 10)
                {
                    _retryTimes++;
                    await Task.Delay(1000);
                    CheckUpdateAsync();
                }
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
        public enum UpdateStatus
        {
            None,
            Downloading,
            Downloaded,
            Updating,
            Error
        }

        string DownloadUrl { get; set; }
        string Hash { get; set; }
        bool HasScript { get; set; }
        public UpdateStatus Status { get; private set; } = UpdateStatus.None;
        /// <summary>
        /// 错误信息，只有当 Status 为 Error 时才非 null
        /// </summary>
        public string? ErrorInfo { get; private set; }
        /// <summary>
        /// 确保运行过一次Download方法后才能访问
        /// </summary>
        public FileDownloader? FileDownloader { get; private set; }
        public event EventHandler<UpdateErrorEventArgs>? UpdateErrorOccurred;
        public NewVersionMeta(Version packageVersion, Version assemblyVersion, string downloadUrl, string hash, bool hasScript) : base(packageVersion, assemblyVersion)
        {
            DownloadUrl = downloadUrl;
            Hash = hash;
            HasScript = hasScript;
        }

        public async Task Download()
        {
            if (Status != UpdateStatus.None && Status != UpdateStatus.Error)
            {
                return;
            }

            Status = UpdateStatus.Downloading;
            try
            {
                var targetPath = Path.Combine(Path.GetTempPath(), $"Randomly-update-pkg.{PackageVersion}.zip");
                // 下载
                FileDownloader = new FileDownloader();
                FileDownloader.DownloadCompleted += (sender, e) =>
                {
                    // 下好了
                    Status = UpdateStatus.Downloaded;
                };
                await FileDownloader.DownloadAsync(DownloadUrl, targetPath, Hash, true);

            }
            catch (Exception ex)
            {
                Status = UpdateStatus.Error;
                ErrorInfo = ex.Message;
                Debug.WriteLine("下载更新异常: \n" + ex.ToString());
                UpdateErrorOccurred?.Invoke(this, new("下载更新时发生异常:" + ex.Message, ex));
            }
            return;
        }
        /// <summary>
        /// 安装更新。该方法会在结束后强制退出程序。
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            if (Status != UpdateStatus.None && Status != UpdateStatus.Downloaded)
            {
                // 下载中或更新中
                return;
            }
            else if (Status != UpdateStatus.Downloaded)
            {
                // 还没下载
                await Download();
            }
            // 等待下载完成
            while (Status != UpdateStatus.Downloaded)
            {
                await Task.Delay(400);
                if (Status == UpdateStatus.Error)
                {
                    return;
                }
            }
            // 下好了

            // 此处作为测试，在资源管理器中打开文件
            Process.Start("explorer.exe", Path.Combine(Path.GetTempPath(), $"Randomly-update-pkg.{PackageVersion}.zip"));

            // 解压

            // 触发更新器安装更新
        }
    }
    public class UpdateErrorEventArgs : EventArgs
    {
        public string Message { get; }
        public Exception Exception { get; }

        public UpdateErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            this.Exception = exception;
        }
    }
    public class FileDownloader : IDisposable
    {
        #region 事件定义
        public event EventHandler<DownloadProgressEventArgs>? ProgressChanged;
        public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;
        #endregion

        #region 状态管理与对象实例
        private readonly string _stateFilePath;
        private readonly HttpClient _client;
        private bool _isResuming;
        private long _totalBytes;
        private long _downloadedBytes;
        private CancellationTokenSource? _cts;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();

        #endregion

        public FileDownloader()
        {
            _client = new HttpClient();
            _stateFilePath = Path.Combine(Path.GetTempPath(), "DownloadStates");
            Directory.CreateDirectory(_stateFilePath);
        }

        /// <summary>
        /// 开始/恢复下载
        /// </summary>
        public async Task DownloadAsync(
            string url,
            string savePath,
            string? expectedHash = null,
            bool enableResume = true)
        {
            try
            {
                if (File.Exists(savePath))
                {
                    using (FileStream stream = File.OpenRead(savePath))
                    {
                        SHA256 sha256 = SHA256.Create();
                        byte[] hashBytes = sha256.ComputeHash(stream);
                        if (expectedHash is not null && expectedHash.Equals(BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                        {
                            await Task.Delay(300);
                            ProgressChanged?.Invoke(this, new DownloadProgressEventArgs(
                                _totalBytes,
                                _totalBytes,
                                _isResuming));
                            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(savePath));
                            return;
                        }
                    }
                }
                _cts = new CancellationTokenSource();
                var tempFile = Path.Combine(_stateFilePath, GetTempFileName(url));
                var stateFile = tempFile + ".state";

                // 加载下载状态
                var (fileSize, startPosition) = enableResume ?
                    await TryResume(stateFile) : (0L, 0L);

                using var response = await GetResponse(url, startPosition, enableResume);
                ValidateResponse(response, enableResume);

                // 初始化下载参数
                _totalBytes = fileSize > 0 ? fileSize : response.Content.Headers.ContentLength ?? -1;
                _downloadedBytes = startPosition;
                _isResuming = startPosition > 0;

                // 创建文件流
                await using var fileStream = new FileStream(
                    tempFile,
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.Delete,
                    bufferSize: 8192,
                    useAsync: true);

                fileStream.Seek(startPosition, SeekOrigin.Begin);

                // 开始下载
                await ProcessContentStream(
                    await response.Content.ReadAsStreamAsync(),
                    fileStream,
                    stateFile,
                    expectedHash,
                    tempFile,
                    savePath,
                    _cts.Token);
            }
            catch (OperationCanceledException)
            {
                CleanupOnCancel();
                throw;
            }
        }

        #region 核心逻辑实现
        private async Task<(long FileSize, long StartPosition)> TryResume(string stateFile)
        {
            if (!File.Exists(stateFile)) return (0, 0);

            var state = JsonConvert.DeserializeObject<DownloadState>(
                await File.ReadAllTextAsync(stateFile));
            return (state.FileSize, state.DownloadedBytes);
        }

        private async Task<HttpResponseMessage> GetResponse(
            string url,
            long startPosition,
            bool enableResume)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (enableResume)
            {
                request.Headers.Range = new RangeHeaderValue(startPosition, null);
            }
            return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        private void ValidateResponse(HttpResponseMessage response, bool enableResume)
        {
            if (enableResume && response.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new HttpRequestException("服务器不支持断点续传");
            }
        }

        private async Task ProcessContentStream(
            Stream contentStream,
            FileStream fileStream,
            string stateFile,
            string? expectedHash,
            string tempFile,
            string savePath,
            CancellationToken ct)
        {
            var buffer = new byte[8192];
            var lastSaveTime = DateTime.MinValue;
            var hasher = expectedHash != null ? SHA256.Create() : null;

            try
            {
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                    _downloadedBytes += bytesRead;
                    hasher?.TransformBlock(buffer, 0, bytesRead, null, 0);

                    // 每500ms或每1%进度保存状态
                    if ((DateTime.Now - lastSaveTime).TotalMilliseconds > 500 ||
                        (_downloadedBytes * 100 / _totalBytes) % 1 == 0)
                    {
                        await SaveState(stateFile);
                        lastSaveTime = DateTime.Now;
                    }

                    ProgressChanged?.Invoke(this, new DownloadProgressEventArgs(
                        _downloadedBytes,
                        _totalBytes,
                        _isResuming));
                }

                // 最终校验
                hasher?.TransformFinalBlock(buffer, 0, 0);
                if (expectedHash is not null &&
                    !VerifyHash(hasher?.Hash!, expectedHash))
                {
                    throw new InvalidDataException("文件校验失败");
                }

                // 移动临时文件
                File.Move(tempFile, savePath, true);
                DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(savePath));
            }
            finally
            {
                Cleanup(tempFile, stateFile);
                hasher?.Dispose();
            }
        }
        #endregion

        #region 辅助方法
        private string GetTempFileName(string url) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(url)))
                   .Replace("/", "_")
                   .Replace("+", "-")
                   .Substring(0, 16);

        private async Task SaveState(string stateFile)
        {
            var state = new DownloadState
            {
                FileSize = _totalBytes,
                DownloadedBytes = _downloadedBytes,
                Timestamp = DateTime.UtcNow
            };
            await File.WriteAllTextAsync(stateFile, JsonConvert.SerializeObject(state));
        }

        private bool VerifyHash(byte[] actualHash, string expectedHash) =>
            actualHash != null &&
            BitConverter.ToString(actualHash)
                .Replace("-", "")
                .Equals(expectedHash, StringComparison.OrdinalIgnoreCase);

        private static void SafeDelete(string path, int maxRetries = 3, int delayMs = 100)
        {
            if (!File.Exists(path)) return;

            // 获取文件专属锁
            var fileLock = _fileLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
            try
            {
                fileLock.Wait();

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        File.Delete(path);
                        return;
                    }
                    catch (IOException) when (i < maxRetries - 1)
                    {
                        // 等待释放锁
                        Thread.Sleep(delayMs * (int)Math.Pow(2, i));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // 尝试移除只读属性
                        File.SetAttributes(path, FileAttributes.Normal);
                    }
                }
            }
            finally
            {
                fileLock.Release();
                _fileLocks.TryRemove(path, out _);
            }
        }

        private void Cleanup(string tempFile, string stateFile)
        {
            SafeDelete(stateFile);
            SafeDelete(tempFile);
        }

        private void CleanupOnCancel()
        {
            // 保留临时文件但删除无效状态
            var tempFiles = Directory.GetFiles(_stateFilePath, "*.tmp");
            foreach (var file in tempFiles)
            {
                var stateFile = file + ".state";
                if (File.Exists(stateFile))
                {
                    // 验证状态有效性
                    try
                    {
                        var state = JsonConvert.DeserializeObject<DownloadState>(
                            File.ReadAllText(stateFile));
                        if (state.DownloadedBytes == new FileInfo(file).Length)
                        {
                            continue; // 保留有效状态
                        }
                    }
                    catch {}
                    File.Delete(stateFile);
                }
            }

            // 清理超过7天的残留文件
            CleanLegacyFiles();
        }

        private void CleanLegacyFiles()
        {
            var cutoff = DateTime.Now.AddDays(-7);
            foreach (var file in Directory.GetFiles(_stateFilePath))
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (fi.LastWriteTime < cutoff)
                    {
                        fi.Delete();
                        File.Delete(file + ".state");
                    }
                }
                catch { }
            }
        }

        public void Cancel() => _cts?.Cancel();

        public void Dispose() => _client?.Dispose();
        #endregion
    }

    #region 事件参数类
    public class DownloadProgressEventArgs : EventArgs
    {
        public long BytesReceived { get; }
        public long TotalBytes { get; }
        public double ProgressPercentage => TotalBytes > 0 ?
            (double)BytesReceived / TotalBytes * 100 : 0;
        public bool IsResuming { get; }

        public DownloadProgressEventArgs(long bytes, long total, bool isResuming)
        {
            BytesReceived = bytes;
            TotalBytes = total;
            IsResuming = isResuming;
        }
    }

    public class DownloadCompletedEventArgs : EventArgs
    {
        public string FilePath { get; }

        public DownloadCompletedEventArgs(string path)
        {
            FilePath = path;
        }
    }

    public class DownloadState
    {
        public long FileSize { get; set; }
        public long DownloadedBytes { get; set; }
        public DateTime Timestamp { get; set; }
    }
    #endregion


}
