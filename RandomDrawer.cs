using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomly_NT
{
    static class RandomDrawer
    {
        /// <summary>
        /// 生成若干个随机整数，范围为[<paramref name="min"/>, <paramref name="max"/>]，取值将包含 <paramref name="min"/> 和 <paramref name="max"/>。
        /// </summary>
        /// <param name="min">最小取值</param>
        /// <param name="max">最大取值</param>
        /// <param name="count">取值数</param>
        /// <returns>随机整数列表</returns>
        public static List<int> DrawRandomInt(int min, int max, int count)
        {
            List<int> result = new List<int>();
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                result.Add(random.Next(min, max + 1));
            }
            return result;
        }

        /// <summary>
        /// 生成若干个随机整数，范围为[<paramref name="min"/>, <paramref name="max"/>]，即可能取值将包含 <paramref name="min"/> 和 <paramref name="max"/>。
        /// </summary>
        /// <param name="min">最小取值</param>
        /// <param name="max">最大取值</param>
        /// <param name="count">取值数</param>
        /// <returns>随机整数列表</returns>
        public static async Task<List<int>> DrawRandomIntAsync(int min, int max, int count)
        {
            return await Task.Run(() =>
            {
                List<int> result = new List<int>();
                Random random = new Random();
                for (int i = 0; i < count; i++)
                {
                    result.Add(random.Next(min, max + 1));
                }
                return result;
            });
        }

        /// <summary>
        /// 生成若干个随机整数，范围为[<paramref name="min"/>, <paramref name="max"/>]，即可能取值将包含 <paramref name="min"/> 和 <paramref name="max"/>。
        /// <para>
        /// 使用此重载方法应注意线程安全问题。避免同时多次执行传入同一个<paramref name="resultList"/>的该函数。
        /// </para>
        /// 另外<paramref name="resultList"/>列表将此重载方法执行时被清空。
        /// </summary>
        /// <param name="min">最小取值</param>
        /// <param name="max">最大取值</param>
        /// <param name="count">取值数</param>
        /// <param name="resultList">用于存储结果的可监视集合, 注意每次函数执行时都会自动清空集合里的内容。</param>
        /// <returns>任务</returns>
        public static async Task DrawRandomIntAsync(int min, int max, int count, ObservableCollection<int> resultList)
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            Random random = new Random();
            if (count > 1000)
            {
                // 待生成的随机数数量过多，启用分批处理，以避免在拷贝到 ObservableCollection 时导致 UI 线程卡顿
                await Task.Run(async () =>
                {
                    List<int> tempList = new List<int>();
                    for (int i = 0; i < count; i++)
                    {
                        tempList.Add(random.Next(min, max + 1));
                    }
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
                await Task.Run(() =>
                {
                    List<int> tempList = new List<int>();
                    for (int i = 0; i < count; i++)
                    {
                        tempList.Add(random.Next(min, max + 1));
                    }
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


        /// <summary>
        /// 生成若干个不重复的随机整数，范围为[<paramref name="min"/>, <paramref name="max"/>]，即可能取值将包含 <paramref name="min"/> 和 <paramref name="max"/>。
        /// <para>
        /// 使用此重载方法应注意线程安全问题。避免同时多次执行传入同一个<paramref name="resultList"/>的该函数。
        /// </para>
        /// 当<paramref name="max"/> - <paramref name="min"/> + 1 小于 <paramref name="count"/>时，将抛出<seealso cref="ArgumentException"/>异常。
        /// 另外<paramref name="resultList"/>列表将此重载方法执行时被清空。
        /// </summary>
        /// <param name="min">最小取值</param>
        /// <param name="max">最大取值</param>
        /// <param name="count">取值数</param>
        /// <param name="resultList">用于存储结果的可监视集合, 注意每次函数执行时都会自动清空集合里的内容。</param>
        /// <returns>任务</returns>
        /// <exception cref="ArgumentException">当<paramref name="max"/> - <paramref name="min"/> + 1 小于 <paramref name="count"/>时</exception>"
        public static async Task DrawUniqueRandomIntAsync(int min, int max, int count, ObservableCollection<int> resultList)
        {
            if (max - min + 1 < count)
            {
                throw new ArgumentException("范围内的数字数量不足以生成所需数量的不重复随机数。");
            }

            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            Random random = new Random();
            HashSet<int> uniqueNumbers = new HashSet<int>();

            if (count > 1000)
            {
                // 待生成的随机数数量过多，启用分批处理，以避免在拷贝到 ObservableCollection 时导致 UI 线程卡顿
                await Task.Run(async () =>
                {
                    while (uniqueNumbers.Count < count)
                    {
                        uniqueNumbers.Add(random.Next(min, max + 1));
                    }

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
                await Task.Run(() =>
                {
                    while (uniqueNumbers.Count < count)
                    {
                        uniqueNumbers.Add(random.Next(min, max + 1));
                    }

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

    }
}
