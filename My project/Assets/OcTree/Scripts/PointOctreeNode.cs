using System.Collections.Generic;
using UnityEngine;

namespace OcTree.Scripts
{
    public class PointOctreeNode<T>
    {
        // 如果一个节点中已经有 NUM_OBJECTS_ALLOWED 个对象，我们就将其拆分成子节点。
        // 一个通常不错的数字似乎是 8-15 左右。
        private const int NUM_OBJECTS_ALLOWED = 8;
        
        private class OctreeObject {
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
        
        public PointOctreeNode(float baseLengthVal, float minSizeVal, Vector3 centerVal) {
            SetValues(baseLengthVal, minSizeVal, centerVal);
        }
        
        public bool Add(T obj, Vector3 objPos) {
            if (!Encapsulates(bounds, objPos)) {
                return false;
            }
            SubAdd(obj, objPos);
            return true;
        }

        public bool Remove(T obj) {
            bool removed = false;

            for (int i = 0; i < objects.Count; i++) {
                if (objects[i].Obj.Equals(obj)) {
                    removed = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!removed && children != null) {
                for (int i = 0; i < 8; i++) {
                    removed = children[i].Remove(obj);
                    if (removed) break;
                }
            }

            if (removed && children != null) {
                // Check if we should merge nodes now that we've removed an item
                if (ShouldMerge()) {
                    Merge();
                }
            }

            return removed;
        }
        
        public int BestFitChild(Vector3 objPos) {
            return (objPos.x <= Center.x ? 0 : 1) + (objPos.y >= Center.y ? 0 : 4) + (objPos.z <= Center.z ? 0 : 2);
        }
        
        private void SetValues(float baseLengthVal, float minSizeVal, Vector3 centerVal) {
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

        private static bool Encapsulates(Bounds outerBounds, Vector3 point) {
            return outerBounds.Contains(point);
        }
        
        private void SubAdd(T obj, Vector3 objPos) {
            // 我们知道，如果走到这一步，它在这个级别是合适的。
            // 我们总是将事物放在尽可能深的子节点中。
            // 因此，如果已经有子节点，我们可以跳过检查，直接向下移动
            if (!HasChildren) {
                if (objects.Count < NUM_OBJECTS_ALLOWED || (SideLength / 2) < minSize) {
                    OctreeObject newObj = new OctreeObject
                    {
                        Obj = obj, 
                        Pos = objPos
                    };
                    objects.Add(newObj);
                    return;
                }
			
                // Enough objects in this node already: Create the 8 children
                int bestFitChild;
                if (children == null) {
                    Split();
                    if (children == null) {
                        Debug.LogError("Child creation failed for an unknown reason. Early exit.");
                        return;
                    }

                    
                    for (int i = objects.Count - 1; i >= 0; i--) {
                        OctreeObject existingObj = objects[i];
                        bestFitChild = BestFitChild(existingObj.Pos);
                        children[bestFitChild].SubAdd(existingObj.Obj, existingObj.Pos); // Go a level deeper					
                        objects.Remove(existingObj); // Remove from here
                    }
                }
            }
            
            int bestFit = BestFitChild(objPos);
            children[bestFit].SubAdd(obj, objPos);
        }

        
        private bool ShouldMerge() {
            int totalObjects = objects.Count;
            if (children != null) {
                foreach (PointOctreeNode<T> child in children) {
                    if (child.children != null) {
                        // If any of the *children* have children, there are definitely too many to merge,
                        // or the child woudl have been merged already
                        return false;
                    }
                    totalObjects += child.objects.Count;
                }
            }
            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }
        
        void Merge() {
            for (int i = 0; i < 8; i++) {
                PointOctreeNode<T> curChild = children[i];
                int numObjects = curChild.objects.Count;
                for (int j = numObjects - 1; j >= 0; j--) {
                    OctreeObject curObj = curChild.objects[j];
                    objects.Add(curObj);
                }
            }
            
            children = null;
        }
        
        void Split() {
            float quarter = SideLength / 4f;
            float newLength = SideLength / 2;
            children = new PointOctreeNode<T>[8];
            children[0] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, quarter, -quarter));
            children[1] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, quarter, -quarter));
            children[2] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, quarter, quarter));
            children[3] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, quarter, quarter));
            children[4] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, -quarter, -quarter));
            children[5] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, -quarter, -quarter));
            children[6] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, -quarter, quarter));
            children[7] = new PointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, -quarter, quarter));
        }
    }
}