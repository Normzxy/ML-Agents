using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditorInternal.VR;


public class HungryAgent : Agent
{
    // This is our hungry Agent.
    [SerializeField] float moveSpeed;
    private Rigidbody rb;

    // This is a food that Angents wants to eat.
    public int foodCount;
    [SerializeField] private Transform target;
    public GameObject food;
    [SerializeField] private List<GameObject> foodList = new();

    // Related to environment...
    [SerializeField] private Transform environmentLocation;

    // Related to timekeeping...
    [SerializeField] private int timePunishment;
    private float deadline;

    // This is danger...
    public HungryPredator predator;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Agent respawn position.
        transform.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), .4f, Random.Range(-4.0f, 4.0f));

        // Food respawn function.
        FoodDelivery();

        // Set punishment timer.
        NewEpisodeTimer();
    }

    private void Update()
    {
        CheckDeadline();
    }

    // Random area Food spawn.
    private void FoodDelivery()
    {
        if (foodList.Count != 0)
        {
            FoodDiscard(foodList);
        }

        for (int i = 0; i < foodCount; i++)
        {
            GameObject freshFood = Instantiate(food);
            // For more training environments, foods needs to be connected with the specific environment.
            freshFood.transform.parent = environmentLocation;
            freshFood.transform.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), .4f, Random.Range(-4.0f, 4.0f));
            foodList.Add(freshFood);
        }
    }

    private void FoodDiscard(List<GameObject> toDiscard)
    {
        foreach(GameObject i in toDiscard)
        {
            Destroy(i.gameObject);
        }
        toDiscard.Clear();
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

    // Variable other refers to everything that is colliding with Agent.
    private void OnTriggerEnter(Collider other)
    {
        // Nutritious!
        if (other.gameObject.tag == "Food")
        {
            foodList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10.0f);
            if (foodList.Count == 0)
            {
                AddReward(10.0f);
                predator.AddReward(-20.0f);
                predator.EndEpisode();
                // Scene restarts.
                EndEpisode();
            }
        }

        // Avoid walls, buddy!
        if (other.gameObject.tag == "Wall")
        {
            FoodDiscard(foodList);
            AddReward(-50.0f);
            predator.EndEpisode();
            EndEpisode();
        }
    }

    // Keeps track on a deadline.
    // Time.time is a current time.
    private void NewEpisodeTimer()
    {
        deadline = Time.time + timePunishment;
    }

    private void CheckDeadline()
    {
        if(Time.time >= deadline)
        {
            AddReward(-5.0f);
            predator.AddReward(-5.0f);
            FoodDiscard(foodList);
            predator.EndEpisode();
            EndEpisode();
        }
    }
}
