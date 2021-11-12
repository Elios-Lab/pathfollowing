using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour
{

    [SerializeField] public PathFollowingAgent agent;
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
    [SerializeField] public Transform[] garage;

    //Flag that becomes true when the number of iteration has reached maxIteration
    public bool isOver = false;

    //Numero di collisioni
    private int colNumber = 0;
    //Numero di goal 
    private int goalNumber = 0;
    //Numero di volte in cui l'episodio termina senza goal/collisione
    private int timeoutNum = 0;

    //Var to save the percentage of goals reached over the number of iterations
    private float ratio = 0;

    void Update()
    {
   
    }

    //Print ratio on GUI
    private void OnGUI()
    {
        GUI.Label(new Rect(10,10,100,100),"Ratio= " + ratio.ToString("F"));
    }

    public void IncrementCollision()
    {       
        colNumber++;
        Debug.Log("Collision number" + colNumber);
        Debug.Log("Success rate: " + ratio);
    }

    public void IncrementGoal() 
    {
        goalNumber++;
        Debug.Log("Goal number: " + goalNumber);
        Debug.Log("Success rate: " + ratio);
    }

    public void IncrementTimeout() 
    {
        timeoutNum++;
        Debug.Log("Timeouts: " + timeoutNum);
        Debug.Log("Success rate: " + ratio);
    }

    public void RandomAgentPositioning()
    {
        if (agent != null && isOver == false)
        {
            //Call the function for the generation of the random spot position
            float maxX = 0, maxZ = 0, minX = 0, minZ = 0;

            float rotation = Random.Range(0,360);

            float x_base;
            float z_base;

            maxX = 9.75f- (carWidth / 2) * Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * rotation)) - (carLength / 2) * Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * rotation));
            maxZ = 3.75f - (carWidth / 2) * Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * rotation)) - (carLength / 2) * Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * rotation));
            minX = -maxX;
            minZ = -maxZ;

            //X coordinate for possible available position randomized after the rotation of the vehicle
            x_base = Random.Range(0f, Mathf.Floor(maxX));

            //Z coordinate for possible available position randomized after the rotation of the vehicle
            z_base = Random.Range(0f, Mathf.Floor(maxZ));

            //Creating a random boolean... 
            if (Random.Range(0, 2) == 1)
                x_base = -x_base;

            if (Random.Range(0, 2) == 1)
                z_base = -z_base;

            //Setting properties
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            agent.GetComponent<CarController>().CurrentSteeringAngle = 0f;
            agent.GetComponent<CarController>().CurrentAcceleration = 0f;
            agent.GetComponent<CarController>().CurrentBrakeTorque = 0f;

            //Setting random rotation
            agent.transform.rotation = Quaternion.Euler(0, rotation, 0);

            //Setting random position
            agent.transform.position = transform.parent.position + new Vector3(x_base, 0.5f, z_base);

        }
    }

    public void RepositionTargetRandom()
    {
        //int i = Random.Range(0, garage.Length);

        int i = 2;

        //Set the goal rotation -90 garage above 90 garage below
        if (i >= 0 && i <= 4)  
        {
            goal.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            goal.transform.rotation = Quaternion.Euler(0, -90, 0);
        }   

        //Set the goal position to the TargetSpawn(child object of garage) position
        goal.transform.position = garage[i].GetChild(4).position;

        //Opens the randomly chosen garage and close the others
        for(int j = 0; j < garage.Length; j++)
        {
            Vector3 position = garage[j].GetChild(3).position;
            Quaternion rotation = garage[j].GetChild(3).rotation;

            if (j == i)
            {
                garage[j].GetChild(3).transform.SetPositionAndRotation(new Vector3(position.x, -5, position.z), rotation);
            }
            else
            {
                garage[j].GetChild(3).transform.SetPositionAndRotation(new Vector3(position.x, 1, position.z), rotation);
            }
        }

    }

}
