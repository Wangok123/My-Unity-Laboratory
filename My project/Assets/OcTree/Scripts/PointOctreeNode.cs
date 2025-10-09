using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcTree.Scripts
{
    public class PointOctreeNode<T>
    {
        // 如果一个节点中已经有 NUM_OBJECTS_ALLOWED 个对象，我们就将其拆分成子节点。
        // 一个通常不错的数字似乎是 8-15 左右。
        private const int NUM_OBJECTS_ALLOWED = 8;

        private class OctreeObject
        {
            public T Obj;
            public Vector3 Pos;
        }

        public Vector3 Center { get; private set; }
        public float SideLength { get; private set; }

        private float minSize;
        private Bounds bounds = default(Bounds);

        private Vector3 actualBoundsSize;

        private PointOctreeNode<T>[] children = null;

        private bool HasChildren => children != null;
        private Bounds[] childBounds;

        private readonly List<OctreeObject> objects = new List<OctreeObject>();

        public static float SqrDistanceToRay(Ray ray, Vector3 point) {
            return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
        }
        
        public PointOctreeNode(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, centerVal);
        }

        public bool Add(T obj, Vector3 objPos)
        {
            if (!Encapsulates(bounds, objPos))
            {
                return false;
            }

            SubAdd(obj, objPos);
            return true;
        }

        public bool Remove(T obj)
        {
            bool removed = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Obj.Equals(obj))
                {
                    removed = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!removed && children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    removed = children[i].Remove(obj);
                    if (removed) break;
                }
            }

            if (removed && children != null)
            {
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        public bool Remove(T obj, Vector3 objPos)
        {
            if (!Encapsulates(bounds, objPos))
            {
                return false;
            }

            return SubRemove(obj, objPos);
        }

        bool SubRemove(T obj, Vector3 objPos)
        {
            bool removed = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Obj.Equals(obj))
                {
                    removed = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!removed && children != null)
            {
                int bestFitChild = BestFitChild(objPos);
                removed = children[bestFitChild].SubRemove(obj, objPos);
            }

            if (removed && children != null)
            {
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        public int BestFitChild(Vector3 objPos)
        {
            return (objPos.x <= Center.x ? 0 : 1) + (objPos.y >= Center.y ? 0 : 4) + (objPos.z <= Center.z ? 0 : 2);
        }

        public void GetNearby(ref Ray ray, float maxDistance, List<T> result) {
            // 1. 扩展边界框用于相交测试
            bounds.Expand(new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
            bool intersected = bounds.IntersectRay(ray);
            bounds.size = actualBoundsSize; // 恢复原始大小
            if (!intersected) {
                return;
            }

            // 2. 检查当前节点中的所有对象
            for (int i = 0; i < objects.Count; i++) {
                if (SqrDistanceToRay(ray, objects[i].Pos) <= (maxDistance * maxDistance)) {
                    result.Add(objects[i].Obj);
                }
            }

            // 3. 递归检查子节点
            if (children != null) {
                for (int i = 0; i < 8; i++) {
                    children[i].GetNearby(ref ray, maxDistance, result);
                }
            }
        }
        
        public void GetNearby(ref Vector3 position, float maxDistance, List<T> result)
        {
            float sqrMaxDistance = maxDistance * maxDistance;
            
            if ((bounds.ClosestPoint(position) - position).sqrMagnitude > sqrMaxDistance)
            {
                return;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if ((position - objects[i].Pos).sqrMagnitude <= sqrMaxDistance)
                {
                    result.Add(objects[i].Obj);
                }
            }

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    children[i].GetNearby(ref position, maxDistance, result);
                }
            }
        }

        public void GetAll(List<T> result) {
            result.AddRange(objects.Select(o => o.Obj));
            
            if (children != null) {
                for (int i = 0; i < 8; i++) {
                    children[i].GetAll(result);
                }
            }
        }
        
        public void DrawAllBounds(float depth = 0) {
            float tintVal = depth / 7; // Will eventually get values > 1. Color rounds to 1 automatically
            Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal);

            Bounds thisBounds = new Bounds(Center, new Vector3(SideLength, SideLength, SideLength));
            Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);

            if (children != null) {
                depth++;
                for (int i = 0; i < 8; i++) {
                    children[i].DrawAllBounds(depth);
                }
            }
            Gizmos.color = Color.white;
        }
        
        public void DrawAllObjects() {
            float tintVal = SideLength / 20;
            Gizmos.color = new Color(0, 1.0f - tintVal, tintVal, 0.25f);

            foreach (OctreeObject obj in objects) {
                Gizmos.DrawIcon(obj.Pos, "marker.tif", true);
            }

            if (children != null) {
                for (int i = 0; i < 8; i++) {
                    children[i].DrawAllObjects();
                }
            }

            Gizmos.color = Color.white;
        }

        public bool HasAnyObjects() {
            if (objects.Count > 0) return true;

            if (children != null) {
                for (int i = 0; i < 8; i++) {
                    if (children[i].HasAnyObjects()) return true;
                }
            }

            return false;
        }
        
        public void SetChildren(PointOctreeNode<T>[] childOctrees) {
            if (childOctrees.Length != 8) {
                Debug.LogError("Child octree array must be length 8. Was length: " + childOctrees.Length);
                return;
            }

            children = childOctrees;
        }
        
        public PointOctreeNode<T> ShrinkIfPossible(float minLength) {
            if (SideLength < (2 * minLength)) {
                return this;
            }
            if (objects.Count == 0 && (children == null || children.Length == 0)) {
                return this;
            }

            // Check objects in root
            int bestFit = -1;
            for (int i = 0; i < objects.Count; i++) {
                OctreeObject curObj = objects[i];
                int newBestFit = BestFitChild(curObj.Pos);
                if (i == 0 || newBestFit == bestFit) {
                    if (bestFit < 0) {
                        bestFit = newBestFit;
                    }
                }
                else {
                    return this; // Can't reduce - objects fit in different octants
                }
            }

            // Check objects in children if there are any
            if (children != null) {
                bool childHadContent = false;
                for (int i = 0; i < children.Length; i++) {
                    if (children[i].HasAnyObjects()) {
                        if (childHadContent) {
                            return this; // Can't shrink - another child had content already
                        }
                        if (bestFit >= 0 && bestFit != i) {
                            return this; // Can't reduce - objects in root are in a different octant to objects in child
                        }
                        childHadContent = true;
                        bestFit = i;
                    }
                }
            }

            // Can reduce
            if (children == null) {
                // We don't have any children, so just shrink this node to the new size
                // We already know that everything will still fit in it
                SetValues(SideLength / 2, minSize, childBounds[bestFit].center);
                return this;
            }

            // We have children. Use the appropriate child as the new root node
            return children[bestFit];
        }
        
        private void SetValues(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SideLength = baseLengthVal;
            minSize = minSizeVal;
            Center = centerVal;

            actualBoundsSize = new Vector3(SideLength, SideLength, SideLength);
            bounds = new Bounds(Center, actualBoundsSize);

            float quarter = SideLength / 4f;
            float childActualLength = SideLength / 2;
            Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
            childBounds = new Bounds[8];
            childBounds[0] = new Bounds(Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
            childBounds[1] = new Bounds(Center + new Vector3(quarter, quarter, -quarter), childActualSize);
            childBounds[2] = new Bounds(Center + new Vector3(-quarter, quarter, quarter), childActualSize);
            childBounds[3] = new Bounds(Center + new Vector3(quarter, quarter, quarter), childActualSize);
            childBounds[4] = new Bounds(Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            childBounds[5] = new Bounds(Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
            childBounds[6] = new Bounds(Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
            childBounds[7] = new Bounds(Center + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        private static bool Encapsulates(Bounds outerBounds, Vector3 point)
        {
            return outerBounds.Contains(point);
        }

        private void SubAdd(T obj, Vector3 objPos)
        {
            // 我们知道，如果走到这一步，它在这个级别是合适的。
            // 我们总是将事物放在尽可能深的子节点中。
            // 因此，如果已经有子节点，我们可以跳过检查，直接向下移动
            if (!HasChildren)
            {
                if (objects.Count < NUM_OBJECTS_ALLOWED || (SideLength / 2) < minSize)
                {
                    //当节点的边长除以2后小于 minSize 时，即使当前节点的对象数量超过了 NUM_OBJECTS_ALLOWED（8个），也不会继续分割。
                    OctreeObject newObj = new OctreeObject
                    {
                        Obj = obj,
                        Pos = objPos
                    };
                    // 直接添加到当前节点
                    objects.Add(newObj);
                    return;
                }

                int bestFitChild;
                if (children == null)
                {
                    // 触发分割
                    Split();
                    if (children == null)
                    {
                        Debug.LogError("Child creation failed for an unknown reason. Early exit.");
                        return;
                    }
                    
                    // 重新分配现有对象到子节点
                    for (int i = objects.Count - 1; i >= 0; i--)
                    {
                        OctreeObject existingObj = objects[i];
                        bestFitChild = BestFitChild(existingObj.Pos);
                        children[bestFitChild].SubAdd(existingObj.Obj, existingObj.Pos); 			
                        objects.Remove(existingObj); 
                    }
                }
            }

            int bestFit = BestFitChild(objPos);
            children[bestFit].SubAdd(obj, objPos);
        }


        private bool ShouldMerge()
        {
            int totalObjects = objects.Count;
            if (children != null)
            {
                foreach (PointOctreeNode<T> child in children)
                {
                    if (child.children != null)
                    {
                        return false;
                    }

                    totalObjects += child.objects.Count;
                }
            }

            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }

        void Merge()
        {
            for (int i = 0; i < 8; i++)
            {
                PointOctreeNode<T> curChild = children[i];
                int numObjects = curChild.objects.Count;
                for (int j = numObjects - 1; j >= 0; j--)
                {
                    OctreeObject curObj = curChild.objects[j];
                    objects.Add(curObj);
                }
            }

            children = null;
        }

        void Split()
        {
            float quarter = SideLength / 4f;
            float newLength = SideLength / 2;
            children = new PointOctreeNode<T>[8];
            children[0] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, quarter, -quarter));
            children[1] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, quarter, -quarter));
            children[2] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, quarter, quarter));
            children[3] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, quarter, quarter));
            children[4] =
                new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, -quarter, -quarter));
            children[5] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, -quarter, -quarter));
            children[6] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, -quarter, quarter));
            children[7] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, -quarter, quarter));
        }
    }
}