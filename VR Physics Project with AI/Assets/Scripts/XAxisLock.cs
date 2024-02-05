using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written to lock the X and Y axes of an object, allowing movement only along the Z-axis

public class ZAxisLock : MonoBehaviour
{
    private float initialX;
    private float initialY;

    void Start()
    {
        // Store the initial X and Y positions
        initialX = transform.position.x;
        initialY = transform.position.y;
    }

    void Update()
    {
        // Lock the X and Y positions to their initial values, allowing movement only along the Z-axis
        transform.position = new Vector3(initialX, initialY, transform.position.z);
    }
}
