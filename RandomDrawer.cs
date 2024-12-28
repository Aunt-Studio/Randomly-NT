using System;
using System.Collections.Generic;
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
        /// <param name="resultList">用于存储结果的列表, 注意每次函数执行时都会自动清空列表里的内容。</param>
        /// <returns>任务</returns>
        public static async Task DrawRandomIntAsync(int min, int max, int count, List<int> resultList)
        {
            await Task.Run(() =>
            {
                resultList.Clear();
                Random random = new Random();
                for (int i = 0; i < count; i++)
                {
                    int number = random.Next(min, max + 1);
                    resultList.Add(number);
                }
            });
        }
    }
}
