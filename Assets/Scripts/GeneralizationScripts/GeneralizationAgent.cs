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
[RequireComponent(typeof(Simulation))]

public class GeneralizationAgent : Agent {

    private Rigidbody _rigidBody;
    private CarController _controller;
    private Simulation _simulation;
    private ActionSegment<float> _lastActions;
    private GameObject _target; 

    StatsRecorder recorder;

    public enum RewardType { Goal, Collision, Dense, TimeOut }

    private bool isTraining;
    private int maxIteration;
    private bool hasCollided;

    private int stat_period = 10;
    private int episode_count = 0;
    private int collisions_count = 0;
    private int goals_achieved_count = 0;
    private int timeouts_count = 0;

    private float dense_reward = 0;
    private float time_reward = 0;
    private float distance_reward = 0;
    private float allignment_reward = 0;
    private float collisions_reward = 0;
    private float alignmentDistance_reward = 0;
    private float goals_reward = 0;
    
    private double maxObs; // To normalize observationss

    public override void Initialize() {
        _rigidBody = GetComponent<Rigidbody>();
        _controller = GetComponent<CarController>();
        _simulation = GetComponent<Simulation>();
        isTraining = _simulation.configManager.isTraining;
        maxIteration = _simulation.configManager.maxIteration;
        _simulation.InitializeSimulation();
        recorder = Academy.Instance.StatsRecorder;
        
        maxObs = Math.Sqrt(2) * _simulation.configManager.MaxLength();

        if (isTraining == false) stat_period = maxIteration;
    }

    public override void OnEpisodeBegin() {
        if(_simulation.configManager.isOver == true && isTraining == false) Time.timeScale = 0;
        if(isTraining == false && _simulation.configManager.isOver == false) 
        _simulation.configManager.iteration++;
        _simulation.InitializeSimulation();                
        _target = _simulation.configManager.goal;
        hasCollided = false;
        UpdateRatio();
    }

    public override void OnActionReceived(ActionBuffers vectorAction) {
        _lastActions = vectorAction.ContinuousActions;
        _controller.CurrentSteeringAngle = vectorAction.ContinuousActions[0];
        _controller.CurrentAcceleration = vectorAction.ContinuousActions[1];
        _controller.CurrentBrakeTorque = vectorAction.ContinuousActions[2];
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = Input.GetAxis("Jump");
    }

    // Timeout
    void FixedUpdate() { if(StepCount == MaxStep - 1) TimeOut(); }

    // Collision
    private void OnCollisionEnter(Collision other) 
    {  
        if (other.gameObject.CompareTag("barrier"))  
        {
            CollisionReward(); 
            //Debug.Log("Collisione Barrier-Agent");
        }
        if (other.gameObject.CompareTag("obstacle"))
        {
            CollisionReward();
            //Debug.Log("Collisione Obstacle-Agent");
        }
        if (other.gameObject.CompareTag("obstacleAligned"))
        {
            CollisionReward();
            //Debug.Log("Collisione ObstacleAligned-Agent");
        }
    }

    // Observation
    public override void CollectObservations(VectorSensor sensor) {
        if (!_simulation.InitComplete) { sensor.AddObservation(new float[9]); return; }
        if (_simulation.configManager.iteration >= maxIteration && isTraining == false) _simulation.configManager.isOver = true;
        
        // Observations
        Vector2 agent_position = new Vector2(transform.position.x, transform.position.z);
        Vector2 target_position = new Vector2(_target.transform.position.x, _target.transform.position.z);
        Vector2 velocity = new Vector2(_rigidBody.velocity.x, _rigidBody.velocity.z);
        Vector2 agent_forward = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 target_forward = new Vector2(_target.transform.forward.x, _target.transform.forward.z);
        int time = StepCount;

        sensor.AddObservation( (target_position - agent_position) /*/ (float)maxObs */); // Direction towards the target
        sensor.AddObservation(velocity); // Agent velocity
        sensor.AddObservation(agent_forward); // Agent direction
        sensor.AddObservation(target_forward); // Target direction
        //sensor.AddObservation(time); // Time
        

        // Add dense reward
        float distance = Mathf.Abs(_target.transform.position.x - transform.position.x) + Mathf.Abs(_target.transform.position.z - transform.position.z); 
        float alignment = Mathf.Abs(Vector3.Dot(agent_forward, target_forward));
        float alignmentDistance = Mathf.Abs(Vector3.Dot(agent_forward, target_position - agent_position)); // To learn to go over the target

        DenseReward(alignment, time, distance, alignmentDistance);
        //Debug.Log("obs1: " + ((target_position - agent_position) / (float)maxObs) + " obs2: " + velocity + " obs3: " + agent_forward + " obs4: " + target_forward);
    }    

    // Dense reward
    private float sigmoid(float value) { return (float)(1.0f / (1.0f + Math.Pow(Math.E, -value))); }
    private float distance_contribution(float weight, float distance) { return (float)(-weight * distance); }
    private float alignment_contribution(float weight, float alignment, float distance) { return (float)(-weight * (1 - alignment) / (distance * distance + 1)); }
    private float alignmentDistance_contribution(float weight, float alignment, float distance) { return (float)(-weight * (1 - alignment) / (distance * distance + 1)); }
    private float time_contribution(float weight, float time, float distance) { return -weight * sigmoid((time/30)-4) * sigmoid(distance-4); }
    public void DenseReward(float alignment, float time, float distance, float alignmentDistance) {
        //float tc = time_contribution(0.1f, time, distance);
        float tc = 0;
        float dc = distance_contribution(0.01f, distance);
        float ac = alignment_contribution(0, alignment, distance);
        float adc = alignmentDistance_contribution(0, alignment, distance);
        float reward = 0.01f * ( tc + dc + ac + adc);
        //Debug.Log("dc: " + dc + " ac: " + ac + " tc: " + tc + "reward: " + reward);
        if (isTraining == true) AddReward(reward);
        dense_reward += reward;
        time_reward += tc;
        distance_reward += dc;
        allignment_reward += ac;
        alignmentDistance_reward += adc;
    }

    // Goal reward
    public void GoalReward() {
        float reward = 10.0f;        
        if (isTraining == true) AddReward(reward);
        goals_reward += reward;
        goals_achieved_count++;
        EndEpisode();
    }

    // Collision reward
    public void CollisionReward() {
        float reward = -5f;
        //Debug.Log(reward);
        if (isTraining == true) AddReward(reward);
        collisions_reward += reward;
        if(!hasCollided) {
            collisions_count++;
            hasCollided = true;
        }
    }

    // Timeout
     public void TimeOut() {
        float reward = -0.5f;
		if (isTraining == true) AddReward(reward);
		timeouts_count++;
        EndEpisode();
    }

    public void UpdateRatio()
    {
        if (episode_count >= stat_period)
        {
            recorder.Add("Events/Goals", 100 * goals_achieved_count / (float)stat_period);
            recorder.Add("Events/Timeouts", 100 * timeouts_count / (float)stat_period);
            recorder.Add("Events/Collisions", 100 * collisions_count / (float)(stat_period));
            recorder.Add("Rewards/Goals", goals_reward / (float)stat_period);
            recorder.Add("Rewards/Dense", dense_reward / (float)stat_period);
            recorder.Add("Rewards/Collisions", collisions_reward / (float)stat_period);
            recorder.Add("Rewards/Time", time_reward / (float)stat_period);
            recorder.Add("Rewards/Distance", distance_reward / (float)stat_period);
            recorder.Add("Rewards/Allignment", allignment_reward / (float)stat_period);
            recorder.Add("Rewards/alignmentDistance_contribution", alignmentDistance_reward / (float)stat_period);
            time_reward = 0;
            distance_reward = 0;
            allignment_reward = 0;
            episode_count = 0;
            goals_achieved_count = 0;
            timeouts_count = 0;
            collisions_count = 0;
            dense_reward = 0;
            collisions_reward = 0;
            goals_reward = 0;
        }
        if (isTraining == false) print("Episode: " + episode_count + " Goals: " + goals_achieved_count + " Collisions: " + collisions_count + " Timeouts: " + timeouts_count);
        episode_count++;
    }
}
