using System.Collections.Generic;
using UnityEngine;

namespace OcTree.Scripts.Test
{
    /// <summary>
    /// 八叉树与线性搜索性能对比测试
    /// </summary>
    public class OctreePerformanceComparison : MonoBehaviour
    {
        [Header("性能测试配置")]
        [SerializeField] private int[] testSizes = {100, 500, 1000, 2000, 5000};
        [SerializeField] private int queryCount = 100;
        [SerializeField] private float worldSize = 200f;
        [SerializeField] private float queryRadius = 15f;
        [SerializeField] private bool runOnStart = false;
    
        private struct TestData
        {
            public Vector3 position;
            public int id;
        
            public TestData(Vector3 pos, int id)
            {
                this.position = pos;
                this.id = id;
            }
        }
    
        void Start()
        {
            if (runOnStart)
            {
                RunPerformanceComparison();
            }
        }
    
        /// <summary>
        /// 运行性能对比测试
        /// </summary>
        [ContextMenu("运行性能对比")]
        public void RunPerformanceComparison()
        {
            Debug.Log("=== 八叉树 vs 线性搜索性能对比 ===");
        
            foreach (int testSize in testSizes)
            {
                Debug.Log($"\n--- 测试规模: {testSize} 个对象 ---");
                ComparePerformance(testSize);
            }
        
            Debug.Log("\n=== 性能对比测试完成 ===");
        }
    
        /// <summary>
        /// 对比指定数量对象的性能
        /// </summary>
        void ComparePerformance(int objectCount)
        {
            // 生成测试数据
            List<TestData> testData = GenerateTestData(objectCount);
        
            // 八叉树测试
            float octreeTime = TestOctreePerformance(testData);
        
            // 线性搜索测试
            float linearTime = TestLinearSearchPerformance(testData);
        
            // 输出结果
            float speedup = linearTime / octreeTime;
            Debug.Log($"八叉树平均查询时间: {octreeTime:F4}ms");
            Debug.Log($"线性搜索平均查询时间: {linearTime:F4}ms");
            Debug.Log($"八叉树性能提升: {speedup:F2}x");
        
            // 内存使用估算
            float octreeMemory = EstimateOctreeMemory(objectCount);
            float linearMemory = EstimateLinearMemory(objectCount);
            Debug.Log($"八叉树内存使用估算: {octreeMemory:F2}KB");
            Debug.Log($"线性搜索内存使用估算: {linearMemory:F2}KB");
        }
    
        /// <summary>
        /// 生成测试数据
        /// </summary>
        List<TestData> GenerateTestData(int count)
        {
            List<TestData> data = new List<TestData>(count);
            Random.InitState(12345); // 固定随机种子确保可重复性
        
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-worldSize/2, worldSize/2),
                    Random.Range(-worldSize/2, worldSize/2),
                    Random.Range(-worldSize/2, worldSize/2)
                );
            
                data.Add(new TestData(pos, i));
            }
        
            return data;
        }
    
        /// <summary>
        /// 测试八叉树性能
        /// </summary>
        float TestOctreePerformance(List<TestData> testData)
        {
            // 构建八叉树
            var octree = new PointOctree<TestData>(worldSize, Vector3.zero, 1f);
        
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        
            // 添加所有对象
            foreach (var data in testData)
            {
                octree.Add(data, data.position);
            }
        
            sw.Stop();
            float buildTime = sw.ElapsedMilliseconds;
        
            // 执行查询测试
            sw.Restart();
            List<TestData> results = new List<TestData>();
        
            for (int i = 0; i < queryCount; i++)
            {
                Vector3 queryPos = new Vector3(
                    Random.Range(-worldSize/3, worldSize/3),
                    Random.Range(-worldSize/3, worldSize/3),
                    Random.Range(-worldSize/3, worldSize/3)
                );
            
                octree.GetNearbyNonAlloc(queryPos, queryRadius, results);
                results.Clear();
            }
        
            sw.Stop();
            float queryTime = sw.ElapsedMilliseconds;
        
            Debug.Log($"  八叉树构建时间: {buildTime}ms");
            Debug.Log($"  八叉树 {queryCount} 次查询总时间: {queryTime}ms");
        
            return queryTime / (float)queryCount;
        }
    
        /// <summary>
        /// 测试线性搜索性能
        /// </summary>
        float TestLinearSearchPerformance(List<TestData> testData)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        
            for (int i = 0; i < queryCount; i++)
            {
                Vector3 queryPos = new Vector3(
                    Random.Range(-worldSize/3, worldSize/3),
                    Random.Range(-worldSize/3, worldSize/3),
                    Random.Range(-worldSize/3, worldSize/3)
                );
            
                // 线性搜索
                List<TestData> results = new List<TestData>();
                float sqrRadius = queryRadius * queryRadius;
            
                foreach (var data in testData)
                {
                    if ((queryPos - data.position).sqrMagnitude <= sqrRadius)
                    {
                        results.Add(data);
                    }
                }
            }
        
            sw.Stop();
            float totalTime = sw.ElapsedMilliseconds;
        
            Debug.Log($"  线性搜索 {queryCount} 次查询总时间: {totalTime}ms");
        
            return totalTime / (float)queryCount;
        }
    
        /// <summary>
        /// 估算八叉树内存使用
        /// </summary>
        float EstimateOctreeMemory(int objectCount)
        {
            // 粗略估算：
            // - 每个对象包装器约 32 字节
            // - 每个节点约 100 字节
            // - 预估节点数量为对象数量的 1/4（经验值）
        
            float objectMemory = objectCount * 32f;
            float nodeCount = Mathf.Max(1, objectCount / 4f);
            float nodeMemory = nodeCount * 100f;
        
            return (objectMemory + nodeMemory) / 1024f; // 转换为KB
        }
    
        /// <summary>
        /// 估算线性搜索内存使用
        /// </summary>
        float EstimateLinearMemory(int objectCount)
        {
            // 只需要存储对象列表
            float objectMemory = objectCount * 32f;
            return objectMemory / 1024f; // 转换为KB
        }
    
        /// <summary>
        /// 复杂场景测试：聚集分布
        /// </summary>
        [ContextMenu("测试聚集分布性能")]
        public void TestClusteredDistribution()
        {
            Debug.Log("=== 聚集分布性能测试 ===");
        
            int objectCount = 2000;
            int clusterCount = 5;
            float clusterRadius = 20f;
        
            // 生成聚集分布的数据
            List<TestData> clusteredData = new List<TestData>();
            Random.InitState(54321);
        
            for (int cluster = 0; cluster < clusterCount; cluster++)
            {
                Vector3 clusterCenter = new Vector3(
                    Random.Range(-worldSize/3, worldSize/3),
                    Random.Range(-worldSize/3, worldSize/3),
                    Random.Range(-worldSize/3, worldSize/3)
                );
            
                int objectsInCluster = objectCount / clusterCount;
                for (int i = 0; i < objectsInCluster; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * clusterRadius;
                    Vector3 pos = clusterCenter + offset;
                
                    clusteredData.Add(new TestData(pos, clusteredData.Count));
                }
            }
        
            Debug.Log($"生成了 {clusteredData.Count} 个聚集分布的对象");
        
            // 测试性能
            float octreeTime = TestOctreePerformance(clusteredData);
            float linearTime = TestLinearSearchPerformance(clusteredData);
        
            float speedup = linearTime / octreeTime;
            Debug.Log($"聚集分布场景下八叉树性能提升: {speedup:F2}x");
        }
    
        /// <summary>
        /// 实时性能监控
        /// </summary>
        [ContextMenu("开始实时性能监控")]
        public void StartRealtimeMonitoring()
        {
            StartCoroutine(RealtimePerformanceMonitor());
        }
    
        System.Collections.IEnumerator RealtimePerformanceMonitor()
        {
            var octree = new PointOctree<TestData>(worldSize, Vector3.zero, 1f);
            List<TestData> dynamicObjects = new List<TestData>();
        
            Debug.Log("开始实时性能监控...");
        
            for (int frame = 0; frame < 100; frame++)
            {
                // 动态添加对象
                if (frame % 10 == 0)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector3 pos = Random.insideUnitSphere * worldSize * 0.4f;
                        TestData newObj = new TestData(pos, dynamicObjects.Count);
                        dynamicObjects.Add(newObj);
                        octree.Add(newObj, pos);
                    }
                }
            
                // 执行查询
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            
                Vector3 queryPos = Random.insideUnitSphere * worldSize * 0.3f;
                var results = octree.GetNearby(queryPos, queryRadius);
            
                sw.Stop();
            
                if (frame % 20 == 0)
                {
                    Debug.Log($"帧 {frame}: 对象数 {octree.Count}, 查询时间 {sw.ElapsedTicks * 0.0001f:F4}ms, 找到 {results.Length} 个对象");
                }
            
                yield return null;
            }
        
            Debug.Log("实时性能监控完成");
        }
    
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 400, 100));
            GUILayout.BeginVertical("box");
        
            GUILayout.Label("八叉树性能测试", GUI.skin.button);
        
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("标准性能对比"))
                RunPerformanceComparison();
        
            if (GUILayout.Button("聚集分布测试"))
                TestClusteredDistribution();
            GUILayout.EndHorizontal();
        
            if (GUILayout.Button("实时性能监控"))
                StartRealtimeMonitoring();
        
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}