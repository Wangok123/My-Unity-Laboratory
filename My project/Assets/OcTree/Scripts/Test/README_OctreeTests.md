# 八叉树测试用例说明

这个测试套件为你的八叉树实现提供了全面的测试和演示功能。

## 测试脚本概览

### 1. OctreeTestManager.cs
**主要功能测试管理器**

- **基本操作测试**: 添加、删除、计数功能
- **分割机制测试**: 验证节点在超过阈值时自动分割
- **范围查询测试**: 球形范围查询和射线查询
- **自动扩展测试**: 验证八叉树自动扩展边界功能
- **合并机制测试**: 验证删除对象后的节点合并
- **性能测试**: 大规模数据的添加、查询、删除性能

**使用方法**:
```csharp
// 在场景中添加此脚本到任意GameObject
// 在Inspector中配置测试参数
// 运行游戏或使用右键菜单 "运行所有测试"
```

**配置参数**:
- `worldSize`: 世界大小 (默认: 100)
- `minNodeSize`: 最小节点尺寸 (默认: 1)
- `testObjectCount`: 性能测试对象数量 (默认: 50)
- `queryRadius`: 查询半径 (默认: 10)

### 2. OctreePerformanceComparison.cs
**性能对比测试**

对比八叉树与线性搜索的性能差异:
- **多规模测试**: 100到5000个对象的性能对比
- **内存使用估算**: 比较两种方法的内存开销
- **聚集分布测试**: 测试对象聚集时的性能表现
- **实时监控**: 动态场景下的性能监控

**预期结果**:
- 对象数量越多，八叉树优势越明显
- 均匀分布比聚集分布性能更好
- 查询性能通常有2-10倍提升

### 3. OctreeVisualizationDemo.cs
**可视化演示**

提供直观的八叉树工作过程展示:
- **分割过程演示**: 可视化节点分割过程
- **查询效果展示**: 实时显示查询范围和结果
- **压力测试可视化**: 大量对象的动态演示
- **交互式控制**: 手动添加对象和调整参数

**可视化元素**:
- 青色线框: 八叉树节点边界
- 红色球体: 普通对象
- 绿色球体: 查询结果对象
- 黄色球体: 查询范围
- 白色球体: 查询中心点

## 快速开始指南

### 第一步: 基础测试
1. 在空场景中创建空GameObject
2. 添加 `OctreeTestManager` 脚本
3. 确保 `runTestsOnStart = true`
4. 运行游戏，查看Console输出

### 第二步: 可视化演示
1. 添加 `OctreeVisualizationDemo` 脚本到同一GameObject
2. 在Scene视图中观察可视化效果
3. 使用GUI面板进行交互操作
4. 尝试不同的演示模式

### 第三步: 性能测试
1. 添加 `OctreePerformanceComparison` 脚本
2. 运行性能对比测试
3. 分析Console中的性能数据
4. 尝试调整测试规模

## 测试用例详解

### 基本功能测试
```csharp
// 测试添加操作
octree.Add(obj, position);
Assert.AreEqual(expectedCount, octree.Count);

// 测试删除操作  
bool removed = octree.Remove(obj);
Assert.IsTrue(removed);

// 测试查询操作
var results = octree.GetNearby(center, radius);
Assert.IsTrue(results.Length > 0);
```

### 分割测试用例
```csharp
// 在小范围内添加超过8个对象
for (int i = 0; i < 12; i++) {
    Vector3 pos = basePos + Random.insideUnitSphere * 2f;
    octree.Add(new TestObject(i), pos);
}
// 验证: 应该触发节点分割
```

### 边界扩展测试
```csharp
// 添加超出初始边界的对象
Vector3 farPosition = new Vector3(worldSize * 2, 0, 0);
octree.Add(obj, farPosition);
// 验证: 八叉树应自动扩展边界
```

### 合并测试用例
```csharp
// 先添加足够对象触发分割
// 然后删除大部分对象
for (int i = 0; i < 10; i++) {
    octree.Remove(objects[i]);
}
// 验证: 应该触发节点合并
```

## 常见问题

### Q: 测试失败怎么办？
A: 检查以下几点:
1. 确保八叉树类在正确的命名空间中
2. 验证minNodeSize设置是否合理
3. 检查对象的Equals方法实现
4. 查看Console中的详细错误信息

### Q: 性能测试结果不理想？
A: 可能原因:
1. 测试数据量太小，看不出性能差异
2. 对象分布过于聚集，影响八叉树效率
3. 查询半径设置不当
4. 硬件性能限制

### Q: 可视化显示异常？
A: 检查项目:
1. 确保在Scene视图中观察
2. 检查Gizmos是否启用
3. 验证可视化参数设置
4. 确保对象位置在合理范围内

## 自定义测试

### 添加新的测试对象类型
```csharp
public class MyTestObject {
    public Vector3 position;
    public string data;
    
    public override bool Equals(object obj) {
        // 实现相等性比较
    }
    
    public override int GetHashCode() {
        // 实现哈希码
    }
}
```

### 创建专门的测试场景
```csharp
[ContextMenu("我的测试场景")]
public void MyCustomTest() {
    // 1. 准备测试数据
    // 2. 执行测试操作
    // 3. 验证结果
    // 4. 输出报告
}
```

## 测试报告解读

### 性能指标
- **添加时间**: 每个对象的平均添加时间
- **查询时间**: 每次查询的平均时间
- **内存使用**: 估算的内存开销
- **性能提升倍数**: 相对于线性搜索的速度提升

### 正常表现
- 添加: 0.001-0.01ms/对象
- 查询: 0.01-0.1ms/查询
- 性能提升: 2-10倍(取决于数据量)
- 内存开销: 比线性搜索多20-50%

这个测试套件将帮助你全面验证八叉树实现的正确性和性能表现，同时提供直观的可视化反馈。