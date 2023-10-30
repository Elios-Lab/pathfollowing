using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class ConfigManager : MonoBehaviour
{

    [SerializeField] public GeneralizationAgent agent;
    [SerializeField] public GameObject environment;
    [SerializeField] public GameObject goal;  
    [SerializeField] public ConfigDeserializer parameters;
    [SerializeField] public GameObject carModel;

    //Variable to set the configuration in training mode or model checking mode
    public bool isTraining;
    
    //Declaring a variable for the number of iteration we wants to check the model
    public int iteration;
    public int maxIteration;    

    //Flag that becomes true when the number of iteration has reached maxIteration
    public bool isOver = false;
    public class ConfigDeserializer
    {
        public Car car;
    }
    [System.Serializable]
    public class Car
    {
        public float length;
        public float width;
        public float height;
        public float weight;
        public float steering;
        public float torque;
        public float brake;

    }
    private Vector3 carSize = new Vector3(1.788832f, 4.705657f, 0.9729889f);
    private Vector3 carScale = new Vector3(100, 100, 100);
    public enum EnvironmentComplexity {BASIC=1, ENTRY=2, MEDIUM=3, ADVANCED=4, EXTREME=5, ROOM1=6, ROOM2=7, ROOM3=8, ROOM4=9};
    public EnvironmentComplexity environmentComplexity = EnvironmentComplexity.BASIC;

    public GameObject obstacleToSpawn, staticObstacleToSpawn, staticAlignedObstacleToSpawn;

    Transform max;
    float maxX = 0, maxZ = 0;
    // float offsetAlignedObstacle = 0;

    private void Awake()
    {
        parameters = JsonUtility.FromJson<ConfigDeserializer>(ReadJSON());
        ScaleCar();
        SetCarParameters();
    }

    void Start() 
    {
            max = GameObject.FindWithTag("barrier").GetComponentInChildren<Transform>();
            maxX = max.localScale.x / 2 - parameters.car.length;            
            maxZ = maxX;

            RandomObstaclesPositioning();  
    }
    
    void Update()
    {
   
    }

    void ScaleCar()
    // Scale the car and its collider according to the JSON file specifics
    {
        // Proportion is carSize : carScale = newSize : newScale
        Vector3 newScale = new Vector3(
            carScale.x * parameters.car.width / carSize.x,
            carScale.y * parameters.car.length / carSize.y,
            carScale.z * parameters.car.height / carSize.z);

        carModel.GetComponent<BoxCollider>().size = new Vector3(parameters.car.width / newScale.x, parameters.car.length / newScale.y, parameters.car.height / newScale.z);
        carModel.transform.localScale = newScale;
        return;
    }

    void SetCarParameters()
    // Set the car's parameters according to the JSON file specifics
    {
        agent.GetComponent<Rigidbody>().mass = parameters.car.weight;
        agent.GetComponent<CarController>().maxSteeringAngle = parameters.car.steering;
        agent.GetComponent<CarController>().maxMotorTorque = parameters.car.torque;
        agent.GetComponent<CarController>().maxBrakeTorque = parameters.car.brake;
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
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0,360);
                    break;
                case EnvironmentComplexity.MEDIUM:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0, 360);
                    break;
                case EnvironmentComplexity.ADVANCED:
                    x_base = Random.Range(-Mathf.Floor(maxX), Mathf.Floor(maxX));
                    z_base = Random.Range(-Mathf.Floor(maxZ), Mathf.Floor(maxZ));
                    rotation = Random.Range(0, 360);
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
        float deltaTargetAgent = 5 * parameters.car.length;

        switch(environmentComplexity)
        {
            case EnvironmentComplexity.BASIC:
                x_base = 0;
                z_base = 2 * parameters.car.length;
                break;
            case EnvironmentComplexity.ENTRY:
                x_base = Random.Range(-Mathf.Floor(maxX) * 7/10, Mathf.Floor(maxX) * 7/10);
                z_base = Random.Range(-Mathf.Floor(maxZ) * 7/10, Mathf.Floor(maxZ) * 7/10);
                break;
            case EnvironmentComplexity.MEDIUM:
                x_base = Random.Range(-Mathf.Floor(maxX) * 7/10, Mathf.Floor(maxX) * 7/10);
                z_base = Random.Range(-Mathf.Floor(maxZ) * 7/10, Mathf.Floor(maxZ) * 7/10);
                break;
            case EnvironmentComplexity.ADVANCED:
                x_base = Random.Range(-Mathf.Floor(maxX) * 7 / 10, Mathf.Floor(maxX) * 7 / 10);
                z_base = Random.Range(-Mathf.Floor(maxZ) * 7 / 10, Mathf.Floor(maxZ) * 7 / 10);
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
        
		distanceX = x_base - agent.transform.position.x;
		distanceZ = z_base - agent.transform.position.z;

		
		if(Mathf.Abs(distanceX) < deltaTargetAgent)
		{                
			if(distanceX > 0)
			{
				x_base += (deltaTargetAgent - distanceX);
				if (x_base >= maxX)
					x_base = maxX;

				//Debug.Log("Minore X positivo");
			}
			else
			{
				x_base -= (deltaTargetAgent - Mathf.Abs(distanceX));
				if (x_base <= -maxX)
					x_base = maxX;

				//Debug.Log("Minore X negativo");
			}
		}

		if(Mathf.Abs(distanceZ) < deltaTargetAgent)
		{                
			if(distanceZ > 0)
			{
				z_base += (deltaTargetAgent - distanceZ);
				if (z_base >= maxZ)
					z_base = maxZ;

				//Debug.Log("Minore Z positivo");
			}
			else
			{
				z_base -= (deltaTargetAgent - Mathf.Abs(distanceZ));
				if (z_base <= -maxZ)
					z_base = maxX;

				//Debug.Log("Minore Z negativo");
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
                numberOfAlignedObstacle = 6;
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
            Vector3 center = (goal.transform.position + agent.transform.position) * 0.5f;           
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
                                            z_base + environment.transform.position.z); Debug.Log("Ciao 2");
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

        Vector3 targetPosition = GameObject.FindGameObjectWithTag("Finish").transform.localPosition;
        Vector3 agentPosition = GameObject.FindGameObjectWithTag("agent").transform.localPosition;
        //Vector3 center = ( (targetPosition + agentPosition) * 0.5f );
        Vector3 center1 = ( (targetPosition + agentPosition) * 0.5f );
        Vector3 center2 = ( (targetPosition + agentPosition) * 0.5f );
        Vector3 center3 = ( (targetPosition + agentPosition) * 0.5f );

        float zAngle = (targetPosition.z - agentPosition.z);
        float xAngle = (targetPosition.x - agentPosition.x);        
        float angleTan = Mathf.Atan((targetPosition.z - agentPosition.z) / (targetPosition.x - agentPosition.x));
        float angleRad = -angleTan;
        float angleGrad;
        float tempRand;

        angleRad += Mathf.PI / 2;
        angleGrad = angleRad * 180 / Mathf.PI;
        //Debug.Log("Angle: " + angle);
        //Debug.Log("zAngle: " + zAngle + "   xAngle: " + xAngle);
        int i = 0;
        foreach (GameObject obstacle in obstaclesAligned)
        {
            obstacle.transform.rotation = Quaternion.Euler(0, angleGrad + Random.Range(-30,30), 0);
            obstacle.transform.localPosition = center1;            

            if (i != 0)
            {
                tempRand = Random.Range(12, 15);

                // --- for inference on more than 3 obstacles ---
                if (i >= 0 && i <= 2)
                {
                    tempRand = Random.Range(12, 15);

                    obstacle.transform.localPosition = new Vector3(center1.x + tempRand * Mathf.Pow(-1.0f, i) * Mathf.Cos(Mathf.PI - angleRad), center1.y,
                                                                    center1.z + tempRand * Mathf.Pow(-1.0f, i) * Mathf.Sin(Mathf.PI - angleRad));
                    //Debug.Log("tempRand1[" + i + "]: " + tempRand);

                    tempRand = Random.Range(0, Mathf.Sqrt(Mathf.Pow(targetPosition.x - center1.x, 2) + Mathf.Pow(targetPosition.z - center1.z, 2)) * 4 / 5);
                    obstacle.transform.localPosition += new Vector3(tempRand * Mathf.Pow(-1.0f, i) * Mathf.Cos(Mathf.PI - angleRad - Mathf.PI / 2), 0,
                                                                    tempRand * Mathf.Pow(-1.0f, i) * Mathf.Sin(Mathf.PI - angleRad - Mathf.PI / 2));
                    //Debug.Log("tempRand2[" + i + "]: " + tempRand);                
                }
                else if (i == 3)
                {
                    center2 = ((targetPosition + center1) * 0.5f);
                    obstacle.transform.localPosition = new Vector3(center2.x + Mathf.Cos(Mathf.PI - angleRad), center2.y,
                                                                    center2.z + Mathf.Sin(Mathf.PI - angleRad));

                }
                else if (i >=4 && i <=5)
                {
                    tempRand = Random.Range(12, 15);

                    obstacle.transform.localPosition = new Vector3(center2.x + tempRand * Mathf.Pow(-1.0f, i) * Mathf.Cos(Mathf.PI - angleRad), center2.y,
                                                                    center2.z + tempRand * Mathf.Pow(-1.0f, i) * Mathf.Sin(Mathf.PI - angleRad));
                    //Debug.Log("tempRand1[" + i + "]: " + tempRand);

                    tempRand = Random.Range(0, Mathf.Sqrt(Mathf.Pow(targetPosition.x - center2.x, 2) + Mathf.Pow(targetPosition.z - center2.z, 2)) * 4 / 5);
                    obstacle.transform.localPosition += new Vector3(tempRand * Mathf.Pow(-1.0f, i) * Mathf.Cos(Mathf.PI - angleRad - Mathf.PI / 2), 0,
                                                                    tempRand * Mathf.Pow(-1.0f, i) * Mathf.Sin(Mathf.PI - angleRad - Mathf.PI / 2));

                }
                else if (i == 6)
                {
                    //center3 = ((agentPosition + center1) * 0.5f); 
                    center3 = ((targetPosition + center2) * 0.5f);                  
                    obstacle.transform.localPosition = new Vector3(center3.x + Mathf.Cos(Mathf.PI - angleRad), center3.y,
                                                                    center3.z + Mathf.Sin(Mathf.PI - angleRad));
                }
                else
                {
                    tempRand = Random.Range(12, 15);

                    obstacle.transform.localPosition = new Vector3(center3.x + tempRand * Mathf.Pow(-1.0f, i) * Mathf.Cos(Mathf.PI - angleRad), center3.y,
                                                                    center3.z + tempRand * Mathf.Pow(-1.0f, i) * Mathf.Sin(Mathf.PI - angleRad));
                    //Debug.Log("tempRand1[" + i + "]: " + tempRand);

                    tempRand = Random.Range(0, Mathf.Sqrt(Mathf.Pow(targetPosition.x - center3.x, 2) + Mathf.Pow(targetPosition.z - center3.z, 2)) * 4 / 5);
                    obstacle.transform.localPosition += new Vector3(tempRand * Mathf.Pow(-1.0f, i) * Mathf.Cos(Mathf.PI - angleRad - Mathf.PI / 2), 0,
                                                                    tempRand * Mathf.Pow(-1.0f, i) * Mathf.Sin(Mathf.PI - angleRad - Mathf.PI / 2));

                }
            }
            //Debug.Log("Finish: x=" + targetPosition.x + " z=" + targetPosition.z);
            //Debug.Log("Agent: x=" + agentPosition.x + " z=" + agentPosition.z);
            //Debug.Log("Center: x=" + center.x + " z=" + center.z);
            //Debug.Log("Angle: " + angle + " rotation: " + rotation);
           
            i++;
        }
    }

    public static string ReadJSON()
    {
        string path = "./config/env_config.json";
        //Read the text from directly from the file
        StreamReader reader = new StreamReader(path);
        string text = reader.ReadToEnd();
        reader.Close();
        return text;
    }
}
