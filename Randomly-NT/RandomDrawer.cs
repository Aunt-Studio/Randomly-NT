using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Randomly_NT
{
    internal static class RandomDrawer
    {
        #region CursorPositionGetRegion
        // 使用 P/Invoke 调用 Windows API 获取鼠标位置
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        #endregion

        #region 种子生成器
        public static Random InitializeRandomGenerator(RandomEntropySource[] randomEntropySources)
        {
            Random random;
            if (randomEntropySources.Length <= 1 && randomEntropySources[0] == RandomEntropySource.SystemClock)
            {
                // 基本情况: 使用系统时钟作为随机数种子
                random = new Random();
            }
            else
            {
                long seed = 0;
                foreach (var source in randomEntropySources)
                {
                    switch (source)
                    {
                        case RandomEntropySource.SystemClock:
                            // 使用系统时钟作为随机数种子
                            seed += Environment.TickCount64;
                            break;
                        case RandomEntropySource.RuntimeNoise:
                            // 使用运行时噪声作为随机数种子
                            seed += GC.GetTotalMemory(false).GetHashCode();
                            seed += Process.GetCurrentProcess().WorkingSet64.GetHashCode();
                            seed += Environment.CurrentManagedThreadId.GetHashCode();
                            break;
                        case RandomEntropySource.MousePoint:
                            // 使用鼠标位置作为随机数种子
                            if (GetCursorPos(out POINT point))
                            {
                                seed += point.X.GetHashCode();
                                seed += point.Y.GetHashCode();
                            }
                            else
                            {
                                seed += Environment.TickCount64.GetHashCode();
                            }
                            break;
                        case RandomEntropySource.RandomOrg:
                            // 使用 Random.org 作为随机数种子
                            try
                            {
                                var task = Task.Run(async () => await GetTrueRandomNumberFromRandomOrg(1, 10000));
                                task.Wait();
                                seed += task.Result.GetHashCode();
                            }
                            catch
                            {
                                throw;
                            }
                            break;
                        default:
                            seed += Environment.TickCount64;
                            break;
                    }
                }
                Debug.WriteLine(seed);
                random = new Random((int)seed);
            }

            return random;
        }
        #endregion

        #region 一般随机数实现

        /// <summary>
        /// 生成若干个随机整数，范围为[<paramref Name="min"/>, <paramref Name="max"/>]，取值将包含 <paramref Name="min"/> 和 <paramref Name="max"/>。
        /// <para>
        /// 将应用传入的熵源。仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。
        /// </para>
        /// </summary>
        /// <param Name="min">最小取值</param>
        /// <param Name="max">最大取值</param>
        /// <param Name="count">取值数</param>
        /// <param Name="randomEntropySources">熵源,仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。</param>
        /// <returns>随机整数列表</returns>
        public static List<int> DrawRandomInt(int min, int max, int count, params RandomEntropySource[] randomEntropySources)
        {

            List<int> result = new List<int>();
            Random random;
            random = InitializeRandomGenerator(randomEntropySources);

            for (int i = 0; i < count; i++)
            {
                result.Add(random.Next(min, max + 1));
            }
            return result;
        }

        

        /// <summary>
        /// 生成若干个随机整数，范围为[<paramref Name="min"/>, <paramref Name="max"/>]，即可能取值将包含 <paramref Name="min"/> 和 <paramref Name="max"/>。
        /// <para>
        /// 将应用传入的熵源。仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。
        /// </para>
        /// </summary>
        /// <param Name="min">最小取值</param>
        /// <param Name="max">最大取值</param>
        /// <param Name="count">取值数</param>
        /// <param Name="randomEntropySources">熵源,仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。</param>
        /// <returns>随机整数列表</returns>
        public static async Task<List<int>> DrawRandomIntAsync(int min, int max, int count, params RandomEntropySource[] randomEntropySources)
        {
            return await Task.Run(() =>
            {
                List<int> result = DrawRandomInt(min, max, count, randomEntropySources);
                return result;
            });
        }

        /// <summary>
        /// 生成若干个随机整数，范围为[<paramref Name="min"/>, <paramref Name="max"/>]，即可能取值将包含 <paramref Name="min"/> 和 <paramref Name="max"/>。
        /// <para>
        /// 使用此重载方法应注意线程安全问题。避免同时多次执行传入同一个<paramref Name="resultList"/>的该函数。
        /// </para>
        /// <para>
        /// 将应用传入的熵源。仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。
        /// </para>
        /// 另外<paramref Name="resultList"/>列表将此重载方法执行时被清空。
        /// </summary>
        /// <param Name="min">最小取值</param>
        /// <param Name="max">最大取值</param>
        /// <param Name="count">取值数</param>
        /// <param Name="resultList">用于存储结果的可监视集合, 注意每次函数执行时都会自动清空集合里的内容。</param>
        /// <param Name="randomEntropySources">熵源,仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。</param>
        /// <returns>任务</returns>
        public static async Task DrawRandomIntAsync(int min, int max, int count, ICollection<int> resultList, params RandomEntropySource[] randomEntropySources)
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            if (count > 1000)
            {
                // 待生成的随机数数量过多，启用分批处理，以避免在拷贝到 ObservableCollection 时导致 UI 线程卡顿
                await Task.Run(async () =>
                {
                    List<int> tempList = new List<int>();

                    tempList = await DrawRandomIntAsync(min, max, count, randomEntropySources);
                    int batchSize = 100; // 每次处理的批次大小
                    int totalBatches = (int)Math.Ceiling((double)tempList.Count / batchSize);

                    dispatcherQueue.TryEnqueue(() => resultList.Clear());

                    for (int batch = 0; batch < totalBatches; batch++)
                    {
                        var currentBatch = tempList.Skip(batch * batchSize).Take(batchSize).ToList();
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            foreach (var number in currentBatch)
                            {
                                resultList.Add(number);
                            }
                        });

                        // 等待一小段时间以便UI线程有机会处理其他任务
                        await Task.Delay(10);
                    }
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    List<int> tempList = new List<int>();
                    tempList = await DrawRandomIntAsync(min, max, count, randomEntropySources);
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        resultList.Clear();
                        foreach (var number in tempList)
                        {
                            resultList.Add(number);
                        }
                    });
                });
            }
        }

        #endregion


        #region 唯一随机数实现
        /// <summary>
        /// 生成若干个不重复的随机整数，范围为[<paramref Name="min"/>, <paramref Name="max"/>]，即可能取值将包含 <paramref Name="min"/> 和 <paramref Name="max"/>。
        /// <para>
        /// 将应用传入的熵源。仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。
        /// </para>
        /// 当<paramref Name="max"/> - <paramref Name="min"/> + 1 小于 <paramref Name="count"/>时，将抛出<seealso cref="ArgumentException"/>异常。
        /// </summary>
        /// <param Name="min">最小取值</param>
        /// <param Name="max">最大取值</param>
        /// <param Name="count">取值数</param>
        /// <param Name="randomEntropySources">熵源,仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。</param>
        /// <returns>随机整数列表</returns>
        public static HashSet<int> DrawUniqueRandomInt(int min, int max, int count, params RandomEntropySource[] randomEntropySources)
        {

            HashSet<int> result = new HashSet<int>();
            Random random;
            random = InitializeRandomGenerator(randomEntropySources);

            while (result.Count < count)
            {
                result.Add(random.Next(min, max + 1));
            }
            return result;
        }

        /// <summary>
        /// 生成若干个不重复的随机整数，范围为[<paramref Name="min"/>, <paramref Name="max"/>]，即可能取值将包含 <paramref Name="min"/> 和 <paramref Name="max"/>。
        /// <para>
        /// 将应用传入的熵源。仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。
        /// </para>
        /// 当<paramref Name="max"/> - <paramref Name="min"/> + 1 小于 <paramref Name="count"/>时，将抛出<seealso cref="ArgumentException"/>异常。
        /// </summary>
        /// <param Name="min">最小取值</param>
        /// <param Name="max">最大取值</param>
        /// <param Name="count">取值数</param>
        /// <param Name="randomEntropySources">熵源,仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。</param>
        /// <returns>随机整数列表</returns>
        public static async Task<HashSet<int>> DrawUniqueRandomIntAsync(int min, int max, int count, params RandomEntropySource[] randomEntropySources)
        {
            return await Task.Run(() =>
            {
                HashSet<int> result = DrawUniqueRandomInt(min, max, count, randomEntropySources);
                return result;
            });
        }

        /// <summary>
        /// 生成若干个不重复的随机整数，范围为[<paramref Name="min"/>, <paramref Name="max"/>]，即可能取值将包含 <paramref Name="min"/> 和 <paramref Name="max"/>。
        /// <para>
        /// 使用此重载方法应注意线程安全问题。避免同时多次执行传入同一个<paramref Name="resultList"/>的该函数。
        /// </para>
        /// 当<paramref Name="max"/> - <paramref Name="min"/> + 1 小于 <paramref Name="count"/>时，将抛出<seealso cref="ArgumentException"/>异常。
        /// 另外<paramref Name="resultList"/>列表将此重载方法执行时被清空。
        /// <para>
        /// 将应用传入的熵源。仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。
        /// </para>
        /// </summary>
        /// <param Name="min">最小取值</param>
        /// <param Name="max">最大取值</param>
        /// <param Name="count">取值数</param>
        /// <param Name="resultList">用于存储结果的可监视集合, 注意每次函数执行时都会自动清空集合里的内容。</param>
        /// <param Name="randomEntropySources">熵源,仅传入 <seealso cref="RandomEntropySource.SystemClock"/> 以使用默认随机数种子。</param>
        /// <returns>任务</returns>
        /// <Exception cref="ArgumentException">当<paramref Name="max"/> - <paramref Name="min"/> + 1 小于 <paramref Name="count"/>时</Exception>"
        public static async Task DrawUniqueRandomIntAsync(int min, int max, int count, ICollection<int> resultList, params RandomEntropySource[] randomEntropySources)
        {
            if (max - min + 1 < count)
            {
                throw new ArgumentException("范围内的数字数量不足以生成所需数量的不重复随机数。");
            }

            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            HashSet<int> uniqueNumbers;

            if (count > 1000)
            {
                // 待生成的随机数数量过多，启用分批处理，以避免在拷贝到 ObservableCollection 时导致 UI 线程卡顿
                await Task.Run(async () =>
                {
                    uniqueNumbers = await DrawUniqueRandomIntAsync(min, max, count, randomEntropySources);

                    int batchSize = 100; // 每次处理的批次大小
                    int totalBatches = (int)Math.Ceiling((double)uniqueNumbers.Count / batchSize);

                    dispatcherQueue.TryEnqueue(() => resultList.Clear());

                    var tempList = uniqueNumbers.ToList();
                    for (int batch = 0; batch < totalBatches; batch++)
                    {
                        var currentBatch = tempList.Skip(batch * batchSize).Take(batchSize).ToList();
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            foreach (var number in currentBatch)
                            {
                                resultList.Add(number);
                            }
                        });

                        // 等待一小段时间以便UI线程有机会处理其他任务
                        await Task.Delay(10);
                    }
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    uniqueNumbers = await DrawUniqueRandomIntAsync(min, max, count, randomEntropySources);

                    dispatcherQueue.TryEnqueue(() =>
                    {
                        resultList.Clear();
                        foreach (var number in uniqueNumbers)
                        {
                            resultList.Add(number);
                        }
                    });
                });
            }
        }

        #endregion

        /// <summary>
        /// 从 Random.org 获取真随机数。
        /// </summary>
        /// <param Name="min"></param>
        /// <param Name="max"></param>
        /// <returns></returns>
        /// <Exception cref="FormatException"></Exception>
        /// <Exception cref="HttpRequestException"></Exception>
        private static async Task<int> GetTrueRandomNumberFromRandomOrg(int min, int max)
        {
            // 从 Random.org 获取真随机数
            // 由于 Random.org 有每天请求次数限制，因此不建议频繁调用此函数
            // 请注意 Random.org 的使用条款
            // https://www.random.org/terms/

            string url = $"https://www.random.org/integers/?num=1&min={min}&max={max}&col=1&base=10&format=plain&rnd=new&cl=w";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发送HTTP GET请求
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(responseBody);

                    // 使用正则表达式提取数字
                    string pattern = @"<span style='font-size:100%;font-weight:bold;'>(\d+)";
                    Match match = Regex.Match(responseBody, pattern);

                    if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                    {
                        return number;
                    }
                    else
                    {
                        throw new FormatException("无法解析 Random.org 的响应内容。");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception while getting random number at random.org: " + ex.ToString());
                    throw;
                }
            }
        }

    }

    /// <summary>
    /// 随机数熵源
    /// </summary>
    public enum RandomEntropySource
    {
        SystemClock,
        RuntimeNoise,
        MousePoint,
        RandomOrg
    }
}
