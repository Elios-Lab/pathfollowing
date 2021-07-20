using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehavior : MonoBehaviour
{
    //CarController1.js
    public float enginePower = 150.0f;
    public float steerPower = 25f;
    public WheelCollider[] wheels;

    public GameObject centerOfMass;
    public Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = centerOfMass.transform.localPosition;
    }

    void Update()
    {
        for(int i = 0; i < wheels.Length; i++)
        {
            if (i < 2)
            {
                wheels[i].motorTorque = Input.GetAxis("Vertical") * enginePower;
                wheels[i].steerAngle = Input.GetAxis("Horizontal") * steerPower;
            }
        }
    }
}
