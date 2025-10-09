using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcTree.Scripts.Test
{
    /// <summary>
    /// 八叉树可视化演示 - 展示分割过程和查询效果
    /// </summary>
    public class OctreeVisualizationDemo : MonoBehaviour
    {
        [Header("可视化设置")]
        [SerializeField] private bool enableVisualization = true;
        [SerializeField] private bool showNodeBounds = true;
        [SerializeField] private bool showObjects = true;
        [SerializeField] private bool showQueryRange = true;
        [SerializeField] private Color nodeColor = Color.cyan;
        [SerializeField] private Color objectColor = Color.red;
        [SerializeField] private Color queryColor = Color.yellow;
        [SerializeField] private Color resultColor = Color.green;
    
        [Header("演示参数")]
        [SerializeField] private float worldSize = 32f;
        [SerializeField] private float minNodeSize = 2f;
        [SerializeField] private Vector3 queryCenter = Vector3.zero;
        [SerializeField] private float queryRadius = 8f;
    
        [Header("动态演示")]
        [SerializeField] private bool autoAddObjects = false;
        [SerializeField] private float addInterval = 1f;
        [SerializeField] private int maxObjects = 50;
        [SerializeField] private bool moveQueryPoint = false;
        [SerializeField] private float moveSpeed = 2f;
    
        private PointOctree<DemoObject> octree;
        private List<DemoObject> allObjects = new List<DemoObject>();
        private List<DemoObject> queryResults = new List<DemoObject>();
        private float lastAddTime;
        private int objectIdCounter = 0;
    
        /// <summary>
        /// 演示用对象类
        /// </summary>
        public class DemoObject
        {
            public Vector3 position;
            public Color color;
            public int id;
            public float size;
            public string name;
        
            public DemoObject(Vector3 pos, int id)
            {
                this.position = pos;
                this.id = id;
                this.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
                this.size = Random.Range(0.5f, 1.5f);
                this.name = $"Obj_{id}";
            }
        
            public override bool Equals(object obj)
            {
                if (obj is DemoObject other)
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
            InitializeDemo();
        }
    
        void Update()
        {
            // 自动添加对象
            if (autoAddObjects && Time.time - lastAddTime > addInterval && allObjects.Count < maxObjects)
            {
                AddRandomObject();
                lastAddTime = Time.time;
            }
        
            // 移动查询点
            if (moveQueryPoint)
            {
                float t = Time.time * moveSpeed;
                queryCenter = new Vector3(
                    Mathf.Sin(t) * worldSize * 0.3f,
                    Mathf.Cos(t * 0.7f) * worldSize * 0.3f,
                    Mathf.Sin(t * 0.5f) * worldSize * 0.3f
                );
            }
        
            // 更新查询结果
            UpdateQueryResults();
        }
    
        /// <summary>
        /// 初始化演示
        /// </summary>
        void InitializeDemo()
        {
            octree = new PointOctree<DemoObject>(worldSize, Vector3.zero, minNodeSize);
            allObjects.Clear();
            queryResults.Clear();
            objectIdCounter = 0;
        
            // 添加一些初始对象
            AddInitialObjects();
        
            Debug.Log($"八叉树演示初始化完成 - 世界大小: {worldSize}");
        }
    
        /// <summary>
        /// 添加初始对象
        /// </summary>
        void AddInitialObjects()
        {
            // 在中心区域添加一些对象
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                );
            
                AddObjectAt(pos);
            }
        
            // 在边缘添加一些对象
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-worldSize*0.4f, worldSize*0.4f),
                    Random.Range(-worldSize*0.4f, worldSize*0.4f),
                    Random.Range(-worldSize*0.4f, worldSize*0.4f)
                );
            
                AddObjectAt(pos);
            }
        }
    
        /// <summary>
        /// 在指定位置添加对象
        /// </summary>
        void AddObjectAt(Vector3 position)
        {
            DemoObject obj = new DemoObject(position, objectIdCounter++);
            allObjects.Add(obj);
            octree.Add(obj, position);
        
            Debug.Log($"添加对象 {obj.name} 到位置 {position}, 总数: {octree.Count}");
        }
    
        /// <summary>
        /// 添加随机对象
        /// </summary>
        void AddRandomObject()
        {
            Vector3 pos = new Vector3(
                Random.Range(-worldSize*0.4f, worldSize*0.4f),
                Random.Range(-worldSize*0.4f, worldSize*0.4f),
                Random.Range(-worldSize*0.4f, worldSize*0.4f)
            );
        
            AddObjectAt(pos);
        }
    
        /// <summary>
        /// 更新查询结果
        /// </summary>
        void UpdateQueryResults()
        {
            if (octree != null)
            {
                queryResults.Clear();
                octree.GetNearbyNonAlloc(queryCenter, queryRadius, queryResults);
            }
        }
    
        /// <summary>
        /// 演示分割过程
        /// </summary>
        [ContextMenu("演示分割过程")]
        public void DemoSplitting()
        {
            StartCoroutine(SplittingDemo());
        }
    
        IEnumerator SplittingDemo()
        {
            Debug.Log("开始演示八叉树分割过程...");
        
            // 重置
            InitializeDemo();
            yield return new WaitForSeconds(1f);
        
            // 在同一区域连续添加对象以触发分割
            Vector3 centerArea = Vector3.zero;
        
            for (int i = 0; i < 12; i++)
            {
                Vector3 pos = centerArea + new Vector3(
                    Random.Range(-3f, 3f),
                    Random.Range(-3f, 3f),
                    Random.Range(-3f, 3f)
                );
            
                AddObjectAt(pos);
            
                Debug.Log($"添加第 {i+1} 个对象 {(i >= 8 ? "(应触发分割)" : "")}");
                yield return new WaitForSeconds(0.5f);
            }
        
            Debug.Log("分割演示完成");
        }
    
        /// <summary>
        /// 演示查询过程
        /// </summary>
        [ContextMenu("演示查询过程")]
        public void DemoQuerying()
        {
            StartCoroutine(QueryingDemo());
        }
    
        IEnumerator QueryingDemo()
        {
            Debug.Log("开始演示八叉树查询过程...");
        
            Vector3[] queryPositions = {
                Vector3.zero,
                new Vector3(10, 0, 0),
                new Vector3(-8, 5, -3),
                new Vector3(0, -10, 8),
                new Vector3(15, 15, 15)
            };
        
            foreach (Vector3 pos in queryPositions)
            {
                queryCenter = pos;
                UpdateQueryResults();
            
                Debug.Log($"在位置 {pos} 查询，半径 {queryRadius}，找到 {queryResults.Count} 个对象");
            
                foreach (var result in queryResults)
                {
                    float distance = Vector3.Distance(queryCenter, result.position);
                    Debug.Log($"  - {result.name} 距离: {distance:F2}");
                }
            
                yield return new WaitForSeconds(2f);
            }
        
            Debug.Log("查询演示完成");
        }
    
        /// <summary>
        /// 压力测试演示
        /// </summary>
        [ContextMenu("压力测试演示")]
        public void StressTestDemo()
        {
            StartCoroutine(StressTest());
        }
    
        IEnumerator StressTest()
        {
            Debug.Log("开始压力测试演示...");
        
            InitializeDemo();
        
            // 快速添加大量对象
            for (int batch = 0; batch < 10; batch++)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (allObjects.Count >= maxObjects) break;
                    AddRandomObject();
                }
            
                Debug.Log($"批次 {batch + 1}: 已添加 {octree.Count} 个对象");
                yield return new WaitForSeconds(0.2f);
            }
        
            // 执行多次查询测试
            for (int i = 0; i < 20; i++)
            {
                queryCenter = new Vector3(
                    Random.Range(-worldSize*0.3f, worldSize*0.3f),
                    Random.Range(-worldSize*0.3f, worldSize*0.3f),
                    Random.Range(-worldSize*0.3f, worldSize*0.3f)
                );
            
                UpdateQueryResults();
            
                if (i % 5 == 0)
                {
                    Debug.Log($"查询 {i + 1}: 位置 {queryCenter}, 找到 {queryResults.Count} 个对象");
                }
            
                yield return new WaitForSeconds(0.1f);
            }
        
            Debug.Log("压力测试完成");
        }
    
        void OnDrawGizmos()
        {
            if (!enableVisualization || octree == null)
                return;
        
            // 绘制八叉树节点边界
            if (showNodeBounds)
            {
                Gizmos.color = nodeColor;
                octree.DrawAllBounds();
            }
        
            // 绘制对象
            if (showObjects)
            {
                foreach (var obj in allObjects)
                {
                    // 根据是否在查询结果中决定颜色
                    bool inResult = queryResults.Contains(obj);
                    Gizmos.color = inResult ? resultColor : objectColor;
                
                    Gizmos.DrawSphere(obj.position, obj.size * 0.5f);
                
                    // 绘制ID标签
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(obj.position + Vector3.up * obj.size, obj.id.ToString());
#endif
                }
            }
        
            // 绘制查询范围
            if (showQueryRange)
            {
                Gizmos.color = queryColor;
                Gizmos.DrawWireSphere(queryCenter, queryRadius);
            
                // 绘制查询中心点
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(queryCenter, 0.5f);
            }
        
            Gizmos.color = Color.white;
        }
    
        void OnGUI()
        {
            if (!enableVisualization)
                return;
        
            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.BeginVertical("box");
        
            GUILayout.Label("八叉树可视化演示", GUI.skin.button);
        
            // 统计信息
            if (octree != null)
            {
                GUILayout.Label($"对象总数: {octree.Count}");
                GUILayout.Label($"查询结果: {queryResults.Count}");
                GUILayout.Label($"查询位置: ({queryCenter.x:F1}, {queryCenter.y:F1}, {queryCenter.z:F1})");
            }
        
            GUILayout.Space(10);
        
            // 手动控制
            GUILayout.Label("手动控制:");
            if (GUILayout.Button("添加随机对象"))
                AddRandomObject();
        
            if (GUILayout.Button("重置演示"))
                InitializeDemo();
        
            if (GUILayout.Button("清空所有对象"))
            {
                InitializeDemo();
                allObjects.Clear();
            }
        
            GUILayout.Space(10);
        
            // 自动演示
            GUILayout.Label("自动演示:");
            if (GUILayout.Button("分割过程演示"))
                DemoSplitting();
        
            if (GUILayout.Button("查询过程演示"))
                DemoQuerying();
        
            if (GUILayout.Button("压力测试"))
                StressTestDemo();
        
            GUILayout.Space(10);
        
            // 设置
            GUILayout.Label("动态设置:");
            autoAddObjects = GUILayout.Toggle(autoAddObjects, "自动添加对象");
            moveQueryPoint = GUILayout.Toggle(moveQueryPoint, "移动查询点");
        
            GUILayout.Label($"查询半径: {queryRadius:F1}");
            queryRadius = GUILayout.HorizontalSlider(queryRadius, 1f, 20f);
        
            GUILayout.Label($"添加间隔: {addInterval:F1}s");
            addInterval = GUILayout.HorizontalSlider(addInterval, 0.1f, 3f);
        
            GUILayout.Space(10);
        
            // 可视化选项
            GUILayout.Label("可视化选项:");
            showNodeBounds = GUILayout.Toggle(showNodeBounds, "显示节点边界");
            showObjects = GUILayout.Toggle(showObjects, "显示对象");
            showQueryRange = GUILayout.Toggle(showQueryRange, "显示查询范围");
        
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}