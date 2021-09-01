using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(CarController))]

public class ReferencePathGeneration : MonoBehaviour
{

    private NavMeshAgent agent;//Navigation component
    private LineRenderer lr;//LineRenderer component
    private NavMeshPath path;
    private CarController _controller;

    public Transform target;
    private Vector3[] cornerArray;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //agent.isStopped = true;
        lr = GetComponent<LineRenderer>();
        NavMeshPath path = new NavMeshPath();
        //Set the navigation position
        agent.CalculatePath(target.position, path);
        agent.SetDestination(target.position);

        int arrayLength = path.corners.Length;
        Vector3[] cornerArray = new Vector3[arrayLength];
        for (int i = 0; i < path.corners.Length; i++)
        {
            cornerArray[i] = path.corners[i];
        }

        for (int i = 0; i < path.corners.Length; i++)
        {    
            Debug.Log("Corners = " + path.corners[i]);
            //Debug.Log("CornerArray = " + cornerArray[i]);
        }
        
        //Display path lines
        lr.positionCount = path.corners.Length;
        lr.SetPositions(path.corners);
    }



        /*
        public Transform target;

        private NavMeshAgent agent;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(target.position, path);


        }


        public Camera cam;

        public UnityEngine.AI.NavMeshAgent agent;

        //Calcolo path e follow, il target è dato dal click del mouse sulla mappa
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit))
                {
                    agent.SetDestination(hit.point);
                }

            }   
        }*/
    }
