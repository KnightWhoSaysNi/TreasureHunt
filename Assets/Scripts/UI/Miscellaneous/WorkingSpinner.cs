using UnityEngine;
using System.Collections;
using System;

public class WorkingSpinner : MonoBehaviour
{
    public float rotationSpeed = -250;

    void Update()
    {
        this.transform.Rotate(0, 0, Time.deltaTime * rotationSpeed);
    }
}