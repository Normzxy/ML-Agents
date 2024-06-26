using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HungryPredator : Agent
{
    // This is our hungry Predator.
    [SerializeField] float moveSpeed;
    private Rigidbody rb;

    public HungryAgent agent;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Predator respawn position.
        transform.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), .4f, Random.Range(-4.0f, 4.0f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Knowledge about position.
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Agent does not move backwards. It rotates and moves forward.
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        // Forward is an X axis.
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        // Rotates by the Y axis around it's own Rigidbody.
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
    }

    // Optional user input interface controll.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    private void OnTriggerEnter(Collider other)
    {
        // Nutritious!
        if (other.gameObject.tag == "Agent")
        {
            AddReward(100.0f);
            agent.AddReward(-50.0f);
            agent.EndEpisode();
            EndEpisode();
        }

        // Avoid walls, buddy!
        if (other.gameObject.tag == "Wall")
        {
            agent.AddReward(-50.0f);
            agent.EndEpisode();
            EndEpisode();
        }
    }
}
