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

    public enum EnvironmentComplexity {BASIC=1, ENTRY=2, MEDIUM=3, ADVANCED=4, EXTREME=5, ROOM1=6, ROOM2=7, ROOM3=8, ROOM4=9};
    public EnvironmentComplexity environmentComplexity = EnvironmentComplexity.BASIC;

    public GameObject obstacleToSpawn, staticObstacleToSpawn, staticAlignedObstacleToSpawn;

    Transform max;
    float maxX = 0, maxZ = 0;
    float offsetAlignedObstacle = 0;

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
                    x_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0,360);
                    break;
                case EnvironmentComplexity.MEDIUM:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), -Mathf.Floor(maxZ) * 5 / 6);
                    rotation = Random.Range(-45, 45);
                    break;
                case EnvironmentComplexity.ROOM1:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), -Mathf.Floor(maxZ) * 5/6);
                    rotation = Random.Range(-45, 45);
                    break;
                case EnvironmentComplexity.ROOM2:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), -Mathf.Floor(maxZ) * 5/6);
                    rotation = Random.Range(-45, 45);
                    break;
                case EnvironmentComplexity.ROOM3:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), -Mathf.Floor(maxZ) * 5/6);
                    rotation = Random.Range(-45, 45);
                    break;
                case EnvironmentComplexity.ROOM4:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), -Mathf.Floor(maxZ) * 5/6);
                    rotation = Random.Range(-45, 45);
                    break;
                default:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));                     
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0, 360);
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
        float deltaTargetAgent = 2 * carLength;

        switch(environmentComplexity)
        {
            case EnvironmentComplexity.BASIC:
                x_base = 0;
                z_base = 2 * carLength;
                break;
            case EnvironmentComplexity.ENTRY:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                break;
            case EnvironmentComplexity.MEDIUM:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(Mathf.Floor(maxZ) * 5 / 6, Mathf.Floor(maxZ));
                break;
            case EnvironmentComplexity.ROOM1:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(Mathf.Floor(maxZ) * 5/6, Mathf.Floor(maxZ));
                break;
            case EnvironmentComplexity.ROOM2:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(Mathf.Floor(maxZ) * 5/6, Mathf.Floor(maxZ));
                break;
            case EnvironmentComplexity.ROOM3:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(Mathf.Floor(maxZ) * 5/6, Mathf.Floor(maxZ));
                break;
            case EnvironmentComplexity.ROOM4:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(Mathf.Floor(maxZ) * 5/6, Mathf.Floor(maxZ));
                break;
            default:
                x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                break;
        }        

        // To avoid target too near the agent
        if(environmentComplexity == EnvironmentComplexity.ENTRY)
        {
            distanceX = x_base - agent.transform.position.x;
            distanceZ = z_base - agent.transform.position.z;

            
            if(Mathf.Abs(distanceX) < deltaTargetAgent)
            {                
                if(distanceX > 0)
                {
                    x_base += (deltaTargetAgent - distanceX);
                    if (x_base >= maxX)
                        x_base -= 1;

                    Debug.Log("Minore X positivo");
                }
                else
                {
                    x_base -= (deltaTargetAgent - Mathf.Abs(distanceX));
                    if (x_base <= -maxX)
                        x_base += 1;

                    Debug.Log("Minore X negativo");
                }
            }

            if (Mathf.Abs(distanceZ) < deltaTargetAgent)
            {
                if (distanceZ > 0)
                {
                    z_base += (deltaTargetAgent - distanceZ);
                    if (z_base >= maxZ)
                        z_base -= 1;

                    Debug.Log("Minore Z positivo");
                }
                else
                {
                    z_base -= (deltaTargetAgent - Mathf.Abs(distanceZ));
                    if (z_base <= -maxZ)
                        z_base += 1;

                    Debug.Log("Minore Z negativo");
                }
            }
        }

        //Setting target position              
        goal.transform.localPosition = new Vector3(x_base, 1, z_base);

        //Debug.Log("TargetX: " + goal.transform.position.x + "TargetZ: " + goal.transform.position.z);
        //Debug.Log("PREAgentX: " + agent.transform.position.x + "PREAgentZ: " + agent.transform.position.z); 
    }

    public void RandomObstaclesPositioning()
    {
        int numberOfDynamicObstacle, numberOfAlignedObstacle;
        float x_base = 0;
        float z_base = 0;
        float rotation = 0;

        switch(environmentComplexity)
        {
            case EnvironmentComplexity.BASIC:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 0;
                break;
            case EnvironmentComplexity.ENTRY:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 0;
                break;
            case EnvironmentComplexity.MEDIUM:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 1;               
                break;
            case EnvironmentComplexity.ADVANCED:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 1;
                break;
            case EnvironmentComplexity.EXTREME:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 1;
                break;
            case EnvironmentComplexity.ROOM1:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 1;
                break;
            case EnvironmentComplexity.ROOM2:
                numberOfDynamicObstacle = 1;
                numberOfAlignedObstacle = 1;
                StaticObstaclePositioning();
                break;
            case EnvironmentComplexity.ROOM3:
                numberOfDynamicObstacle = 4;
                numberOfAlignedObstacle = 1;
                obstacleToSpawn.GetComponent<ObstacleMovement>().speed = 2;
                StaticObstaclePositioning();
                break;
            case EnvironmentComplexity.ROOM4:
                numberOfDynamicObstacle = 8;
                numberOfAlignedObstacle = 1;
                obstacleToSpawn.GetComponent<ObstacleMovement>().speed = 2;
                StaticObstaclePositioning();
                break;
            default:
                numberOfDynamicObstacle = 0;
                numberOfAlignedObstacle = 0;
                break;
        }

        /* obstacleToSpawn.transform.localPosition = new Vector3(x_base + environment.transform.position.x, 
                                                    GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>().position.y , 
                                                    z_base + environment.transform.position.z); */

        for (int i=0; i<numberOfDynamicObstacle; i++)
        {
            x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
            z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
            rotation = Random.Range(0, 360);
            obstacleToSpawn.transform.localPosition = new Vector3(x_base + environment.transform.position.x, 
                                                    GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>().position.y , 
                                                    z_base + environment.transform.position.z);
            Instantiate(obstacleToSpawn, obstacleToSpawn.transform.localPosition, Quaternion.Euler(0, rotation, 0), environment.transform);
        }

        for (int i = 0; i < numberOfAlignedObstacle; i++)
        {
            Vector3 center = ((goal.transform.position + agent.transform.position) * 0.5f);            
            Instantiate(staticAlignedObstacleToSpawn, center, Quaternion.Euler(0, rotation, 0), environment.transform);
        }

    }

    public void StaticObstaclePositioning()
    {
        float x_base = 0;
        float z_base = 0;

        // first static obstacle
        x_base = -max.localScale.x / 2 + staticObstacleToSpawn.transform.localScale.x / 2;
        z_base = -(max.localScale.x / 2 - max.localScale.x / 3);
        staticObstacleToSpawn.transform.localPosition = new Vector3(x_base + environment.transform.position.x, 
                                            GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>().position.y, 
                                            z_base + environment.transform.position.z); 
        Instantiate(staticObstacleToSpawn, staticObstacleToSpawn.transform.localPosition, Quaternion.Euler(0, 0, 0), environment.transform);   
        
        // second static obstacle
        x_base = max.localScale.x / 2 - staticObstacleToSpawn.transform.localScale.x / 2;
        z_base = max.localScale.x / 2 - max.localScale.x / 3;
        staticObstacleToSpawn.transform.localPosition = new Vector3(x_base + environment.transform.position.x, 
                                            GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>().position.y, 
                                            z_base + environment.transform.position.z); 
        Instantiate(staticObstacleToSpawn, staticObstacleToSpawn.transform.localPosition, Quaternion.Euler(0, 0, 0), environment.transform);   
        
    }

    public float MaxLength()
    {
        return GameObject.FindGameObjectWithTag("barrier").transform.localScale.x;
    }

    public void RandomizeObstacleAttribute()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("obstacle");
        GameObject[] obstaclesAligned = GameObject.FindGameObjectsWithTag("obstacleAligned");

        foreach (GameObject obstacle in obstacles)
        {
            obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            obstacleToSpawn.GetComponent<ObstacleMovement>().speed = Random.Range(0.5f, 3);
            //Debug.Log("randObstacle");
        }

        foreach (GameObject obstacle in obstaclesAligned)
        {
            Vector3 targetPosition = GameObject.FindGameObjectWithTag("Finish").transform.localPosition;
            Vector3 agentPosition = GameObject.FindGameObjectWithTag("agent").transform.localPosition;
            Vector3 center = (targetPosition + agentPosition) * 0.5f;
            offsetAlignedObstacle = Random.Range(-25, 25);            
            float angle = Mathf.Atan( Mathf.Abs( (targetPosition.z - agentPosition.z) / (targetPosition.x - agentPosition.x) ) );
            //center.x = center.x + Mathf.Cos(angle) * offsetAlignedObstacle;
            //center.z = center.z + Mathf.Sin(angle) * offsetAlignedObstacle;


            obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            obstacle.transform.localPosition = center;
            //Debug.Log("Finish: x=" + targetPosition.x + " z=" + targetPosition.z);
            //Debug.Log("Agent: x=" + agentPosition.x + " z=" + agentPosition.z);
            //Debug.Log("Center: x=" + center.x + " z=" + center.z);
            //Debug.Log("Angle: " + angle + " rotation: " + rotation);
        }
    }

}
