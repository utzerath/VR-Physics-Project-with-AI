using System;
using UnityEngine;

namespace Filo{

    public class CableJoint {
    
        public Cable.Link link1;
        public Vector3 offset1;    
        
        public Cable.Link link2;
        public Vector3 offset2;
       
        [HideInInspector] public float length = 0;
        public float restLength = 1;

        private float totalLambda = 0;

        private float invMass1;
        private float invMass2;
        private Matrix4x4 invInertiaTensor1;
        private Matrix4x4 invInertiaTensor2;

        private Vector3 worldOffset1;
        private Vector3 worldOffset2;
        private Vector3 r1;
        private Vector3 r2;
        private Vector3 jacobian;
        private float k;    

        private Vector3 impulse;
        private float lambda;

        public Vector3 WorldSpaceAttachment1{
            get{return link1.body ? link1.body.transform.TransformPoint(offset1) : Vector3.zero;}
        }
        
        public Vector3 WorldSpaceAttachment2{
            get{return link2.body ? link2.body.transform.TransformPoint(offset2) : Vector3.zero;}
        }

        public Cable.Link Link1{
            get{return link1;}
            set{
                this.link1 = value;
            }
        }

        public Cable.Link Link2
        {
            get{return link2;}
            set{
                this.link2 = value;
            }
        }

        public Vector3 Jacobian{
            get{return jacobian;}
        }

        public float ImpulseMagnitude{ //**< Impulse. Divide by fixedDeltaTime to get force in Newtons, divide by external acceleration (gravity) to get mass.*/
            get{return totalLambda;}
        }

        public float Strain{
            get{return restLength > 0 ? length / restLength : 1;}
        }
    
        public CableJoint(Cable.Link link1, Cable.Link link2, Vector3 offset1, Vector3 offset2, float restLength){

            Link1 = link1;
            Link2 = link2;
            this.offset1 = offset1;
            this.offset2 = offset2;
            this.restLength = restLength;

            if (link1.body != null && link2.body != null)
                UpdateLength();
        }

        public void UpdateLength()
        { 
            length = 0;
            jacobian = Vector3.zero;

            if (link1.body == null || link2.body == null) return;

            worldOffset1 = link1.body.transform.TransformPoint(offset1);
            worldOffset2 = link2.body.transform.TransformPoint(offset2);

            Vector3 vector = worldOffset2 - worldOffset1;
            length = vector.magnitude;
            jacobian = vector / (length + 0.00001f);
        }

        public void UpdateMasses()
        {

            totalLambda = 0;
            invMass1 = 0;
            invMass2 = 0;
            invInertiaTensor1 = Matrix4x4.zero;
            invInertiaTensor2 = Matrix4x4.zero;
            float w1 = 0, w2 = 0;

            if (link1 == null || link2 == null) return;

            link1.body.GetInverseInertiaTensor(ref invInertiaTensor1);
            link2.body.GetInverseInertiaTensor(ref invInertiaTensor2);

            var rb1 = link1.body.GetRigidbody();
            var rb2 = link2.body.GetRigidbody();

            if (rb1 != null && !rb1.isKinematic)
            {
                invMass1 = 1.0f/rb1.mass;
                r1 = worldOffset1 - rb1.worldCenterOfMass;
                w1 = Vector3.Dot(Vector3.Cross(invInertiaTensor1.MultiplyVector(Vector3.Cross(r1,jacobian)),r1),jacobian);
            }

            if (rb2 != null && !rb2.isKinematic)
            {
                invMass2 = 1.0f/rb2.mass;
                r2 = worldOffset2 - rb2.worldCenterOfMass;
                w2 = Vector3.Dot(Vector3.Cross(invInertiaTensor2.MultiplyVector(Vector3.Cross(r2,jacobian)),r2),jacobian);
            }

            if (link1.type == Cable.Link.LinkType.Pinhole)
            {
                invMass1 = 1;
                w1 = 1;
            }
            if (link2.type == Cable.Link.LinkType.Pinhole)
            {
                invMass2 = 1;
                w2 = 1;
            }

            k = invMass1 + invMass2 + w1 + w2;
        }

        // multiple cables can be attached to a body, so cable speed must be stored in the link.
        public void SolveVelocities(float deltaTime, float bias)
        {
            UpdateLength();

            // position constraint: distance between attachment points minus rest distance must be zero.
            float c = length - restLength;

            impulse = Vector3.zero;
            lambda = 0;

            if (link1 != null && link2 != null && c > 0 && k > 0)
            {
                // calculate the relative velocity of both attachment points along jacobian:
                float cDot = (link2.body.GetVelocityAtPointAlongDir(worldOffset2, jacobian) + link2.cableVelocity) -
                             (link1.body.GetVelocityAtPointAlongDir(worldOffset1, jacobian) + link1.cableVelocity);

                // calculate constraint force intensity:  
                lambda = (-cDot - c * bias / deltaTime) / k;

                // accumulate and clamp impulse:
                float tempLambda = totalLambda;
                totalLambda = Mathf.Min(0, totalLambda + lambda);
                lambda = totalLambda - tempLambda;

                // apply impulse to both rigidbodies:
                impulse = jacobian * lambda;

                link1.body.ApplyImpulse(-impulse, r1, invMass1, ref invInertiaTensor1);
                link2.body.ApplyImpulse( impulse, r2, invMass2, ref invInertiaTensor2);

                if (link1.type == Cable.Link.LinkType.Pinhole)
                    link1.cableVelocity -= lambda * invMass1;

                if (link2.type == Cable.Link.LinkType.Pinhole)
                    link2.cableVelocity += lambda * invMass2;
            }
        }

    }
}
