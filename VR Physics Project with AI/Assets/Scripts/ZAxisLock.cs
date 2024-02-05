using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written to lock the Y and Z axes of an object, allowing movement only along the X-axis

public class XAxisLock : MonoBehaviour
{
    private float initialY;
    private float initialZ;

    void Start()
    {
        // Store the initial Y and Z positions
        initialY = transform.position.y;
        initialZ = transform.position.z;
    }

    void Update()
    {
        // Lock the Y and Z positions to their initial values, allowing movement only along the X-axis
        transform.position = new Vector3(transform.position.x, initialY, initialZ);
    }
}
