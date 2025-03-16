using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Randomly_NT.ClassMode
{

    public class Student
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; }
        public int Level { get; set; }
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Unknown
    }
    public class Question
    {
        public Difficulty Difficulty { get; set; } = Difficulty.Unknown;
        public string Title { get; set; }
        public string? Description { get; set; }
    }
    #region 课堂抽取部分实现
    public class StudentSelector
    {
        // 定义权重矩阵
        public static readonly float[,] _weights =
        {
            //Lv0    Lv1   Lv2
            { 0.8f, 0.2f, 0.1f },   // Easy
            { 0.3f, 0.6f, 0.2f },   // Medium
            { 0.1f, 0.3f, 0.9f },   // Hard
            { 0.1f, 0.1f, 0.1f }    // Unknown
        };
        private readonly List<Student> _allStudents;
        private readonly HashSet<string> _usedIds = new();

        public StudentSelector(IEnumerable<Student> students)
        {
            _allStudents = students.ToList();
        }

        public Student Select(Difficulty difficulty, params RandomEntropySource[] randomEntropySource)
        {
            // 获取候选学生
            var candidates = _allStudents.Where(s => !_usedIds.Contains(s.Id)).ToList();
            // 如果没有候选学生，重置已使用的学生 ID
            if (candidates.Count == 0)
            {
                Debug.WriteLine("所有学生已被抽取，重置已使用的学生 ID");
                _usedIds.Clear();
                candidates = _allStudents.ToList();
            }
            // 计算权重
            var weights = candidates.Select(s => _weights[(int)difficulty, s.Level]).ToArray();
            var selected = WeightedRandomPick(candidates, weights, randomEntropySource);
            _usedIds.Add(selected.Id);
            return selected;
        }

        private static T WeightedRandomPick<T>(IList<T> items, float[] weights, params RandomEntropySource[] randomEntropySource)
        {
            var total = weights.Sum();
            var random = RandomDrawer.InitializeRandomGenerator(randomEntropySource).NextDouble() * total;

            for (int i = 0; i < items.Count; i++)
            {
                if (random < weights[i]) return items[i];
                random -= weights[i];
            }
            return items.Last();
        }
    }
    #endregion

    #region 学生分层部分实现
    public class RawStudent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; }
        public float Score { get; set; }
        public int Level { get; set; }
    }
    // ML.NET 训练需要的数据结构
    public record StudentRecord
    {
        [LoadColumn(0)]
        public float Score;
    }

    public class StudentClusterService
    {
        private readonly MLContext _mlContext = new();

        public List<Student> ClusterStudents(List<RawStudent> students)
        {
            // 数据检查
            if (students == null || !students.Any())
            {
                throw new ArgumentException("学生列表不能为空");
            }

            // 创建 ML.NET 数据视图
            var dataView = _mlContext.Data.LoadFromEnumerable(students.Select(s => new StudentRecord { Score = s.Score }));
            // 创建管道
            var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(StudentRecord.Score))
                .Append(_mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 3));
            // 训练模型
            var model = pipeline.Fit(dataView);
            // 预测
            var predictions = model.Transform(dataView);
            // 获取预测结果
            var clusterResults = _mlContext.Data.CreateEnumerable<ClusterPrediction>(predictions, false).Select(p => p.PredictedClusterId).ToArray();
            // 获取聚类中心点
            var clusterIds = predictions.GetColumn<uint>("PredictedLabel").ToArray();
            var centroids = InferCentroids(students, clusterIds);
            // 建立层级映射
            var levelMap = CreateLevelMapping(centroids);
            Debug.WriteLine($"学生分层结果：{string.Join(", ", clusterResults)}");
            for (int i = 0; i < students.Count; i++)
            {
                students[i].Level = levelMap[clusterResults[i] - 1];
            }
            return students.Select(s => new Student { Name = s.Name, Level = s.Level }).ToList();
        }

        // 获取聚类中心点
        private float[] InferCentroids(List<RawStudent> students, uint[] clusterIds)
        {
            // 按聚类分组计算平均值
            return clusterIds
                .Distinct()
                .OrderBy(id => id)
                .Select(id => students
                    .Where((_, i) => clusterIds[i] == id)
                    .Average(s => s.Score))
                .ToArray();
        }

        // 根据中心点成绩建立层级映射
        private Dictionary<uint, int> CreateLevelMapping(float[] centroids)
        {
            // 对中心点成绩排序并建立映射
            var sorted = centroids
                .Select((value, index) => new { Value = value, Index = index })
                .OrderBy(x => x.Value)
                .ToList();

            return sorted.ToDictionary(
                x => (uint)x.Index,
                x => sorted.IndexOf(x)  // 0=低分簇 → Level0, 1=中分簇 → Level1, 2=高分簇 → Level2
            );
        }

    }
    public static class ModelExtensions
    {
        public static ITransformer GetLastTransformer(this ITransformer model)
        {
            return model switch
            {
                TransformerChain<ITransformer> chain => chain.Last(),
                _ => model
            };
        }
    }
    public class ClusterPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedClusterId { get; set; }

        [ColumnName("Score")]
        public float[] Distances { get; set; }
    }
    #endregion

}
