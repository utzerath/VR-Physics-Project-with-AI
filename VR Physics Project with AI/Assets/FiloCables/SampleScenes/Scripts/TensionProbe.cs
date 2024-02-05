using System;
using System.Collections.Generic;
using UnityEngine;
using Filo;

[RequireComponent(typeof(Cable))]
[RequireComponent(typeof(CableRenderer))]
public class TensionProbe : MonoBehaviour
{
    CableRenderer cableRenderer;
    Cable cable;
    public float tension = 0;
    public float cableTension = 0;

    public void Awake()
    {
        cableRenderer = GetComponent<CableRenderer>();
        cable = GetComponent<Cable>();
    }

    public void Update(){
        tension = cableRenderer.sampledCable.Length / cable.RestLength;

        IList<CableJoint> joints = cable.Joints;
        foreach (CableJoint j in joints){
            float force = j.ImpulseMagnitude / Time.fixedDeltaTime;
            Debug.Log(force + " N, Mass:" + force/-9.81f +" Kg");
        }
    } 
}


