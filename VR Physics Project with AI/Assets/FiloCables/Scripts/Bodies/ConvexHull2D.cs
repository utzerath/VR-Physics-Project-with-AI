using System;
using System.Collections.Generic;
using UnityEngine;

namespace Filo{
    [CreateAssetMenu(menuName = "Filo Cables/ConvexHull2D")]
    public class ConvexHull2D : ScriptableObject
    {
        public CableBody.CablePlane plane;
        public Vector3 planeNormal;
        public Mesh mesh;
        
        [HideInInspector] public List<Vector2> input = new List<Vector2>();
        [HideInInspector][SerializeField] public List<Vector2> hull = new List<Vector2>();
        [HideInInspector][SerializeField] public float perimeter = 0;
        [HideInInspector][SerializeField] public Rect bounds;

        private void OnValidate(){
            MeshPlaneIntersection();
            ConvexHull();
            UpdateHullPerimeterAndBounds();
        }

        /**
         * Returns the index of the leftmost point of a list of 2D points.
         */
        private int LeftMostInputPoint(){
    
            int leftMost = 0;
    
            for (int i = 0; i < input.Count; ++i)
                if (input[i].x < input[leftMost].x)
                    leftMost = i;

            return leftMost;
        }

        /**
         * To find orientation of ordered triplet (p, q, r). 
         * The function returns following values 
         * 0 --> p, q and r are colinear 
         * 1 --> Clockwise 
         *-1 --> Counterclockwise 
         */
        public static int Orientation(Vector2 p, Vector2 q, Vector2 r) 
        { 
            float val = (q.y - p.y) * (r.x - q.x) - 
                        (q.x - p.x) * (r.y - q.y); 
          
            if (val == 0) return 0;   // colinear 
            return (val > 0) ? 1: -1; // clock or counterclock wise 
        } 
    
        /**
         * Calculates the convex hull of a list of points using Jarvis' algorithm (wrapping).
         */
        private void ConvexHull(){
    
            // Trivial case: all points are part of the hull:
            if (input.Count <= 3){
                hull = new List<Vector2>(input);
                return;
            }
            
            // Starting at the leftmost point:
            int l = LeftMostInputPoint();
    
            hull.Clear();
    
            int p = l, q;
            do{
    
                hull.Add(input[p]);
    
                q = (p+1)%input.Count;

                // Find the most counter-clockwise point:
                for (int i = 0; i < input.Count; ++i){
                    if (Orientation(input[p],input[i],input[q]) < 0)
                        q = i;
                }
            
                p = q;
    
            }while (p != l);
   
        }

        private void UpdateHullPerimeterAndBounds(){
            perimeter = 0;
            
            for (int i = 0; i < hull.Count; ++i){

                bounds.xMin = Mathf.Min(bounds.xMin,hull[i].x);
                bounds.yMin = Mathf.Min(bounds.yMin,hull[i].y);

                bounds.xMax = Mathf.Max(bounds.xMin,hull[i].x);
                bounds.yMax = Mathf.Max(bounds.yMin,hull[i].y);

                int next = i+1;
                if (next == hull.Count) 
                    next = 0;
                perimeter += Vector3.Distance(hull[i],hull[next]);
            }
        }

        private bool LinePlaneIntersect(Vector3 p1, Vector3 p2, Vector3 normal, ref Vector3 intersection){

            Vector3 u = p2 - p1;
            float dot = Vector3.Dot(normal, u);

            if (Mathf.Abs(dot) > 1E-6)
            {
                float s = - Vector3.Dot(normal, p1) / dot;
                if (s >= 0 && s <= 1){
                    intersection = p1 + s * u;
                    return true;
                }
            }
            return false;
        }

        private void AppendInputPoint(Vector3 p){
            switch(plane){
                case CableBody.CablePlane.XY: input.Add(new Vector2(p.x,p.y)); break;
                case CableBody.CablePlane.XZ: input.Add(new Vector2(p.x,p.z)); break;
                case CableBody.CablePlane.YZ: input.Add(new Vector2(p.y,p.z)); break;
                case CableBody.CablePlane.Custom: input.Add(Quaternion.Inverse(Quaternion.LookRotation(planeNormal, Vector3.up)) * p); break;
            }
        }

        private void MeshPlaneIntersection(){

            input.Clear();

            if (mesh == null)
                return;

            Vector3 normal = planeNormal;
            switch(plane){
                case CableBody.CablePlane.XY: normal = new Vector3(0, 0, 1); break;
                case CableBody.CablePlane.XZ: normal = new Vector3(0, 1, 0); break;
                case CableBody.CablePlane.YZ: normal = new Vector3(1, 0, 0); break;
                case CableBody.CablePlane.Custom: normal = planeNormal; break;
            }

            Vector3[] vertices = mesh.vertices;
            int[] tris = mesh.triangles;

            // Determine intersection points for each triangle:
            Vector3 it = Vector3.zero;
            for (int i = 0; i < tris.Length; i += 3) {

                if (LinePlaneIntersect(vertices[tris[i]], vertices[tris[i + 1]], normal, ref it))
                    AppendInputPoint(it);
                
    
                if (LinePlaneIntersect(vertices[tris[i]],vertices[tris[i+2]], normal, ref it))
                    AppendInputPoint(it);
                

                if (LinePlaneIntersect(vertices[tris[i+2]],vertices[tris[i+1]], normal, ref it))
                    AppendInputPoint(it);

            }

            CleanInput();
        }

        private void CleanInput(){
            
            int i = 0;
            while (i < input.Count)
            {
                Vector2 value = input[i];
     
                int j = i + 1;
                while (j < input.Count)
                {
                    if (Vector2.SqrMagnitude(value - input[j]) < 1E-4)
                        input.RemoveAt(j);
                    else
                        ++j;
                }
                ++i;
            }
            
        }
    
    }
}


