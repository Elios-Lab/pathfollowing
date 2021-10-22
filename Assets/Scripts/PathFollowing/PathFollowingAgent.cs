using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimulationManagerPFollowing))]
public class PathFollowingAgent : Agent
{
    private Rigidbody _rigitBody;
    private CarController _controller;
    private SimulationManagerPFollowing _simulationManager;
    private float[] _lastActions;
    //private GameObject _targetGoal;
    private Target _targetGoal;
    int i = 0;

    public override void Initialize()
    {
        _rigitBody = GetComponent<Rigidbody>();
        _controller = GetComponent<CarController>();
        _simulationManager = GetComponent<SimulationManagerPFollowing>();
        _simulationManager.InitializeSimulation();
    }

    public override void OnEpisodeBegin()
    {
        _simulationManager.InitializeSimulation();
        _targetGoal = null;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        _lastActions = vectorAction;
        _controller.CurrentSteeringAngle = vectorAction[0];
        _controller.CurrentAcceleration = vectorAction[1];
        _controller.CurrentBrakeTorque = vectorAction[2];
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
        actionsOut[2] = Input.GetAxis("Jump");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("barrier"))
        {
            AddReward(-0.01f);

            if(i != 2)
            {
                i = 0;
            }

            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_lastActions != null && _simulationManager.InitComplete)
        {
            if (_targetGoal == null)
                _targetGoal = _simulationManager.target[i];
                //_targetGoal = _simulationManager.target;
            Vector3 dirToTarget = (_targetGoal.transform.position - transform.position).normalized;
            sensor.AddObservation(transform.position.normalized);
            sensor.AddObservation(
                this.transform.InverseTransformPoint(_targetGoal.transform.position));
            sensor.AddObservation(
                this.transform.InverseTransformVector(_rigitBody.velocity.normalized));
            sensor.AddObservation(
                this.transform.InverseTransformDirection(dirToTarget));
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(transform.right);
            //sensor.AddObservation(StepCount / MaxStep);
            float velocityAlignment = Vector3.Dot(dirToTarget, _rigitBody.velocity);
            AddReward(0.001f * velocityAlignment);
        }
        else
        {
            sensor.AddObservation(new float[18]);
        }
    }

    public IEnumerator JackpotReward(float bonus, Target target)
    {
        /*
        if (bonus > 0.2f)
            Debug.LogWarning("Jackpot hit! " + bonus + " Target " + i);
        AddReward(0.2f + bonus);
        yield return new WaitForEndOfFrame();

        EndEpisode();*/
        
        if (i == 2) 
        {
            i = 0;
            EndEpisode();
        }
        else if(target == _simulationManager.target[i])
        {
            if (bonus > 0.2f)
                Debug.LogWarning("Jackpot hit! " + bonus + " Target" + i);
            AddReward(0.2f + bonus);
            yield return new WaitForEndOfFrame();

            //lista di punti(waypoints), cambio il punto in cui devo passare
            i++;
            _targetGoal = _simulationManager.target[i];
        }

    }

}
