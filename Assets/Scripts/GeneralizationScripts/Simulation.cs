using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Simulation : MonoBehaviour
{
    //[SerializeField] public List<Target> target;
    [SerializeField] public GeneralizationAgent agent;
    [SerializeField] public ConfigManager configManager;
    [SerializeField] public DrawCheckpoint drawCheckpoint;
    [SerializeField] public MakeCheckpoints makeCheckpoint; 


    private bool _initComplete = false;

    public bool InitComplete => _initComplete;

    /*
    public void RepositionAgentRandom()
    {
        if (agent != null)
        {
            //Call the function for the generation of the random spot position
            newRandPosition = SelectRandomSpawn();
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            agent.GetComponent<CarController>().CurrentSteeringAngle = 0f;
            agent.GetComponent<CarController>().CurrentAcceleration = 0f;
            agent.GetComponent<CarController>().CurrentBrakeTorque = 0f;
            agent.transform.rotation = Quaternion.Euler(0, Random.Range(0,360), 0);
            agent.transform.position = new Vector3(newRandPosition.x, newRandPosition.y,newRandPosition.z);
            //agent.transform.position = transform.parent.position + new Vector3(, , );
        }
    }*/


    public void InitializeSimulation()
    {
        _initComplete = false;
        
        drawCheckpoint.ResetCheckpoints();
        makeCheckpoint.ResetObjectToSpawn();
        configManager.RandomObstaclesPositioning();
        configManager.RandomAgentPositioning();
        configManager.RepositionTargetRandom();

        _initComplete = true;
    }

    public void ResetSimulation()
    {
        
    }

}