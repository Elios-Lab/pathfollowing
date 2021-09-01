using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimulationManagerPFollowing : MonoBehaviour
{
    [SerializeField] private PathFollowingAgent agent;
    [SerializeField] public GameObject target;

    private bool _initComplete = false;

    public bool InitComplete => _initComplete;

    public void RepositionAgentRandom()
    {
        if (agent != null)
        {
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            agent.GetComponent<CarController>().CurrentSteeringAngle = 0f;
            agent.GetComponent<CarController>().CurrentAcceleration = 0f;
            agent.GetComponent<CarController>().CurrentBrakeTorque = 0f;
            agent.transform.rotation = Quaternion.Euler(0, -90, 0);
            agent.transform.position = transform.parent.position + new Vector3(44.3f, 0.05999994f, -35.2f);
        }
    }

    //al momento la posizione è fixata
    public void RepositionTargetRandom()
    {
        //target.transform.rotation = Quaternion.Euler(0, 45, 0);
        //target.transform.position = transform.parent.position + new Vector3(13.18f, 1.167712f, -37.79f);
    }

    public void InitializeSimulation()
    {
        _initComplete = false;
        RepositionAgentRandom();
        RepositionTargetRandom();


        _initComplete = true;
    }

    public void ResetSimulation()
    {
        
    }
}
