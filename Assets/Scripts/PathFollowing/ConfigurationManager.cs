using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class ConfigurationManager : MonoBehaviour
{
    [SerializeField] public PathFollowingAgent agent;
    [SerializeField] public GameObject environment;
    [SerializeField] public GameObject goal;
    [SerializeField] public ConfigDeserializer parameters;
    [SerializeField] public GameObject carModel;
    // [SerializeField] public float carLength;
    // [SerializeField] public float carWidth;

    //Variable to set the configuration in training mode or model checking mode
    public bool isTraining;

    //Declaring a variable for the number of iteration we wants to check the model
    public int iteration;
    public int maxIteration;

    //Garage transform list
    [SerializeField] public Transform[] garage;

    //Flag that becomes true when the number of iteration has reached maxIteration
    public bool isOver = false;

    [System.Serializable]
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

    private void Awake()
    {
        parameters = JsonUtility.FromJson<ConfigDeserializer>(ReadJSON());
        ScaleCar();
        SetCarParameters();
    }

    void Update()
    {

    }

    /*
    //Print ratio on GUI
    private void OnGUI()
    {
        GUI.Label(new Rect(10,10,100,100),"Ratio= " + ratio.ToString("F"));
    }*/

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
        if (agent != null && isOver == false)
        {
            //Call the function for the generation of the random spot position
            float maxX = 0, maxZ = 0, minX = 0, minZ = 0;

            float rotation = Random.Range(0, 360);

            float x_base;
            float z_base;

            maxX = 9.75f - (parameters.car.width / 2) * Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * rotation)) - (parameters.car.length / 2) * Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * rotation));
            maxZ = 3.75f - (parameters.car.width / 2) * Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * rotation)) - (parameters.car.length / 2) * Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * rotation));
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
            agent.transform.rotation = Quaternion.Euler(0, 0, 0);

            //Setting random position
            agent.transform.position = transform.parent.position + new Vector3(x_base, 0.5f, z_base);

        }
    }

    public void RepositionTargetRandom()
    {
        int i = Random.Range(0, garage.Length);

        //int i = 5;

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
        for (int j = 0; j < garage.Length; j++)
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
