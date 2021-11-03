using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamResolution : MonoBehaviour
{
    //Check the position of the camera first
    [SerializeField] float cameraDistance;

    //Assuming is a square, I give the length of one side
    [SerializeField] float imageDimension;
    
    public float FieldOfView;

    private void OnValidate()
    {
        FieldOfView = (Mathf.Rad2Deg * Mathf.Atan((imageDimension / 2) / cameraDistance)) * 2;
    }

}
