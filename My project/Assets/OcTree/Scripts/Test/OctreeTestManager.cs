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
        
        [Header("射线查询测试")]
        [SerializeField] private Vector3 rayOrigin = Vector3.zero;
        [SerializeField] private Vector3 rayDirection = Vector3.right;
        [SerializeField] private float rayMaxDistance = 15f;
        [SerializeField] private bool showRayQuery = false;
    
        private PointOctree<TestObject> octree;
        private List<TestObject> testObjects = new List<TestObject>();
        private List<TestObject> queryResults = new List<TestObject>();
        private List<TestObject> rayQueryResults = new List<TestObject>();
    
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
        
            // 测试基于位置的范围查询
            Vector3 queryCenter = Vector3.zero;
            float queryDist = 10f;
        
            // 使用非分配版本
            List<TestObject> results = new List<TestObject>();
            bool found = queryOctree.GetNearbyNonAlloc(queryCenter, queryDist, results);
        
            Debug.Log($"位置查询 - 在位置 {queryCenter} 半径 {queryDist} 内找到 {results.Count} 个对象:");
            foreach (var result in results)
            {
                float distance = Vector3.Distance(queryCenter, result.position);
                Debug.Log($"  - {result.name} 距离: {distance:F2}");
            }
        
            // 使用分配版本对比
            var arrayResults = queryOctree.GetNearby(queryCenter, queryDist);
            Debug.Log($"位置查询 - 数组版本返回 {arrayResults.Length} 个对象");
            
            // 测试基于射线的范围查询
            TestRayQueries(queryOctree, queryDist);
        }
        
        /// <summary>
        /// 测试基于射线的查询
        /// </summary>
        void TestRayQueries(PointOctree<TestObject> queryOctree, float maxDistance)
        {
            Debug.Log("--- 测试射线查询 ---");
            
            // 测试不同方向的射线
            Ray[] testRays = {
                new Ray(Vector3.zero, Vector3.right),           // 沿X轴正方向
                new Ray(Vector3.zero, Vector3.up),              // 沿Y轴正方向
                new Ray(Vector3.zero, Vector3.forward),         // 沿Z轴正方向
                new Ray(Vector3.zero, Vector3.one.normalized),  // 对角线方向
                new Ray(new Vector3(-10, 0, 0), Vector3.right), // 从外部射入
                new Ray(new Vector3(0, 10, 0), Vector3.down)    // 从上往下
            };
            
            string[] rayNames = {
                "沿X轴正方向",
                "沿Y轴正方向", 
                "沿Z轴正方向",
                "对角线方向",
                "从外部射入",
                "从上往下"
            };
            
            for (int i = 0; i < testRays.Length; i++)
            {
                Ray ray = testRays[i];
                
                // 使用非分配版本测试
                List<TestObject> rayResults = new List<TestObject>();
                bool foundRay = queryOctree.GetNearbyNonAlloc(ray, maxDistance, rayResults);
                
                Debug.Log($"射线查询 ({rayNames[i]}) - 起点: {ray.origin}, 方向: {ray.direction}");
                Debug.Log($"  在距离 {maxDistance} 内找到 {rayResults.Count} 个对象:");
                
                foreach (var result in rayResults)
                {
                    float distanceToRay = Mathf.Sqrt(PointOctreeNode<TestObject>.SqrDistanceToRay(ray, result.position));
                    Vector3 closestPoint = ray.origin + Vector3.Project(result.position - ray.origin, ray.direction);
                    float distanceAlongRay = Vector3.Distance(ray.origin, closestPoint);
                    
                    Debug.Log($"    - {result.name} 到射线距离: {distanceToRay:F2}, 沿射线距离: {distanceAlongRay:F2}");
                }
                
                // 使用分配版本对比
                var rayArrayResults = queryOctree.GetNearby(ray, maxDistance);
                Debug.Log($"  数组版本返回 {rayArrayResults.Length} 个对象");
                
                // 验证两种方法返回相同结果
                if (rayResults.Count != rayArrayResults.Length)
                {
                    Debug.LogWarning($"射线查询结果不一致! NonAlloc: {rayResults.Count}, Array: {rayArrayResults.Length}");
                }
            }
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
        
            // 位置查询性能测试
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
            Debug.Log($"{queryCount} 次位置查询耗时: {queryTime}ms (平均 {queryTime/queryCount:F3}ms/查询)");
            Debug.Log($"平均每次位置查询找到 {totalFound/(float)queryCount:F1} 个对象");
            
            // 射线查询性能测试
            sw.Restart();
            int rayQueryCount = 20;
            int totalRayFound = 0;
            
            for (int i = 0; i < rayQueryCount; i++)
            {
                Vector3 rayOriginPos = new Vector3(
                    Random.Range(-40f, 40f),
                    Random.Range(-40f, 40f),
                    Random.Range(-40f, 40f)
                );
                
                Vector3 rayDir = Random.onUnitSphere;
                Ray testRay = new Ray(rayOriginPos, rayDir);
                
                var rayResults = perfOctree.GetNearby(testRay, rayMaxDistance);
                totalRayFound += rayResults.Length;
            }
            
            sw.Stop();
            float rayQueryTime = sw.ElapsedMilliseconds;
            Debug.Log($"{rayQueryCount} 次射线查询耗时: {rayQueryTime}ms (平均 {rayQueryTime/rayQueryCount:F3}ms/查询)");
            Debug.Log($"平均每次射线查询找到 {totalRayFound/(float)rayQueryCount:F1} 个对象");
        
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
        /// 手动触发射线查询测试
        /// </summary>
        [ContextMenu("执行射线查询")]
        public void PerformRayQuery()
        {
            if (octree == null)
            {
                Debug.LogWarning("八叉树未初始化");
                return;
            }
            
            Ray testRay = new Ray(rayOrigin, rayDirection.normalized);
            rayQueryResults.Clear();
            bool found = octree.GetNearbyNonAlloc(testRay, rayMaxDistance, rayQueryResults);
            
            Debug.Log($"射线查询 - 起点: {testRay.origin}, 方向: {testRay.direction}, 最大距离: {rayMaxDistance}");
            Debug.Log($"找到 {rayQueryResults.Count} 个对象:");
            
            foreach (var result in rayQueryResults)
            {
                float distanceToRay = Mathf.Sqrt(PointOctreeNode<TestObject>.SqrDistanceToRay(testRay, result.position));
                Vector3 closestPoint = testRay.origin + Vector3.Project(result.position - testRay.origin, testRay.direction);
                float distanceAlongRay = Vector3.Distance(testRay.origin, closestPoint);
                
                Debug.Log($"  - {result.name} 到射线距离: {distanceToRay:F2}, 沿射线距离: {distanceAlongRay:F2}");
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
            rayQueryResults.Clear();
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
        
            // 绘制位置查询范围
            if (showQueryResults)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(queryPosition, queryRadius);
            
                // 高亮位置查询结果
                Gizmos.color = Color.red;
                foreach (var result in queryResults)
                {
                    Gizmos.DrawSphere(result.position, 0.5f);
                }
            }
            
            // 绘制射线查询
            if (showRayQuery)
            {
                Ray visualRay = new Ray(rayOrigin, rayDirection.normalized);
                
                // 绘制射线
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(visualRay.origin, visualRay.direction * rayMaxDistance);
                
                // 绘制射线起点
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(visualRay.origin, 0.8f);
                
                // 高亮射线查询结果
                Gizmos.color = Color.magenta;
                foreach (var result in rayQueryResults)
                {
                    Gizmos.DrawSphere(result.position, 0.6f);
                    
                    // 绘制从对象到射线的最短距离线
                    Vector3 closestPoint = visualRay.origin + Vector3.Project(result.position - visualRay.origin, visualRay.direction);
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(result.position, closestPoint);
                    Gizmos.color = Color.magenta;
                }
            }
        
            Gizmos.color = Color.white;
        }
    
        void OnGUI()
        {
            if (!enableVisualization)
                return;
        
            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.BeginVertical("box");
        
            GUILayout.Label("八叉树测试控制面板", EditorGUIStyles.BoldLabel);
        
            if (octree != null)
            {
                GUILayout.Label($"对象总数: {octree.Count}");
                GUILayout.Label($"位置查询结果: {queryResults.Count}");
                GUILayout.Label($"射线查询结果: {rayQueryResults.Count}");
            }
        
            GUILayout.Space(10);
        
            if (GUILayout.Button("运行所有测试"))
                RunAllTests();
        
            if (GUILayout.Button("添加随机对象"))
                AddRandomObjects();
        
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("执行位置查询"))
                PerformRangeQuery();
            
            if (GUILayout.Button("执行射线查询"))
                PerformRayQuery();
            GUILayout.EndHorizontal();
        
            if (GUILayout.Button("清空八叉树"))
                ClearOctree();
        
            GUILayout.Space(10);
        
            GUILayout.Label("位置查询设置:");
            queryRadius = GUILayout.HorizontalSlider(queryRadius, 1f, 50f);
            GUILayout.Label($"查询半径: {queryRadius:F1}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("射线查询设置:");
            rayMaxDistance = GUILayout.HorizontalSlider(rayMaxDistance, 1f, 50f);
            GUILayout.Label($"射线最大距离: {rayMaxDistance:F1}");
            
            GUILayout.Space(10);
            
            GUILayout.Label("可视化选项:");
            showQueryResults = GUILayout.Toggle(showQueryResults, "显示位置查询");
            showRayQuery = GUILayout.Toggle(showRayQuery, "显示射线查询");
        
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