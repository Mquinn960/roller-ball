using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RollerAgent : Agent
{
    private Rigidbody rBody;
    private DateTime initialTime;
    private bool foundTarget;

    public Transform Target;
    public float speed = 10;

    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    private void initEpisode()
    {
        SetReward(0.0f);
        foundTarget = false;
        initialTime = DateTime.Now;
    }
    
    public override void OnEpisodeBegin()
    {
        this.initEpisode();

        if (this.transform.localPosition.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 0, 0.5f, 0);
        }

        // Move the target to a new spot
        Target.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4,
                                           0.5f,
                                           UnityEngine.Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor) 
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        var diffInSeconds = (DateTime.Now - initialTime).TotalSeconds;

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            foundTarget = true;
        }

        if (foundTarget)
        {
            // Found the target
            SetReward(0.5f);

            if (diffInSeconds >= 2)
            {
                AddReward(0.5f);
                EndEpisode();
            }

            // Fell off platform after finding target
            if (this.transform.localPosition.y < 0)
            {
                AddReward(-0.45f);
                EndEpisode();
            }

        } else {
            // Fell off platform before finding target
            if (this.transform.localPosition.y < 0)
            {
                SetReward(0.0f);
                EndEpisode();
            }    
        }

    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }

}
