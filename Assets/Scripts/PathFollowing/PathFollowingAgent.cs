using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimulationManagerPFollowing))]



public class PathFollowingAgent : Agent
{

    StatsRecorder statsRecorder;

    public enum RewardType
    {
        Goal,
        Collision,
        Dense,
        TimeOut
    }

    //debug purpouse
    private float printReward=0;

    //Var to save the percentage of goals reached over the number of iterations
    private float ratio = 0;

    private Rigidbody _rigitBody;
    private CarController _controller;
    private SimulationManagerPFollowing _simulationManager;
    private ActionSegment<float> _lastActions;

    private bool isTraining;
    private int maxIteration;

    //Numero di collisioni
    private int colNumber = 0;
    //Numero di goal 
    private int goalNumber = 0;
    //Numero di volte in cui l'episodio termina senza goal/collisione
    private int timeoutNum = 0;

    private GameObject _targetGoal;

    //Vector3 circular array for saving positions 
    //private float[] arrayOfPosition = new float[20];

    //Index to manage array datas
    //private int bufferIndex = 0;

    //arrayOfPosition variance
    //private float arrayVariance;

    //Variance threshold;
    //public float varianceThreshold;

    public void Awake()
    {
        statsRecorder = Academy.Instance.StatsRecorder;
    }

    public override void Initialize()
    {
        _rigitBody = GetComponent<Rigidbody>();
        _controller = GetComponent<CarController>();
        _simulationManager = GetComponent<SimulationManagerPFollowing>();
        isTraining = _simulationManager.configurationManager.isTraining;
        maxIteration = _simulationManager.configurationManager.maxIteration;
        _simulationManager.InitializeSimulation();
    }

    public override void OnEpisodeBegin()
    {
        //Stop the simulation if isOver true
        if(_simulationManager.configurationManager.isOver == true && isTraining == false)
        {
            Time.timeScale = 0;
        }
        
        if(isTraining == false && _simulationManager.configurationManager.isOver == false)
        {
            _simulationManager.configurationManager.iteration++;
        }

        //Array.Clear(arrayOfPosition, 0, 20);
        _simulationManager.InitializeSimulation();
        _targetGoal = null;
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        _lastActions = vectorAction.ContinuousActions;
        _controller.CurrentSteeringAngle = vectorAction.ContinuousActions[0];
        _controller.CurrentAcceleration = vectorAction.ContinuousActions[1];
        _controller.CurrentBrakeTorque = vectorAction.ContinuousActions[2];
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = Input.GetAxis("Jump");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("barrier"))
        {

            AddReward(ComputeReward(RewardType.Collision));
            IncrementCollision();

            EndEpisode();

        }
    }

    private void FixedUpdate()
    {
        if (StepCount == MaxStep - 1)
        {
            IncrementTimeout();
            ComputeReward(RewardType.TimeOut);
        }

        if (((goalNumber + timeoutNum + colNumber) % 100) == 0)
        {
            statsRecorder.Add("Goal Ratio %", 100 * (float)goalNumber / (goalNumber + timeoutNum + colNumber));
            statsRecorder.Add("Collision Ratio %", 100 * (float)colNumber / (goalNumber + timeoutNum + colNumber));
            statsRecorder.Add("Timeout Ratio %", 100 * (float)timeoutNum / (goalNumber + timeoutNum + colNumber));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        //Check if we are in model checking mode
        if (isTraining == false && colNumber + goalNumber + timeoutNum != 0)
            //Update ratio
           ratio = goalNumber / (float)(colNumber + goalNumber + timeoutNum);

        if (_simulationManager.configurationManager.iteration >= maxIteration && isTraining == false)
        {
            _simulationManager.configurationManager.isOver = true;
        }

        if (_simulationManager.InitComplete)
        {
            //Check the index value, set it to 0 if has reached the array's max size
            //if(bufferIndex > 19)
            //{
            //    bufferIndex = 0;
            //}

            if (_targetGoal == null)
                _targetGoal = _simulationManager.configurationManager.goal;
            //_targetGoal = _simulationManager.target[i];
            Vector3 distance = _targetGoal.transform.position - transform.position;
            Vector3 dirToTarget = (distance).normalized;
            float magDistance = distance.magnitude;
            Vector3 agentPos = transform.position - _simulationManager.configurationManager.environment.transform.position;
            //Vector3 targetPos = _targetGoal.transform.position - _simulationManager.configurationManager.environment.transform.position;

            //arrayOfPosition[bufferIndex] = agentPos.magnitude;
            /*
            if(arrayOfPosition[19] != 0f)
            {
                //Compute variance
                StartCoroutine(ComputeVariance(arrayOfPosition, 20));
            }*/ 

            //Observations
            //sensor.AddObservation(agentPos.normalized);
            //sensor.AddObservation(this.transform.InverseTransformPoint(_targetGoal.transform.position));
            sensor.AddObservation(
                this.transform.InverseTransformVector(_rigitBody.velocity.normalized)); //come sono allinato con la velocità
            sensor.AddObservation(
                this.transform.InverseTransformDirection(dirToTarget)); //dove dovrei andare
            //valore di riferimento per obs precedenti.
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(distance);
            //sensor.AddObservation(transform.right);
            //sensor.AddObservation(StepCount / MaxStep);
            float velocityAlignment = Vector3.Dot(dirToTarget, _rigitBody.velocity.normalized);
            
            //Dense reward 
            AddReward(ComputeReward(RewardType.Dense, velocityAlignment, magDistance));

            //Increase buffer index 
            //bufferIndex++;
        }
        else
        {
            sensor.AddObservation(new float[18]);
        }
    }

    public IEnumerator JackpotReward(float bonus)
    {

        AddReward(ComputeReward(RewardType.Goal, bonus));
        IncrementGoal();

        yield return new WaitForEndOfFrame();

        EndEpisode();

    }

    public float ComputeReward(RewardType type, float bonus1 = 0f, float bonus2 = 0f)
    {

        ///////REWARD TABLE
        /// Ending alignment (bonus1 for goal) goes from: fermandosi in modo coerente con l'allineamento max 0.8 min 0.3, contromano max 0.2 min 0
        /// Dot product (bonus1 for dense) goes from -1 to 1
        /// Distance from target (bonus2) is the distance to target. Considering a 100m^2 is 100*sqrt(2). Min distance is 0

        ///////REWARD RANGES
        /// Collision: -75
        /// DENSE
        /// Goal: from 40 to 100

        //reward var
        float reward = 0;

        ////////// Reward values
        float rewardGoal = 50f; //was 40
        float rewardCollision = -150f; //backup 75
        //float rewardStucked = -50f;

        //coefficient for the ending alignment between the agent and the goal
        float c0 = 75f;
        //Angle
        float c1 = 1f;
        //Coeff. distance
        float c2 = -0.01f;
        //Coeff to scale relu-like function
        float c3 = 0.01f;


        //float c3 = 0.001f;

        switch (type)
        {
            case RewardType.Goal:
                //calcolo reward goal             
                //il goal fornisce all'agente un reward pari a 0.3 + il valore bonus dato dall'allineamento moltiplicato per il coeff c0
                reward = rewardGoal + c0 * bonus1;
                //Debug.Log("Goal reached! Reward: " + reward + " Steps " + StepCount);
                break;
            case RewardType.Dense:
                //calcolo reward denso, dato dal prodottto tra una costante e l'allineamento della velocità rispetto al target
                reward = c1 * (bonus1 - 1) / (bonus2 + 1) + c2 * (bonus2 * bonus2) * Mathf.Max(1, StepCount - 700) * c3; //-1 aggiunto al bonus per avere reward sempre negativo tranne quando sta facendo bene |||| aggiungere distanza 
                this.printReward = reward;
                break;
            case RewardType.Collision:
                //penalità per aver colpito un ostacolo
                reward = rewardCollision;
                //Debug.Log("Collision detected! Reward: " + reward + " Steps " + StepCount);
                break;
            /*
            case RewardType.Stuck:
                reward = rewardStucked;
                //Debug.Log("Stucked Vehicle");
                break;*/
        }

        return reward;
    }

    public void IncrementCollision()
    {
        colNumber++;

        if (isTraining == false) 
        { 
            Debug.Log("Collision number: " + colNumber);
            Debug.Log("Success rate: " + ratio);
        }
    }

    public void IncrementGoal()
    {
        goalNumber++;

        if (isTraining == false) 
        { 
            Debug.Log("Goal number: " + goalNumber);
            Debug.Log("Success rate: " + ratio);
        }
    }

    public void IncrementTimeout()
    {
        timeoutNum++;

        if(isTraining == false) 
        { 
            Debug.Log("Timeouts: " + timeoutNum);
            Debug.Log("Success rate: " + ratio);
        }
    }

    
    // variance
    /*
    public IEnumerator ComputeVariance(float[] a, int n)
    {
        // Compute mean (average of elements)
        float sum = 0f;

        for (int i = 0; i < n; i++)
            sum += a[i];

        float mean = (float)sum / (float)n;

        // Compute sum squared
        // differences with mean.
        float sqDiff = 0f;

        for (int i = 0; i < n; i++)
            sqDiff += (a[i] - mean) * (a[i] - mean);

        arrayVariance = sqDiff / n;
        //Debug.Log("Variance: " + arrayVariance);

        if (arrayVariance < varianceThreshold)
        {
            //Debug.LogWarning("Threshold");
            AddReward(ComputeReward(RewardType.Stuck));

            yield return new WaitForEndOfFrame();

            EndEpisode();
        }
    }*/

}
