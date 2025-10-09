using System.Collections.Generic;
using UnityEngine;

namespace OcTree.Scripts.Test
{
    /// <summary>
    /// 八叉树功能测试管理器 - 测试所有八叉树核心功能
    /// </summary>
    public class OctreeTestManager : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private float worldSize = 100f;
        [SerializeField] private float minNodeSize = 1f;
        [SerializeField] private int testObjectCount = 50;
        [SerializeField] private bool enableVisualization = true;
        [SerializeField] private bool runTestsOnStart = true;
    
        [Header("查询测试")]
        [SerializeField] private Vector3 queryPosition = Vector3.zero;
        [SerializeField] private float queryRadius = 10f;
        [SerializeField] private bool showQueryResults = true;
    
        private PointOctree<TestObject> octree;
        private List<TestObject> testObjects = new List<TestObject>();
        private List<TestObject> queryResults = new List<TestObject>();
    
        /// <summary>
        /// 测试用对象类
        /// </summary>
        public class TestObject
        {
            public string name;
            public Vector3 position;
            public Color color;
            public int id;
        
            public TestObject(string name, Vector3 position, int id)
            {
                this.name = name;
                this.position = position;
                this.id = id;
                this.color = Random.ColorHSV();
            }
        
            public override string ToString()
            {
                return $"{name}(ID:{id}) at {position}";
            }
        
            public override bool Equals(object obj)
            {
                if (obj is TestObject other)
                    return id == other.id;
                return false;
            }
        
            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }
    
        void Start()
        {
            InitializeOctree();
        
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }
    
        /// <summary>
        /// 初始化八叉树
        /// </summary>
        void InitializeOctree()
        {
            octree = new PointOctree<TestObject>(worldSize, Vector3.zero, minNodeSize);
            Debug.Log($"八叉树初始化完成 - 世界大小: {worldSize}, 最小节点: {minNodeSize}");
        }
    
        /// <summary>
        /// 运行所有测试
        /// </summary>
        [ContextMenu("运行所有测试")]
        public void RunAllTests()
        {
            Debug.Log("=== 开始八叉树功能测试 ===");
        
            TestBasicOperations();
            TestSplittingMechanism();
            TestRangeQueries();
            TestAutoGrowth();
            TestRemovalAndMerging();
            TestPerformance();
        
            Debug.Log("=== 八叉树功能测试完成 ===");
        }
    
        /// <summary>
        /// 测试基本操作（添加、删除、计数）
        /// </summary>
        void TestBasicOperations()
        {
            Debug.Log("--- 测试基本操作 ---");
        
            // 清空并重新初始化
            octree = new PointOctree<TestObject>(worldSize, Vector3.zero, minNodeSize);
            testObjects.Clear();
        
            // 测试添加操作
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-worldSize/4, worldSize/4),
                    Random.Range(-worldSize/4, worldSize/4),
                    Random.Range(-worldSize/4, worldSize/4)
                );
            
                TestObject obj = new TestObject($"TestObj_{i}", pos, i);
                testObjects.Add(obj);
                octree.Add(obj, pos);
            }
        
            Debug.Log($"添加了 {testObjects.Count} 个对象，八叉树计数: {octree.Count}");
        
            // 测试删除操作
            if (testObjects.Count > 0)
            {
                TestObject toRemove = testObjects[0];
                bool removed = octree.Remove(toRemove);
                testObjects.RemoveAt(0);
            
                Debug.Log($"删除对象 {toRemove.name}: {(removed ? "成功" : "失败")}");
                Debug.Log($"删除后八叉树计数: {octree.Count}");
            }
        
            // 验证GetAll功能
            var allObjects = octree.GetAll();
            Debug.Log($"GetAll返回 {allObjects.Count} 个对象");
        }
    
        /// <summary>
        /// 测试分割机制
        /// </summary>
        void TestSplittingMechanism()
        {
            Debug.Log("--- 测试分割机制 ---");
        
            // 创建小的八叉树来快速触发分割
            var smallOctree = new PointOctree<TestObject>(16f, Vector3.zero, 1f);
        
            // 在同一区域添加多个对象以触发分割
            Vector3 basePos = Vector3.zero;
            for (int i = 0; i < 12; i++) // 超过阈值8个
            {
                Vector3 pos = basePos + new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(-2f, 2f),
                    Random.Range(-2f, 2f)
                );
            
                TestObject obj = new TestObject($"Split_Test_{i}", pos, 100 + i);
                smallOctree.Add(obj, pos);
            }
        
            Debug.Log($"分割测试完成，添加了12个对象，八叉树计数: {smallOctree.Count}");
            Debug.Log("应该已触发节点分割");
        }
    
        /// <summary>
        /// 测试范围查询
        /// </summary>
        void TestRangeQueries()
        {
            Debug.Log("--- 测试范围查询 ---");
        
            // 添加一些测试对象
            var queryOctree = new PointOctree<TestObject>(50f, Vector3.zero, 1f);
        
            Vector3[] positions = {
                new Vector3(0, 0, 0),    // 中心点
                new Vector3(5, 0, 0),    // 在查询范围内
                new Vector3(15, 0, 0),   // 在查询范围外
                new Vector3(0, 8, 0),    // 边界附近
                new Vector3(-3, -3, -3)  // 在查询范围内
            };
        
            for (int i = 0; i < positions.Length; i++)
            {
                TestObject obj = new TestObject($"Query_Test_{i}", positions[i], 200 + i);
                queryOctree.Add(obj, positions[i]);
            }
        
            // 执行范围查询
            Vector3 queryCenter = Vector3.zero;
            float queryDist = 10f;
        
            // 使用非分配版本
            List<TestObject> results = new List<TestObject>();
            bool found = queryOctree.GetNearbyNonAlloc(queryCenter, queryDist, results);
        
            Debug.Log($"在位置 {queryCenter} 半径 {queryDist} 内找到 {results.Count} 个对象:");
            foreach (var result in results)
            {
                float distance = Vector3.Distance(queryCenter, result.position);
                Debug.Log($"  - {result.name} 距离: {distance:F2}");
            }
        
            // 使用分配版本对比
            var arrayResults = queryOctree.GetNearby(queryCenter, queryDist);
            Debug.Log($"数组版本返回 {arrayResults.Length} 个对象");
        }
    
        /// <summary>
        /// 测试自动扩展功能
        /// </summary>
        void TestAutoGrowth()
        {
            Debug.Log("--- 测试自动扩展 ---");
        
            // 创建较小的八叉树
            var growthOctree = new PointOctree<TestObject>(10f, Vector3.zero, 1f);
        
            // 添加超出边界的对象
            Vector3[] outOfBoundsPositions = {
                new Vector3(20, 0, 0),   // 超出X边界
                new Vector3(0, 25, 0),   // 超出Y边界
                new Vector3(0, 0, -30),  // 超出Z边界
                new Vector3(-15, -15, -15) // 多方向超出
            };
        
            for (int i = 0; i < outOfBoundsPositions.Length; i++)
            {
                TestObject obj = new TestObject($"Growth_Test_{i}", outOfBoundsPositions[i], 300 + i);
                growthOctree.Add(obj, outOfBoundsPositions[i]);
                Debug.Log($"添加超界对象 {obj.name} 到位置 {outOfBoundsPositions[i]}");
            }
        
            Debug.Log($"自动扩展测试完成，八叉树应已自动扩展以容纳所有对象");
            Debug.Log($"最终对象计数: {growthOctree.Count}");
        }
    
        /// <summary>
        /// 测试删除和合并机制
        /// </summary>
        void TestRemovalAndMerging()
        {
            Debug.Log("--- 测试删除和合并 ---");
        
            var mergeOctree = new PointOctree<TestObject>(20f, Vector3.zero, 1f);
            List<TestObject> mergeTestObjects = new List<TestObject>();
        
            // 添加足够多的对象以触发分割
            for (int i = 0; i < 15; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                );
            
                TestObject obj = new TestObject($"Merge_Test_{i}", pos, 400 + i);
                mergeTestObjects.Add(obj);
                mergeOctree.Add(obj, pos);
            }
        
            Debug.Log($"添加了 {mergeTestObjects.Count} 个对象以触发分割");
        
            // 删除大部分对象以触发合并
            for (int i = 0; i < 10; i++)
            {
                if (mergeTestObjects.Count > 0)
                {
                    TestObject toRemove = mergeTestObjects[0];
                    bool removed = mergeOctree.Remove(toRemove);
                    mergeTestObjects.RemoveAt(0);
                
                    if (removed)
                    {
                        Debug.Log($"删除对象 {toRemove.name}，剩余: {mergeOctree.Count}");
                    }
                }
            }
        
            Debug.Log($"删除操作完成，应已触发节点合并，最终计数: {mergeOctree.Count}");
        }
    
        /// <summary>
        /// 性能测试
        /// </summary>
        void TestPerformance()
        {
            Debug.Log("--- 性能测试 ---");
        
            var perfOctree = new PointOctree<TestObject>(100f, Vector3.zero, 1f);
            List<TestObject> perfObjects = new List<TestObject>();
        
            // 大量添加操作测试
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        
            for (int i = 0; i < testObjectCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-40f, 40f),
                    Random.Range(-40f, 40f),
                    Random.Range(-40f, 40f)
                );
            
                TestObject obj = new TestObject($"Perf_Test_{i}", pos, 500 + i);
                perfObjects.Add(obj);
                perfOctree.Add(obj, pos);
            }
        
            sw.Stop();
            float addTime = sw.ElapsedMilliseconds;
            Debug.Log($"添加 {testObjectCount} 个对象耗时: {addTime}ms (平均 {addTime/testObjectCount:F3}ms/对象)");
        
            // 查询性能测试
            sw.Restart();
            int queryCount = 20;
            int totalFound = 0;
        
            for (int i = 0; i < queryCount; i++)
            {
                Vector3 queryPos = new Vector3(
                    Random.Range(-30f, 30f),
                    Random.Range(-30f, 30f),
                    Random.Range(-30f, 30f)
                );
            
                var results = perfOctree.GetNearby(queryPos, queryRadius);
                totalFound += results.Length;
            }
        
            sw.Stop();
            float queryTime = sw.ElapsedMilliseconds;
            Debug.Log($"{queryCount} 次范围查询耗时: {queryTime}ms (平均 {queryTime/queryCount:F3}ms/查询)");
            Debug.Log($"平均每次查询找到 {totalFound/(float)queryCount:F1} 个对象");
        
            // 删除性能测试
            sw.Restart();
            int removeCount = Mathf.Min(perfObjects.Count, 20);
        
            for (int i = 0; i < removeCount; i++)
            {
                perfOctree.Remove(perfObjects[i]);
            }
        
            sw.Stop();
            float removeTime = sw.ElapsedMilliseconds;
            Debug.Log($"删除 {removeCount} 个对象耗时: {removeTime}ms (平均 {removeTime/removeCount:F3}ms/对象)");
        }
    
        /// <summary>
        /// 手动触发范围查询测试
        /// </summary>
        [ContextMenu("执行范围查询")]
        public void PerformRangeQuery()
        {
            if (octree == null)
            {
                Debug.LogWarning("八叉树未初始化");
                return;
            }
        
            queryResults.Clear();
            bool found = octree.GetNearbyNonAlloc(queryPosition, queryRadius, queryResults);
        
            Debug.Log($"在位置 {queryPosition} 半径 {queryRadius} 内找到 {queryResults.Count} 个对象:");
            foreach (var result in queryResults)
            {
                float distance = Vector3.Distance(queryPosition, result.position);
                Debug.Log($"  - {result.name} 距离: {distance:F2}");
            }
        }
    
        /// <summary>
        /// 添加随机测试对象
        /// </summary>
        [ContextMenu("添加随机对象")]
        public void AddRandomObjects()
        {
            if (octree == null)
                InitializeOctree();
        
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-worldSize/2, worldSize/2),
                    Random.Range(-worldSize/2, worldSize/2),
                    Random.Range(-worldSize/2, worldSize/2)
                );
            
                TestObject obj = new TestObject($"Random_{Random.Range(1000, 9999)}", pos, testObjects.Count + 1000);
                testObjects.Add(obj);
                octree.Add(obj, pos);
            }
        
            Debug.Log($"添加了10个随机对象，总计: {octree.Count}");
        }
    
        /// <summary>
        /// 清空八叉树
        /// </summary>
        [ContextMenu("清空八叉树")]
        public void ClearOctree()
        {
            InitializeOctree();
            testObjects.Clear();
            queryResults.Clear();
            Debug.Log("八叉树已清空");
        }
    
        void OnDrawGizmos()
        {
            if (!enableVisualization || octree == null)
                return;
        
            // 绘制八叉树边界
            octree.DrawAllBounds();
        
            // 绘制对象位置
            octree.DrawAllObjects();
        
            // 绘制查询范围
            if (showQueryResults)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(queryPosition, queryRadius);
            
                // 高亮查询结果
                Gizmos.color = Color.red;
                foreach (var result in queryResults)
                {
                    Gizmos.DrawSphere(result.position, 0.5f);
                }
            }
        
            Gizmos.color = Color.white;
        }
    
        void OnGUI()
        {
            if (!enableVisualization)
                return;
        
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");
        
            GUILayout.Label("八叉树测试控制面板", EditorGUIStyles.BoldLabel);
        
            if (octree != null)
            {
                GUILayout.Label($"对象总数: {octree.Count}");
                GUILayout.Label($"查询结果: {queryResults.Count}");
            }
        
            GUILayout.Space(10);
        
            if (GUILayout.Button("运行所有测试"))
                RunAllTests();
        
            if (GUILayout.Button("添加随机对象"))
                AddRandomObjects();
        
            if (GUILayout.Button("执行范围查询"))
                PerformRangeQuery();
        
            if (GUILayout.Button("清空八叉树"))
                ClearOctree();
        
            GUILayout.Space(10);
        
            GUILayout.Label("查询设置:");
            queryRadius = GUILayout.HorizontalSlider(queryRadius, 1f, 50f);
            GUILayout.Label($"查询半径: {queryRadius:F1}");
        
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    
        /// <summary>
        /// 简单的EditorGUI样式类
        /// </summary>
        public static class EditorGUIStyles
        {
            public static GUIStyle BoldLabel
            {
                get
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.fontStyle = FontStyle.Bold;
                    return style;
                }
            }
        }
    }
}