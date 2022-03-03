using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;


[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimulationManagerPFollowing))]

public class PathFollowingAgent : Agent {

    private Rigidbody _rigidBody;
    private CarController _controller;
    private SimulationManagerPFollowing _simulationManager;
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
    private float goals_reward = 0;

    //computing acceleration purpose
    public Vector3 lastVelocity;

    public List<Vector3> path = new List<Vector3>();
    public List<float> agentAccel = new List<float>();

    public Dictionary<string, dynamic> dic = new Dictionary<string, dynamic>();

    public override void Initialize() {
        _rigidBody = GetComponent<Rigidbody>();
        _controller = GetComponent<CarController>();
        _simulationManager = GetComponent<SimulationManagerPFollowing>();
        isTraining = _simulationManager.configurationManager.isTraining;
        maxIteration = _simulationManager.configurationManager.maxIteration;
        _simulationManager.InitializeSimulation();
        recorder = Academy.Instance.StatsRecorder;

        if (isTraining == false) stat_period = maxIteration;
    }

    public override void OnEpisodeBegin() {
        if(_simulationManager.configurationManager.isOver == true && isTraining == false) Time.timeScale = 0;
        if(isTraining == false && _simulationManager.configurationManager.isOver == false) 
        _simulationManager.configurationManager.iteration++;
        _simulationManager.InitializeSimulation();
        _target = _simulationManager.configurationManager.goal;
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

    // Timeout and draw path
    void FixedUpdate()
    {
        if (StepCount == MaxStep - 1)
            TimeOut();
        
        Vector3 acceleration = (this.GetComponent<Rigidbody>().velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = this.GetComponent<Rigidbody>().velocity;

        path.Add(this.transform.GetChild(2).position);
        agentAccel.Add(acceleration.magnitude);

        if (path.Count > 1)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                DrawLine(path[i], path[i + 1], 2f);
                //Debug.DrawLine(checkpoints[i].transform.position, checkpoints[i+1].transform.position, Color.green, 0.05f, false);
            }
        }
    }

    // Collision
    private void OnCollisionEnter(Collision other) {  if (other.gameObject.CompareTag("barrier")) CollisionReward(); }

    // Observation
    public override void CollectObservations(VectorSensor sensor) {
        if (!_simulationManager.InitComplete) { sensor.AddObservation(new float[9]); return; }
        if (_simulationManager.configurationManager.iteration >= maxIteration && isTraining == false) _simulationManager.configurationManager.isOver = true;
        
        // Observations
        Vector2 agent_position = new Vector2(transform.position.x, transform.position.z);
        Vector2 target_position = new Vector2(_target.transform.position.x, _target.transform.position.z);
        Vector2 velocity = new Vector2(_rigidBody.velocity.x, _rigidBody.velocity.z);
        Vector2 agent_forward = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 target_forward = new Vector2(_target.transform.forward.x, _target.transform.forward.z);
        int time = StepCount;

        sensor.AddObservation(target_position - agent_position); // Direction towards the target
        sensor.AddObservation(velocity); // Agent velocity
        sensor.AddObservation(agent_forward); // Agent direction
        sensor.AddObservation(target_forward); // Target direction
        //sensor.AddObservation(time); // Time

        // Add dense reward
        float distance = Mathf.Abs(_target.transform.position.x - transform.position.x) + Mathf.Abs(_target.transform.position.z - transform.position.z); 
        float alignment = Mathf.Abs(Vector3.Dot(agent_forward, target_forward));

        DenseReward(alignment, time, distance);
    }

    public void UpdateRatio() {
        if(episode_count >= stat_period) {
            recorder.Add("Events/Goals", 100 * goals_achieved_count / (float)stat_period);
            recorder.Add("Events/Timeouts", 100 * timeouts_count / (float)stat_period);
            recorder.Add("Events/Collisions", 100 * collisions_count / (float)(stat_period));
            recorder.Add("Rewards/Goals", goals_reward / (float)stat_period);
            recorder.Add("Rewards/Dense", dense_reward / (float)stat_period);
            recorder.Add("Rewards/Collisions", collisions_reward / (float)stat_period);
            recorder.Add("Rewards/Time", time_reward / (float)stat_period);
            recorder.Add("Rewards/Distance", distance_reward / (float)stat_period);
            recorder.Add("Rewards/Allignment", allignment_reward / (float)stat_period);
            time_reward = 0;
            distance_reward = 0;
            allignment_reward = 0;
            episode_count=0;
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

    // Dense reward
    private float sigmoid(float value) { return (float)(1.0f / (1.0f + Math.Pow(Math.E, -value))); }
    private float distance_contribution(float weight, float distance) { return (float)(-weight * distance); }
    private float alignment_contribution(float weight, float alignment, float distance) { return (float)(-weight * (1 - alignment) / (distance * distance + 1)); }
    private float time_contribution(float weight, float time, float distance) { return -weight * sigmoid((time/30)-4) * sigmoid(distance-4); }
    
    public void DenseReward(float alignment, float time, float distance) {
        //float tc = time_contribution(0.1f, time, distance);
        float tc = 0;
        float dc = distance_contribution(0.01f, distance);
        float ac = alignment_contribution(1.5f, alignment, distance);
        float reward = 0.01f * ( tc + dc + ac );
        //Debug.Log("dc: " + dc + " ac: " + ac + " tc: " + tc + "reward: " + reward);
        if (isTraining == true) AddReward(reward);
        dense_reward += reward;
        time_reward += tc;
        distance_reward += dc;
        allignment_reward += ac;
    }

    // Goal reward
    public void GoalReward() {

        float reward = 1.0f;
        //Debug.Log(reward);
        if (isTraining == true) AddReward(reward);
        goals_reward += reward;
        goals_achieved_count++;
        //SaveToFile("test");
        //Time.timeScale = 0;
        EndEpisode();
        
    }

    // Collision reward
    public void CollisionReward() {
        float reward = -0.01f;
        //Debug.Log(reward);
        if (isTraining == true) AddReward(reward);
        collisions_reward += reward;
        if(!hasCollided) {
            collisions_count++;
            hasCollided = true;
        }

        //SaveToFile("test");
    }

    // Timeout
    public void TimeOut() 
    {
        timeouts_count++;
        //SaveToFile("test");
        EndEpisode();
    }

    public void ResetPathList()
    {
        path.Clear();
        agentAccel.Clear();
    }

    void SaveToFile(string fileName)
    {
        System.IO.File.WriteAllText(fileName, ""); // clear old file, if any
        string firstLine = System.String.Format("x,y,z\r\n");
        System.IO.File.AppendAllText(fileName, firstLine);
        foreach (Vector3 pos in path)
        {
            // format XYZ separated by ; and with 2 decimal places:
            string line = System.String.Format("{0,3:f2},{1,3:f2},{2,3:f2}\r\n", pos.x, pos.y, pos.z);
            System.IO.File.AppendAllText(fileName, line); // append to the file
        }
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, float width)
    {
        int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
        if (count == 1)
        {
            Debug.DrawLine(p1, p2, Color.green, 0.05f, false);
        }
        else
        {
            Camera c = Camera.main;
            if (c == null)
            {
                Debug.LogError("Camera.main is null");
                return;
            }
            var scp1 = c.WorldToScreenPoint(p1);
            var scp2 = c.WorldToScreenPoint(p2);

            Vector3 v1 = (scp2 - scp1).normalized; // line direction
            Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector

            for (int i = 0; i < count; i++)
            {
                Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
                Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                Debug.DrawLine(origin, destiny, Color.green, 0.05f, false);
            }
        }
    }


}
