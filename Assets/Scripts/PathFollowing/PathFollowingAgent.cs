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

    public enum RewardType
    {
        Goal,
        Collision,
        Dense
    }

    //debug purpouse
    private float printReward=0;

    private Rigidbody _rigitBody;
    private CarController _controller;
    private SimulationManagerPFollowing _simulationManager;
    private ActionSegment<float> _lastActions;

    private bool isTraining;
    private bool isOver;
    
    private GameObject _targetGoal;
   

    public override void Initialize()
    {
        _rigitBody = GetComponent<Rigidbody>();
        _controller = GetComponent<CarController>();
        _simulationManager = GetComponent<SimulationManagerPFollowing>();
        isTraining = _simulationManager.configurationManager.isTraining;
        isOver = _simulationManager.configurationManager.isOver;
        _simulationManager.InitializeSimulation();
    }

    public override void OnEpisodeBegin()
    {
        //Stop the simulation if isOver true
        if(isOver == true && isTraining == false)
        {
            Time.timeScale = 0;
        }
        
        if(isTraining == false && isOver == false)
        {
            _simulationManager.configurationManager.iteration++;
        }

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
            if (isTraining)
            {
                AddReward(ComputeReward(RewardType.Collision));
            }
            else if (isTraining == false)
            {
                _simulationManager.configurationManager.IncrementCollision();
            }

            EndEpisode();

        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*
        //Check if we are in model checking mode
        if (isTraining == false && colNumber + goalNumber + timeoutNum != 0)
            //Update ratio
            ratio = goalNumber / (float)(colNumber + goalNumber + timeoutNum);

        if (iteration >= maxIteration && isTraining == false)
        {
            isOver = true;
        }*/

        if (_simulationManager.InitComplete)
        {
            if (_targetGoal == null)
                _targetGoal = _simulationManager.configurationManager.goal;
            //_targetGoal = _simulationManager.target[i];
            Vector3 distance = _targetGoal.transform.position - transform.position;
            Vector3 dirToTarget = (distance).normalized;
            float magDistance = distance.magnitude;
            Vector3 agentPos = transform.position - _simulationManager.configurationManager.environment.transform.position;
            //Vector3 targetPos = _targetGoal.transform.position - _simulationManager.configurationManager.environment.transform.position;

            //Observations
            sensor.AddObservation(agentPos.normalized);
            sensor.AddObservation(this.transform.InverseTransformPoint(_targetGoal.transform.position));
            sensor.AddObservation(
                this.transform.InverseTransformVector(_rigitBody.velocity.normalized)); //come sono allinato con la velocità
            sensor.AddObservation(
                this.transform.InverseTransformDirection(dirToTarget)); //dove dovrei andare
            //valore di riferimento per obs precedenti.
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(transform.right);
            //sensor.AddObservation(StepCount / MaxStep);
            float velocityAlignment = Vector3.Dot(dirToTarget, _rigitBody.velocity.normalized);

            if (isTraining == true)
            { 
                AddReward(ComputeReward(RewardType.Dense, velocityAlignment, magDistance));
            }
            else if (isTraining == false)
            {   
                if(StepCount == MaxStep - 1)
                    _simulationManager.configurationManager.IncrementTimeout();
            }
        }
        else
        {
            sensor.AddObservation(new float[18]);
        }
    }

    public IEnumerator JackpotReward(float bonus)
    {

        if (isTraining == true) 
        { 
            AddReward(ComputeReward(RewardType.Goal, bonus));
        }
        else if (isTraining == false)
        {
            _simulationManager.configurationManager.IncrementGoal();
        }

        yield return new WaitForEndOfFrame();

        EndEpisode();


        /*
        if (i == n && target == _simulationManager.target[n]) 
        {
            if (bonus > 0.2f)
                Debug.LogWarning("Jackpot hit! " + bonus + " Target" + i);
            AddReward(0.3f + bonus);
            yield return new WaitForEndOfFrame();

            i = 0;
            EndEpisode();
        }
        else if(i != n && target == _simulationManager.target[i])
        {
            if (bonus > 0.2f)
                Debug.LogWarning("Jackpot hit! " + bonus + " Target" + i);
            AddReward(0.001f + bonus);
            yield return new WaitForEndOfFrame();

            //lista di punti(waypoints), cambio il punto in cui devo passare
            i++;
            _targetGoal = _simulationManager.target[i];
        }*/
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
        float rewardGoal = 40f;
        float rewardCollision = -75f;

        //coefficient for the ending alignment between the agent and the goal
        float c0 = 75f;
        //Angle
        float c1 = 1f;
        //Coeff. distance
        float c2 = -0.01f; 


        //float c3 = 0.001f;

        switch (type)
        {
            case RewardType.Goal:
                //calcolo reward goal             
                //il goal fornisce all'agente un reward pari a 0.3 + il valore bonus dato dall'allineamento moltiplicato per il coeff c0
                reward = rewardGoal + c0 * bonus1;
                Debug.Log("Goal reached! Reward: " + reward + " Steps " + StepCount);
                break;
            case RewardType.Dense:
                //calcolo reward denso, dato dal prodottto tra una costante e l'allineamento della velocità rispetto al target
                reward = c1 * (bonus1 - 1) / (bonus2 + 1) + c2 * (bonus2 * bonus2); //-1 aggiunto al bonus per avere reward sempre negativo tranne quando sta facendo bene |||| aggiungere distanza 
                this.printReward = reward;
                break;
            case RewardType.Collision:
                //penalità per aver colpito un ostacolo
                reward = rewardCollision;
                Debug.Log("Collision detected! Reward: " + reward + " Steps " + StepCount);
                break;
        }

        return reward;
    }

}
