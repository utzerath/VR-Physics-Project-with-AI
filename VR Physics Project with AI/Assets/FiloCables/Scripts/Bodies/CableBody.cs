using UnityEngine;

namespace Filo{

    public abstract class CableBody : MonoBehaviour
    {
        public enum CablePlane{
            XY,
            XZ,
            YZ,
            Custom
        }
    
        public CablePlane plane;
        public Vector3 planeNormal;
        public bool freezeRotation = true;
        private Rigidbody rbody = null;

        private void Awake()
        {
            FindRigidbody();
        }

        public void FindRigidbody(){
            rbody = GetComponentInParent<Rigidbody>();
            CalculateInertiaTensor();
        }

        public Rigidbody GetRigidbody(){
            return rbody;
        }

        /**
         * Transforms a world space point to cable space.
         */
        public Vector3 WorldToCable(Vector3 wsPoint){
    
            Vector3 lsPoint = transform.InverseTransformPoint(wsPoint);
    
            switch(plane){
                case CablePlane.XY: return lsPoint;
                case CablePlane.XZ: return new Vector3(lsPoint.z,lsPoint.x,lsPoint.y);
                case CablePlane.YZ: return new Vector3(lsPoint.y,lsPoint.z,lsPoint.x);
                case CablePlane.Custom: return Quaternion.Inverse(Quaternion.LookRotation(planeNormal, Vector3.up)) * lsPoint;
                default: return Vector3.zero; 
            }
        }

        /**
         * Transforms a cable space point to world space.
         */
        public Vector3 CableToWorld(Vector3 cablePoint){
    
            Vector3 lsPoint;

            switch(plane){
                case CablePlane.XY: lsPoint = cablePoint; break;
                case CablePlane.XZ: lsPoint = new Vector3(cablePoint.y, cablePoint.z, cablePoint.x); break;
                case CablePlane.YZ: lsPoint = new Vector3(cablePoint.z,cablePoint.x,cablePoint.y); break;
                case CablePlane.Custom: lsPoint = Quaternion.LookRotation(planeNormal, Vector3.up) * cablePoint; break;
                default: lsPoint = Vector3.zero; break;
            }
    
            return transform.TransformPoint(lsPoint);
        }

        /**
         * Projects a 3D world-space point to the local-space 2D cable plane.
         */
        public Vector2 WorldSpaceToCablePlane(Vector3 wsPoint){
    
            Vector3 lsPoint = transform.InverseTransformPoint(wsPoint);
    
            switch(plane){
                case CablePlane.XY: return new Vector2(lsPoint.x,lsPoint.y); 
                case CablePlane.XZ: return new Vector2(lsPoint.z,lsPoint.x);
                case CablePlane.YZ: return new Vector2(lsPoint.y,lsPoint.z);
                case CablePlane.Custom: var vector = Quaternion.Inverse(Quaternion.LookRotation(planeNormal, Vector3.up)) * lsPoint;
                                        return new Vector2(vector.x, vector.y);
                default: return Vector2.zero; 
            }
        }
    
        /**
         * Transforms a local-space 2D cable plane to world space.
         */
        public Vector3 CablePlaneToWorldSpace(Vector2 cablePoint){
    
            Vector3 lsPoint;
            switch(plane){
                case CablePlane.XY: lsPoint = new Vector3(cablePoint.x,cablePoint.y,0); break;
                case CablePlane.XZ: lsPoint = new Vector3(cablePoint.y, 0, cablePoint.x); break;
                case CablePlane.YZ: lsPoint = new Vector3(0,cablePoint.x,cablePoint.y); break;
                case CablePlane.Custom: lsPoint = Quaternion.LookRotation(planeNormal, Vector3.up) * cablePoint; break;
                default: lsPoint = Vector3.zero; break;
            }
    
            return transform.TransformPoint(lsPoint);
            
        }

        public Vector3 GetWorldSpaceTangent(Vector3 origin, bool orientation){
            return CablePlaneToWorldSpace(GetLeftOrRightMostPointFromOrigin(WorldSpaceToCablePlane(origin),orientation));
        }

        public Vector3 GetCablePlaneNormal(){

            switch(plane){
                case CablePlane.XY: return transform.forward; 
                case CablePlane.XZ: return transform.up; 
                case CablePlane.YZ: return transform.right;
                case CablePlane.Custom: return transform.TransformDirection(planeNormal);
                default: return Vector3.zero; 
            }
   
        }

        public virtual void ApplyFreezing(){

            if (rbody != null && freezeRotation)
            {
                // project angular velocity onto world space inertia axis (which is orthogonal to cable plane):
                rbody.angularVelocity = Vector3.Project(rbody.angularVelocity,GetCablePlaneNormal());
            }
   
        }

        public virtual void CalculateInertiaTensor(){

            if (rbody != null){

                rbody.ResetInertiaTensor();

                if (freezeRotation)
                switch(plane){
                    case CablePlane.XY: rbody.inertiaTensor = new Vector3(Mathf.Infinity,Mathf.Infinity,rbody.inertiaTensor.z); break;
                    case CablePlane.XZ: rbody.inertiaTensor = new Vector3(Mathf.Infinity,rbody.inertiaTensor.y,Mathf.Infinity); break;
                    case CablePlane.YZ: rbody.inertiaTensor = new Vector3(rbody.inertiaTensor.x,Mathf.Infinity,Mathf.Infinity); break;
                    case CablePlane.Custom: break;
                }
            }
        }

        public float GetVelocityAtPointAlongDir(Vector3 point, Vector3 direction)
        {
            if (rbody != null)
                return Vector3.Dot(rbody.GetPointVelocity(point), direction);
            return 0;
        }

        public void GetInverseInertiaTensor(ref Matrix4x4 tensor)
        {
            if (rbody != null)
            {
                Vector3 invInertia1 = rbody.inertiaTensorRotation * new Vector3(rbody.inertiaTensor.x > 0 ? 1.0f / rbody.inertiaTensor.x : 0,
                                                                                rbody.inertiaTensor.y > 0 ? 1.0f / rbody.inertiaTensor.y : 0,
                                                                                rbody.inertiaTensor.z > 0 ? 1.0f / rbody.inertiaTensor.z : 0);

                Matrix4x4 m = Matrix4x4.Rotate(rbody.rotation);
                tensor[0, 0] = invInertia1.x;
                tensor[1, 1] = invInertia1.y;
                tensor[2, 2] = invInertia1.z;
                tensor[3, 3] = 1;
                tensor = m * tensor * m.transpose;
            }
        }

        public void ApplyImpulse(Vector3 impulse, Vector3 r, float invMass, ref Matrix4x4 invInertiaTensor)
        {
            if (rbody != null && !rbody.isKinematic)
            {
                rbody.velocity += impulse * invMass;
                rbody.angularVelocity += invInertiaTensor.MultiplyVector(Vector3.Cross(r, impulse));
            }
        }

        /**
         * Returns a worldspace point that lies in the 2D convex hull of the body. 
         */
        public abstract Vector3 RandomHullPoint();
    
        /**
         * Returns the left or rightmost visible point in the cable plane convex hull from a given origin.
         */
        public abstract Vector2 GetLeftOrRightMostPointFromOrigin(Vector2 origin, bool orientation);
    
        /**
         * Returns the distance along the convex hull surface between two points in the cable plane.
         * The sign of the distance depends on body orientation.
         */
        public abstract float SurfaceDistance(Vector2 p1, Vector2 p2,bool orientation,bool shortest = true);

        public abstract Vector3 SurfacePointAtDistance(Vector3 origin, float distance, bool orientation, out int index);

        public abstract void AppendSamples(CableRenderer.SampledCable samples, Vector3 origin, float spacing, float distance , float spoolSeparation, bool reverse, bool orientation);
    }
}

