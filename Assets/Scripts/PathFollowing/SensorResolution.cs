using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorResolution : MonoBehaviour
{
    [SerializeField] float targetResolution;
    [SerializeField] float distance;
    public float numberRays;
    public float angle;
    public float maxDegree;
    


    // Update is called once per frame
    void OnValidate()
    {
        angle = Mathf.Rad2Deg * Mathf.Asin(targetResolution / distance);
        maxDegree = 180 - angle / 2;
        numberRays = Mathf.Ceil(maxDegree / angle);
    }
}
