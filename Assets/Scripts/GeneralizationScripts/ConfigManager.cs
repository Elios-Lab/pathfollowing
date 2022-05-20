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

    //Flag that becomes true when the number of iteration has reached maxIteration
    public bool isOver = false;

    public int cellWidth;

    public enum EnvironmentComplexity {BASIC=1, ENTRY=2, MEDIUM=3, ADVANCED=4, EXTREME=5};
    public EnvironmentComplexity environmentComplexity = EnvironmentComplexity.BASIC;

    public GameObject obstacleToSpawn;

    Transform max;
    float maxX = 0, maxZ = 0;    

    void Start() 
    {
            max = GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>();
            maxX = max.localScale.x / 2 - carLength;            
            maxZ = maxX; 

            RandomObstaclesPositioning();
    }
    
    void Update()
    {
   
    }
    
    public void RandomAgentPositioning()
    {
        float rotation;
        float x_base;
        float z_base;

        if (agent != null && isOver == false)
        {           
            switch(environmentComplexity)
            {
                case EnvironmentComplexity.BASIC:
                    x_base = 0f;
                    z_base = 0f;
                    rotation = 0f;
                    break;
                case EnvironmentComplexity.ENTRY:
                    x_base = Random.Range(-Mathf.Floor(maxZ) / 3, Mathf.Floor(maxZ) / 3);
                    z_base = Random.Range(-Mathf.Floor(maxZ) / 3, Mathf.Floor(maxZ) / 3);
                    rotation = Random.Range(0,360);
                    break;
                case EnvironmentComplexity.MEDIUM:
                    x_base = 0f;
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0,360);
                    break;
                default:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));                     
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0,360);
                    break;
            }
            

            //Setting properties
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            agent.GetComponent<CarController>().CurrentSteeringAngle = 0f;
            agent.GetComponent<CarController>().CurrentAcceleration = 0f;
            agent.GetComponent<CarController>().CurrentBrakeTorque = 0f;

            //Setting agent rotation
            agent.transform.rotation = Quaternion.Euler(0, rotation, 0);            

            //Setting agent position              
            agent.transform.localPosition = new Vector3(x_base, 1, z_base);             
        }
    }

    public void RepositionTargetRandom()
    {
        float x_base;
        float z_base; 
        float distanceX, distanceZ;
        float deltaTargetAgent = 5 * carLength;

        switch(environmentComplexity)
        {
            case EnvironmentComplexity.BASIC:
                x_base = 0;
                z_base = 2 * carLength;
                break;
            case EnvironmentComplexity.ENTRY:
                x_base = Random.Range(-Mathf.Floor(maxZ) / 3, Mathf.Floor(maxZ) / 3);
                z_base = Random.Range(-Mathf.Floor(maxZ) / 3, Mathf.Floor(maxZ) / 3);
                break;
            case EnvironmentComplexity.MEDIUM:
                x_base = 0;
                z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                break;
            default:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                break;
        }        

        // To avoid target too near the agent
        if(environmentComplexity != EnvironmentComplexity.BASIC)
        {
            distanceX = x_base - agent.transform.position.x;
            distanceZ = z_base - agent.transform.position.z;

            
            if(Mathf.Abs(distanceX) < deltaTargetAgent)
            {                
                if(distanceX < 0)
                {
                    x_base += (deltaTargetAgent - distanceX);
                    Debug.Log("Minore lungo X negativo");
                }
                else
                {
                    x_base -= (-deltaTargetAgent + distanceX);
                    Debug.Log("Minore lungo X positivo");
                }
            }

            if(Mathf.Abs(distanceZ) < deltaTargetAgent)
            {                
                if(distanceZ < 0)
                {
                    z_base += (deltaTargetAgent - distanceZ);
                    Debug.Log("Minore lungo Z negativo");
                }
                else
                {
                    z_base -= (-deltaTargetAgent + distanceZ);
                    Debug.Log("Minore lungo Z positivo");
                }
            }
        }

        //Setting target position              
        goal.transform.localPosition = new Vector3(x_base, 1, z_base);

        Debug.Log("TargetX: " + goal.transform.position.x + "TargetZ: " + goal.transform.position.z);
        Debug.Log("PREAgentX: " + agent.transform.position.x + "PREAgentZ: " + agent.transform.position.z); 
    }

    public void RandomObstaclesPositioning()
    {
        int numberOfObstacle;
        float x_base = 0;
        float z_base = 0;

        switch(environmentComplexity)
        {
            case EnvironmentComplexity.BASIC:
                numberOfObstacle = 0;                
                break;
            case EnvironmentComplexity.ENTRY:
                numberOfObstacle = 0;
                break;
            case EnvironmentComplexity.MEDIUM:
                numberOfObstacle = 1;
                x_base = 0;
                z_base = 2 * carLength;
                obstacleToSpawn.GetComponent<ObstacleMovement>().speed = 0;
                break;
            case EnvironmentComplexity.ADVANCED:
                numberOfObstacle = 0;
                break;
            case EnvironmentComplexity.EXTREME:
                numberOfObstacle = 0;
                break;
            default:
                numberOfObstacle = 0;
                break;
        }

        obstacleToSpawn.transform.localPosition = new Vector3(x_base + environment.transform.position.x, 
                                                    GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>().position.y , 
                                                    z_base + environment.transform.position.z);

        for (int i=0; i<numberOfObstacle; i++)
            Instantiate(obstacleToSpawn, obstacleToSpawn.transform.localPosition, Quaternion.Euler(0, 0, 0), environment.transform);
    }

}
