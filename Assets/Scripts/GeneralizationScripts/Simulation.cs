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
        Time.timeScale = 20;

        configManager.RandomAgentPositioning();
        configManager.RepositionTargetRandom();
        configManager.RandomizeObstacleAttribute();
        _initComplete = true;

        //Debug.Log("AgentX: " + agent.transform.position.x + "AgentZ: " + agent.transform.position.z);        
    }

    public void ResetSimulation()
    {
        
    }

}