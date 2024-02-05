using System;
using System.Collections.Generic;
using UnityEngine;

namespace Filo
{

    [ExecuteInEditMode]
    [AddComponentMenu("Filo Cables/Cable")]
    public class Cable : MonoBehaviour
    {

        [Serializable]
        public class Link
        {

            public enum LinkType
            {
                Attachment,
                Rolling,
                Pinhole,
                Hybrid
            }

            public CableBody body;
            public LinkType type;
            public bool orientation;
            public float slack;

            public bool hybridRolling;
            public float storedCable;
            public float spoolSeparation;
            public float cableSpawnSpeed;

            [NonSerialized] public float cableVelocity = 0;

            public Vector3 inAnchor;
            public Vector3 outAnchor;
        }

        [Tooltip("Dynamically creates/removes new cable links using body colliders, allowing dynamic cable path change at runtime.")]
        public bool dynamicSplitMerge = false;

        public List<Link> links = null;
        [HideInInspector] [SerializeField] private List<CableJoint> joints = null;
        [HideInInspector] [SerializeField] private float restLength = 0;

        public float RestLength
        {
            get { return restLength; }
        }

        public int JointCount
        {
            get { return joints.Count; }
        }

        public IList<CableJoint> Joints
        {
            get { return joints != null ? joints.AsReadOnly() : null; }
        }

        void Start()
        {
            Setup();
        }

        void OnValidate()
        {
            Setup();
        }

        public void OnDrawGizmosSelected()
        {

            if (joints == null) return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < joints.Count; ++i)
            {

                if (joints[i] != null)
                {
                    Vector3 pos1 = joints[i].WorldSpaceAttachment1;
                    Vector3 pos2 = joints[i].WorldSpaceAttachment2;

                    Gizmos.DrawLine(pos1, pos2);
                    Gizmos.DrawWireSphere(pos1, 0.02f);
                    Gizmos.DrawWireSphere(pos2, 0.02f);
                }
            }

        }

        public void Setup()
        {
            InitializeLinks();
            GenerateJoints();
            CalculateRestLength();
        }

        private void InitializeLinks()
        {
            if (links != null)
            {
                for (int i = 0; i < links.Count; ++i)
                    links[i].hybridRolling = links[i].type == Link.LinkType.Hybrid && links[i].storedCable > 0;
            }
        }

        public CableJoint GetPreviousJoint(int linkIndex, bool closed)
        {
            if (linkIndex > 0)
                return joints[linkIndex - 1];
            if (closed && joints.Count > 0)
                return joints[joints.Count - 1];
            return null;
        }

        public CableJoint GetNextJoint(int linkIndex, bool closed)
        {
            if (linkIndex < joints.Count)
                return joints[linkIndex];
            if (closed && joints.Count > 0)
                return joints[0];
            return null;
        }

        private void GenerateJoints()
        {
            joints = null;
            if (links != null && links.Count > 0)
            {
                joints = new List<CableJoint>(links.Count - 1);

                for (int i = 0; i < links.Count - 1; ++i)
                {

                    Link link1 = links[i];
                    Link link2 = links[i + 1];

                    if (link1.body != null && link2.body != null)
                    {

                        Vector3 t1, t2;
                        FindCommonTangents(link1, link2, out t1, out t2, true);

                        if (link1.type == Link.LinkType.Hybrid)
                            t1 -= link1.body.GetCablePlaneNormal() * link1.storedCable * link1.spoolSeparation;

                        if (link2.type == Link.LinkType.Hybrid)
                            t2 -= link2.body.GetCablePlaneNormal() * link2.storedCable * link2.spoolSeparation;

                        joints.Add(new CableJoint(link1, link2,
                                                  link1.body.transform.InverseTransformPoint(t1),
                                                  link2.body.transform.InverseTransformPoint(t2),
                                                  (t2 - t1).magnitude + link1.slack)); 
                    }
                    else
                        joints.Add(null); // add a null joint so that there's always as many joints as links minus one.
                }
            }
        }

        private void CalculateRestLength()
        {

            restLength = 0;
            if (joints == null) return;

            bool closed = (links[0].body == links[links.Count - 1].body);

            for (int i = 0; i < links.Count; ++i)
            {
                Link link = links[i];

                if (link.body != null)
                {

                    CableJoint prevJoint = GetPreviousJoint(i, closed);
                    CableJoint nextJoint = GetNextJoint(i, closed);

                    if (nextJoint != null && prevJoint != null && (!(i == 0 && closed)))
                    {

                        link.storedCable = Mathf.Abs(link.body.SurfaceDistance(link.body.WorldSpaceToCablePlane(prevJoint.WorldSpaceAttachment2),
                                                                               link.body.WorldSpaceToCablePlane(nextJoint.WorldSpaceAttachment1),
                                                                               link.orientation));
                        restLength += link.storedCable;

                    }
                    else if (link.type == Link.LinkType.Hybrid)
                    {

                        restLength += link.storedCable;

                        if (nextJoint != null)
                        {
                            Vector2 tangent = link.body.WorldSpaceToCablePlane(nextJoint.WorldSpaceAttachment1);
                            int j = 0;

                            link.outAnchor = link.body.transform.InverseTransformPoint(link.body.SurfacePointAtDistance(tangent, link.storedCable, link.orientation, out j));
                        }
                        else if (prevJoint != null)
                        {
                            Vector2 tangent = link.body.WorldSpaceToCablePlane(prevJoint.WorldSpaceAttachment2);
                            int j = 0;
                            link.inAnchor = link.body.transform.InverseTransformPoint(link.body.SurfacePointAtDistance(tangent, link.storedCable, !link.orientation, out j));
                        }
                    }

                    if (i < links.Count - 1 && joints[i] != null)
                    {
                        restLength += joints[i].restLength;
                    }
                }
            }
        }

        private void FindCommonTangents(Link link1, Link link2, out Vector3 t1, out Vector3 t2, bool initHybrid = false)
        {

            // Pick a random point in each shape:
            t1 = link1.body.RandomHullPoint();
            t2 = link2.body.RandomHullPoint();

            Vector3 prevT1, prevT2;

            // iterate: find a tangent point on the other body until both tangent points remain unchanged.
            int maxIter = 40;
            int i = 0;
            do
            {

                prevT1 = t1;
                prevT2 = t2;

                if (link2.type == Link.LinkType.Attachment || link2.type == Link.LinkType.Pinhole || (link2.type == Link.LinkType.Hybrid && !link2.hybridRolling && !initHybrid))
                    t2 = link2.body.transform.TransformPoint(link2.inAnchor);
                else
                    t2 = link2.body.GetWorldSpaceTangent(t1, link2.orientation);

                if (link1.type == Link.LinkType.Attachment || link1.type == Link.LinkType.Pinhole || (link1.type == Link.LinkType.Hybrid && !link1.hybridRolling && !initHybrid))
                    t1 = link1.body.transform.TransformPoint(link1.outAnchor);
                else
                    t1 = link1.body.GetWorldSpaceTangent(t2, !link1.orientation);

            } while (((prevT1 - t1).sqrMagnitude > 1E-6 ||
                      (prevT2 - t2).sqrMagnitude > 1E-6) && i++ < maxIter);

        }


        private void UpdatePinhole(CableJoint joint1, CableJoint joint2, float deltaTime)
        {
            if (joint1 != null && joint2 != null)
            {
                float restLength1 = joint1.restLength;
                float restLength2 = joint2.restLength;

                if (joint1.length > restLength1)
                {
                    float delta = joint1.length - restLength1;
                    joint1.restLength += delta;
                    joint2.restLength -= delta;
                }
                if (joint2.length > restLength2)
                {
                    float delta = joint2.length - restLength2;
                    joint1.restLength -= delta;
                    joint2.restLength += delta;
                }
            }
        }

        private void UpdatePinholes(float deltaTime)
        {
            for (int i = 1; i < links.Count - 1; ++i)
            {
                if (links[i].body != null && joints[i - 1] != null && joints[i] != null)
                {
                    if (links[i].type == Link.LinkType.Pinhole)
                        UpdatePinhole(joints[i - 1], joints[i], deltaTime);
                }
            }
        }

        private void UpdateHybridLink(Link link, bool cableGoesIn, Vector3 attachment)
        {

            if (link.storedCable <= 0 && link.hybridRolling)
            {

                // Switch to attachment mode:
                link.hybridRolling = false;

                // Update joints again, since joint attachment points have now changed.
                UpdateJoints();

            }
            else if (!link.hybridRolling)
            {

                // Find positive and negative attachment points:
                Vector3 tplus = link.body.GetWorldSpaceTangent(attachment, false);
                Vector3 tminus = link.body.GetWorldSpaceTangent(attachment, true);

                // Current cable space attachment point:
                Vector2 t = link.body.WorldSpaceToCablePlane(link.body.transform.TransformPoint(cableGoesIn ? link.inAnchor : link.outAnchor));

                // Calculate distance to positive and negative attachment points:
                float d1 = link.body.SurfaceDistance(link.body.WorldSpaceToCablePlane(tplus), t, false);
                float d2 = link.body.SurfaceDistance(link.body.WorldSpaceToCablePlane(tminus), t, false);

                // In case the attachment point exceeds either tangent, go back to rolling mode and adapt orientation accordingly:
                if (d1 < 0 || d2 > 0)
                {

                    // Pick the closest tangent. This avoids issues when both distances change sign at the same time.
                    if (Mathf.Abs(d1) < Mathf.Abs(d2))
                    {
                        link.hybridRolling = true;
                        link.orientation = !cableGoesIn;
                    }
                    else
                    {
                        link.hybridRolling = true;
                        link.orientation = cableGoesIn;
                    }
                }

            }
        }

        private void UpdateHybridLinks()
        {

            // Only first and last link can be hybrid, update them:
            if (links[0].body != null && links[0].type == Link.LinkType.Hybrid)
            {
                UpdateHybridLink(links[0], false, joints[0].WorldSpaceAttachment2);
            }

            if (links.Count > 1)
            {
                int lastLink = links.Count - 1;
                if (links[lastLink].body != null && links[lastLink].type == Link.LinkType.Hybrid)
                    UpdateHybridLink(links[lastLink], true, joints[joints.Count - 1].WorldSpaceAttachment1);
            }
        }

        private void UpdateJoints()
        {

            for (int i = 0; i < joints.Count; ++i)
            {

                if (joints[i] != null && links[i].body != null && links[i + 1].body != null)
                {

                    CableJoint joint = joints[i];
                    Link link1 = links[i];
                    Link link2 = links[i + 1];

                    Vector3 t1, t2;
                    FindCommonTangents(links[i], links[i + 1], out t1, out t2);

                    Vector2 currentT1 = joint.link1.body.WorldSpaceToCablePlane(joint.WorldSpaceAttachment1);
                    Vector2 currentT2 = joint.link2.body.WorldSpaceToCablePlane(joint.WorldSpaceAttachment2);

                    // Get surface distances between old and new tangents:
                    float d1 = joint.link1.body.SurfaceDistance(currentT1, joint.link1.body.WorldSpaceToCablePlane(t1), links[i].orientation);
                    float d2 = joint.link2.body.SurfaceDistance(currentT2, joint.link2.body.WorldSpaceToCablePlane(t2), links[i + 1].orientation);

                    // Spawn more cable if necessary
                    if (links[i].type == Link.LinkType.Attachment)
                    {
                        d1 += links[i].cableSpawnSpeed;
                        restLength += links[i].cableSpawnSpeed;
                    }

                    if (links[i + 1].type == Link.LinkType.Attachment)
                    {
                        d2 -= links[i + 1].cableSpawnSpeed;
                        restLength += links[i + 1].cableSpawnSpeed;
                    }

                    // Update hybrid link attachment points (displace along the plane normal based on amount of stored cable):
                    if (links[i].type == Link.LinkType.Hybrid)
                        t1 -= joint.link1.body.GetCablePlaneNormal() * links[i].storedCable * links[i].spoolSeparation;

                    if (links[i + 1].type == Link.LinkType.Hybrid)
                        t2 -= joint.link2.body.GetCablePlaneNormal() * links[i + 1].storedCable * links[i + 1].spoolSeparation;

                    // Update stored lengths:
                    link1.storedCable -= d1;
                    link2.storedCable += d2;

                    // Update rest lengths:
                    joint.restLength += d1;
                    joint.restLength -= d2;

                    // Update tangent points:
                    joint.offset1 = joint.link1.body.transform.InverseTransformPoint(t1);
                    joint.offset2 = joint.link2.body.transform.InverseTransformPoint(t2);

                }

            }
        }

        public void UpdateCable(float deltaTime)
        {

            if (joints == null) return;

            for (int i = 0; i < joints.Count; ++i)
            {
                if (joints[i] != null)
                    joints[i].UpdateLength();
            }

            UpdateJoints();

            UpdateHybridLinks();

            UpdatePinholes(deltaTime);

            SplitMerge();

            for (int i = 0; i < joints.Count; ++i)
            {
                if (joints[i] != null)
                    joints[i].UpdateMasses(); 
            }

        }

        private void SplitMerge()
        {

            if (!dynamicSplitMerge) return;

            // merge links with negative stored cable:
            for (int i = 1; i < links.Count - 1; ++i)
            {

                CableJoint prevJoint = joints[i - 1];
                CableJoint nextJoint = joints[i];

                if (links[i].type == Link.LinkType.Rolling && links[i].body != null && prevJoint != null && nextJoint != null)
                {

                    if (links[i].storedCable < 0)
                    {

                        prevJoint.restLength += nextJoint.restLength;
                        prevJoint.Link2 = nextJoint.Link2;

                        // Update joint attachment points:
                        Vector3 t1, t2;
                        FindCommonTangents(links[i - 1], links[i + 1], out t1, out t2);
                        prevJoint.offset1 = prevJoint.Link1.body.transform.InverseTransformPoint(t1);
                        prevJoint.offset2 = prevJoint.Link2.body.transform.InverseTransformPoint(t2);
                        prevJoint.UpdateLength();

                        links.RemoveAt(i);
                        joints.RemoveAt(i);
                    }

                }
            }

            // split joints that intersect a body:
            for (int i = 0; i < joints.Count; ++i)
            {

                CableJoint currentJoint = joints[i];

                if (currentJoint != null)
                {

                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(currentJoint.WorldSpaceAttachment1, currentJoint.WorldSpaceAttachment2 - currentJoint.WorldSpaceAttachment1), out hit, currentJoint.length))
                    {

                        CableBody body = hit.collider.GetComponent<CableBody>();

                        // Only split if the body is a disc or a convex shape, and the raycast hit is sane.
                        if ((body is CableDisc || body is CableShape) && hit.distance > 0.1f && hit.distance + 0.1f < currentJoint.length)
                        {

                            float initialRestLength = currentJoint.restLength;

                            // Create new link and joint:
                            Link newLink = new Link();
                            newLink.type = Link.LinkType.Rolling;
                            newLink.body = body;

                            CableJoint newJoint = new CableJoint(newLink, currentJoint.Link2, Vector3.zero, Vector3.zero, currentJoint.restLength);
                            currentJoint.Link2 = newLink;

                            // Calculate orientation.
                            Vector3 v = Vector3.Cross(body.GetCablePlaneNormal(),currentJoint.WorldSpaceAttachment2 - currentJoint.WorldSpaceAttachment1);
                            newLink.orientation = Vector3.Dot(hit.point - body.transform.position, v) < 0;

                            // Update joint attachment points:
                            Vector3 t1, t2;
                            FindCommonTangents(links[i], newLink, out t1, out t2);
                            currentJoint.offset1 = currentJoint.Link1.body.transform.InverseTransformPoint(t1);
                            currentJoint.offset2 = currentJoint.Link2.body.transform.InverseTransformPoint(t2);

                            FindCommonTangents(newLink, links[i + 1], out t1, out t2);
                            newJoint.offset1 = newJoint.Link1.body.transform.InverseTransformPoint(t1);
                            newJoint.offset2 = newJoint.Link2.body.transform.InverseTransformPoint(t2);

                            currentJoint.UpdateLength();
                            newJoint.UpdateLength();

                            // Adjust rest lengths so that tensions are equal:
                            float tension = initialRestLength / (currentJoint.length + newJoint.length);
                            currentJoint.restLength = currentJoint.length * tension;
                            newJoint.restLength = newJoint.length * tension;

                            // Insert new joint/link:
                            joints.Insert(i + 1, newJoint);
                            links.Insert(i + 1, newLink);
                        }
                    }
                }
            }
        }

        public void Solve(float deltaTime, float bias)
        {
            if (joints == null) return;

            for (int i = 0; i < joints.Count; ++i)
            {
                if (joints[i] != null)
                    joints[i].SolveVelocities(deltaTime, bias);
            }

            for (int i = 0; i < links.Count; ++i)
            {
                if (links[i].body != null)
                    links[i].body.ApplyFreezing();
            }
        }

    }
}
