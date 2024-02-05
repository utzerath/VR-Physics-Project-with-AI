using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//written for locking the y axis of an object

public class YAxisLock : MonoBehaviour
{
    private float initialX;
    private float initialZ;

    void Start()
    {
        // Store the initial X and Z positions
        initialX = transform.position.x;
        initialZ = transform.position.z;
    }

    void Update()
    {
        // Lock the X and Z positions to their initial values
        transform.position = new Vector3(initialX, transform.position.y, initialZ);
    }
}

