using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{

    [SerializeField] public GeneralizationAgent agent;
    [SerializeField] public GameObject environment;
    [SerializeField] public GameObject goal;
    [SerializeField] public float carLength;
    [SerializeField] public float carWidth;

    //Variable to set the configuration in training mode or model checking mode
    public bool isTraining;
    
    //Declaring a variable for the number of iteration we wants to check the model
    public int iteration;
    public int maxIteration; 

    //Garage transform list
    [SerializeField] public GameObject obstacle;

    //Flag that becomes true when the number of iteration has reached maxIteration
    public bool isOver = false;

    public int cellWidth;

    void Update()
    {
   
    }

    /*
    //Print ratio on GUI
    private void OnGUI()
    {
        GUI.Label(new Rect(10,10,100,100),"Ratio= " + ratio.ToString("F"));
    }*/

    int count = 0;
    public void RandomAgentPositioning()
    {
        if (agent != null && isOver == false)
        {
            float maxX = 0, maxZ = 0;
            float rotation = Random.Range(0,360);
            float x_base;
            float z_base;
            Transform max;
            
            max = GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>();
            maxX = max.localScale.x / 2 - carLength;            
            maxZ = maxX;   

            //X coordinate for possible available position randomized after the rotation of the vehicle
            x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));

            //Z coordinate for possible available position randomized after the rotation of the vehicle
            z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));

            //Setting properties
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            agent.GetComponent<CarController>().CurrentSteeringAngle = 0f;
            agent.GetComponent<CarController>().CurrentAcceleration = 0f;
            agent.GetComponent<CarController>().CurrentBrakeTorque = 0f;

            //Setting random rotation
            agent.transform.rotation = Quaternion.Euler(0, rotation, 0);            

            //Setting random position
            agent.transform.position = new Vector3(x_base, 1, z_base);            

            Debug.Log("RandomAgentPositioning DENTRO");

            count++;
            Debug.Log("count: " + count + " x_base: " + x_base + " z_base: " + z_base + " agent_x: " + agent.transform.position.x + " agent_z: " + agent.transform.position.z);
        }
        Debug.Log("RandomAgentPositioning FUORI");
    }

    public void RepositionTargetRandom()
    {
        int i = Random.Range(0, 22);

        //int i = 2;

        //Set the goal rotation -90 garage above 90 garage below
        if (i >= 0 && i <= 4)  
        {
            goal.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            goal.transform.rotation = Quaternion.Euler(0, -90, 0);
        }   
    }

    public void RandomObstaclesPositioning()
    {
        for (int i = 0; i <= cellWidth; i++)
        {
            for (int j = 0; j <= cellWidth; j++)
            {
                Vector3 pos = new Vector3(Random.Range(1,10),0,0);
                float rot = Random.Range(0,360);
            }
        }
    }

}
