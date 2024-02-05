using System;
using System.Collections.Generic;
using UnityEngine;

namespace Filo
{

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Cable))]
    [ExecuteInEditMode]
    [AddComponentMenu("Filo Cables/Cable Renderer")]
    public class CableRenderer : MonoBehaviour
    {
        public class SampledCable
        {
            private List<List<Vector3>> segments = new List<List<Vector3>>();
            private int segmentCount = 0;
            private Vector3 lastSample = Vector3.zero;
            public Matrix4x4 transform = Matrix4x4.identity;

            public IList<List<Vector3>> Segments
            {
                get { return segments; }
            }

            public float Length { get; private set; } = 0;

            public void AppendSample(Vector3 sample, bool accumulateLength = true)
            {
                sample = transform.MultiplyPoint3x4(sample);

                // If this is not the first sample, update the sampled cable length:
                if (accumulateLength && segmentCount > 0 && segments[0].Count > 0)
                    Length += Vector3.Distance(sample, lastSample);

                // Add the first segment, if the list is empty.
                if (segmentCount == 0)
                    segmentCount = 1;

                if (segments.Count == 0)
                    segments.Add(new List<Vector3>());

                // Append sample to last segment:
                segments[segmentCount - 1].Add(sample);
                lastSample = sample;
            }

            public void ReverseLastSamples(int count)
            {
                if (segmentCount > 0)
                {

                    List<Vector3> segment = segments[segmentCount - 1];
                    if (count <= segment.Count)
                    {

                        segment.Reverse(segment.Count - count, count);

                        for (int i = segment.Count - count; i < segment.Count - 1; ++i)
                        {
                            Length += Vector3.Distance(segment[i], segment[i + 1]);
                        }

                        lastSample = segment[segment.Count - 1];

                    }
                }
            }

            public void NewSegment()
            {

                // Increase our segment count. Only if it is larger than the length of the segment list,
                // add a new segment. This allows to reuse empty segments from previous frames and 
                // gets rid of garbage generation.

                segmentCount++;
                if (segments.Count < segmentCount)
                    segments.Add(new List<Vector3>());
            }

            public void Clear()
            {

                // Do not clear the segment list. Instead, clear each segment and leave it empty.
                // This gets rid of garbage generation.
                for (int i = 0; i < segments.Count; ++i)
                    segments[i].Clear();

                // Reset segment count and cable length:
                segmentCount = 0;
                Length = 0;
            }

            public void Close()
            {

                // Re-add the first sample at the end of the cable:
                if (segmentCount > 0 && segments[0].Count > 0)
                {
                    segments[segmentCount - 1].Add(segments[0][0]);
                }
            }

        }

        public class CurveFrame{

            public Vector3 position = Vector3.zero;
            public Vector3 tangent = Vector3.forward;
            public Vector3 normal = Vector3.up;
            public Vector3 binormal = Vector3.left;

            public void Reset(){
                position = Vector3.zero;
                tangent = Vector3.forward;
                normal = Vector3.up;
                binormal = Vector3.left;
            }

            public void Transport(Vector3 newPosition, Vector3 newTangent){

                // Calculate delta rotation:
                Quaternion rotQ = Quaternion.FromToRotation(tangent,newTangent);
               
                // Rotate previous frame axes to obtain the new ones:
                normal = rotQ * normal;
                binormal = rotQ * binormal;
                tangent = newTangent;
                position = newPosition;
            }

            public void DrawDebug(float length){
                Debug.DrawRay(position,normal*length,Color.blue);
                Debug.DrawRay(position,tangent*length,Color.red);
                Debug.DrawRay(position,binormal*length,Color.green);
            }
        }

        [HideInInspector] public SampledCable sampledCable = new SampledCable();
        private List<Vector3> samplesBuffer = new List<Vector3>();

        private Cable cable;
        private MeshFilter filter;
        private Mesh mesh;
        
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector4> tangents = new List<Vector4>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> tris = new List<int>();

        private CurveFrame frame;

        public CableSection section;
        public Vector2 uvScale = Vector2.one;
        public float thickness = 0.025f;

        [Header("Quality")]
        public float edgeLoopSpacing = 0.2f;
        [Min(2)]
        public int maxEdgeLoopsPerLooseJoint = 10;
        [Min(2)]
        public int maxEdgeLoopsPerTautJoint = 2;

        [Header("Looseness")]
        [Range(0, 1)]
        [Tooltip("Percentage of loose cable visually represented. Set it to zero to deactivate cable looseness. The amount of loose cable is first clamped to maxLooseCable, then multiplied by loosenessScale.")]
        public float loosenessScale = 1;
        [Tooltip("Maximum amount of loose cable per cable segment, in meters. Set it to zero to deactivate cable looseness. The amount of loose cable is first clamped to maxLooseCable, then multiplied by loosenessScale.")]
        public float maxLooseCable = 0.25f;

        [Header("Verticality")]
        [Tooltip("Maximum distance between two cable links (projected in the XZ plane) to consider the cable segment vertical.")]
        [Min(1E-4f)]
        public float verticalThreshold = 0.25f;
        [Tooltip("Frequency of the sine curve used to represent vertical cables.")]
        public uint verticalCurlyness = 1;

        public void OnEnable(){

            cable = GetComponent<Cable>();
            filter = GetComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = "cable";
            mesh.MarkDynamic(); 
            filter.mesh = mesh; 
        }

        public void OnDisable(){
            DestroyImmediate(mesh);
        }

        private void SampleLink(CableJoint prevJoint, Cable.Link link, CableJoint nextJoint)
        {

            Vector3? t1 = null, t2 = null;

            if (prevJoint != null)
                t1 = prevJoint.link2.body.WorldToCable(prevJoint.WorldSpaceAttachment2);
            if (nextJoint != null)
                t2 = nextJoint.link1.body.WorldToCable(nextJoint.WorldSpaceAttachment1);

            // Hybrid links (only at the start or the end of the cable)
            if (link.type == Cable.Link.LinkType.Hybrid)
            {
                float spacing = edgeLoopSpacing;
                if (t1.HasValue)
                {
                    link.body.AppendSamples(sampledCable, t1.Value, spacing, link.storedCable, link.spoolSeparation, false, link.orientation);
                }
                else if (t2.HasValue)
                {
                    link.body.AppendSamples(sampledCable, t2.Value, spacing, link.storedCable, link.spoolSeparation, true, link.orientation);
                }
            }
            // Rolling links (only mid-cable)
            else if (link.type == Cable.Link.LinkType.Rolling)
            {
                if (t1.HasValue && t2.HasValue)
                {
                    float distance = link.body.SurfaceDistance(t1.Value, t2.Value, !link.orientation, false);
                    link.body.AppendSamples(sampledCable, t1.Value, edgeLoopSpacing, distance, 0, false, link.orientation);
                }
            }
            // Attachment, source and pinhole links:        
            else
            {

                if (t1.HasValue)
                    sampledCable.AppendSample(prevJoint.link2.body.transform.TransformPoint(link.inAnchor));

                if (t1.HasValue && t2.HasValue && t1.Value != t2.Value)
                    sampledCable.NewSegment();

                if (t2.HasValue)
                    sampledCable.AppendSample(nextJoint.link1.body.transform.TransformPoint(link.outAnchor));
            }
        }

        private void SampleJoint(CableJoint joint)
        {
            Vector3 p1 = joint.WorldSpaceAttachment1;
            Vector3 p2 = joint.WorldSpaceAttachment2;
            int samples;

            if (joint.length < joint.restLength && loosenessScale > 0)
            {
                float sampledLength = Mathf.Lerp(joint.length, Mathf.Min(joint.restLength, joint.length + maxLooseCable), loosenessScale);
                samples = Mathf.Min((int)(sampledLength / Mathf.Max(edgeLoopSpacing, 0.01f)), maxEdgeLoopsPerLooseJoint);

                Vector3 dir = p2 - p1;
                if (dir.x * dir.x + dir.z * dir.z > verticalThreshold)
                {
                    if (Utils.Catenary(p1, p2, sampledLength, samples, samplesBuffer))
                    {
                        for (int j = 1; j < samples - 1; ++j)
                            sampledCable.AppendSample(samplesBuffer[j]);
                        return;
                    }
                }
                else
                {
                    if (Utils.Sinusoid(p1, p2 - p1, sampledLength, verticalCurlyness, samples, samplesBuffer))
                    {
                        for (int j = 1; j < samples - 1; ++j)
                            sampledCable.AppendSample(samplesBuffer[j]);
                        return;
                    }
                }
            }

            samples = Mathf.Min((int)(joint.restLength / Mathf.Max(edgeLoopSpacing, 0.01f)), maxEdgeLoopsPerTautJoint);
            for (int j = 1; j < samples - 1; ++j)
                sampledCable.AppendSample(Vector3.Lerp(p1, p2, j / (float)(samples - 1)));

        }

        public void SampleCable()
        {
            sampledCable.Clear();
            sampledCable.transform = transform.worldToLocalMatrix;

            var joints = cable.Joints;

            if (joints == null || cable.links.Count == 0)
                return;

            bool closed = (cable.links[0].body == cable.links[cable.links.Count - 1].body);

            for (int i = 0; i < cable.links.Count; ++i)
            {

                if (cable.links[i].body != null)
                {

                    CableJoint prevJoint = cable.GetPreviousJoint(i, closed);
                    CableJoint nextJoint = cable.GetNextJoint(i, closed);

                    // Sample the link, except if the cable is closed and this is the first link.
                    if (!(i == 0 && closed) || cable.links[i].type == Cable.Link.LinkType.Attachment || cable.links[i].type == Cable.Link.LinkType.Pinhole)
                        SampleLink(prevJoint, cable.links[i], nextJoint);

                    // Sample the joint (only adds sample points if cable is not tense):
                    if (i < joints.Count && joints[i] != null)
                        SampleJoint(joints[i]);
                }

            }

            if (closed)
            {
                sampledCable.Close();
            }
        }

        public void LateUpdate()
        {
            ClearMeshData();

            if (section == null)
                return;

            SampleCable();

            IList<List<Vector3>> segments = sampledCable.Segments;

            int sectionSegments = section.Segments;
            int verticesPerSection = sectionSegments + 1;  // the last vertex in each section must be duplicated, due to uv wraparound.

            float vCoord = -uvScale.y * cable.RestLength;  // v texture coordinate.
            int sectionIndex = 0;

            float strain = sampledCable.Length / cable.RestLength;

            // we will define and transport a reference frame along the curve using parallel transport method:
            if (frame == null)          
                frame = new CurveFrame();

            Vector4 texTangent = Vector4.zero;
            Vector2 uv = Vector2.zero;

            for (int k = 0; k < segments.Count; ++k)
            {
                List<Vector3> samples = segments[k];

                // Reinitialize frame for each segment.
                frame.Reset();

                for (int i = 0; i < samples.Count; ++i)
                {
                    // Calculate previous and next curve indices:
                    int nextIndex = Mathf.Min(i+1,samples.Count-1);
                    int prevIndex = Mathf.Max(i-1,0);
    
                    Vector3 nextV = (samples[nextIndex] - samples[i]).normalized;
                    Vector3 prevV = (samples[i] - samples[prevIndex]).normalized;
                    Vector3 tangent = nextV + prevV;
    
                    // update frame:
                    frame.Transport(samples[i], tangent);
        
                    // advance v texcoord:
                    vCoord += uvScale.y * (Vector3.Distance(samples[i], samples[prevIndex]) /  strain);
    
                    // Loop around each segment:
                    for (int j = 0; j <= sectionSegments; ++j){

                        Vector3 norm = (section.vertices[j].x * frame.normal + section.vertices[j].y * frame.binormal) * thickness;
                        vertices.Add(frame.position + norm);
                        normals.Add(norm);
                        texTangent = -Vector3.Cross(norm, frame.tangent);
                        texTangent.w = 1;
                        tangents.Add(texTangent);
    
                        uv.Set(j / (float)sectionSegments * uvScale.x,vCoord);
                        uvs.Add(uv);
    
                        if (j < sectionSegments && i < samples.Count-1){
    
                            tris.Add(sectionIndex*verticesPerSection + j);          
                            tris.Add(sectionIndex*verticesPerSection + (j+1));      
                            tris.Add((sectionIndex+1)*verticesPerSection + j);      
    
                            tris.Add(sectionIndex*verticesPerSection + (j+1));      
                            tris.Add((sectionIndex+1)*verticesPerSection + (j+1));  
                            tris.Add((sectionIndex+1)*verticesPerSection + j);      
    
                        }
                    }
    
                    sectionIndex++;
                }
            }

            CommitMeshData();
        }

        private void ClearMeshData(){
            mesh.Clear();
            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            uvs.Clear();
            tris.Clear();
        }

        private void CommitMeshData(){
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0,uvs);
            mesh.SetTriangles(tris,0,true);
        }
    }
}


