using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class Maze_Raycasts : Agent
{
    [SerializeField] private List<Transform> targets = new List<Transform>(); // List of targets
    [SerializeField] private float moveSpeed = 2f;

    private int targetCounter = 0;
    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on the agent.");
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent's position and rotation
        transform.localPosition = new Vector3(-21f, 0.5f, -21f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        rb.velocity = Vector3.zero;

        // Reset targets    
        targetCounter = 0;
        SetTargetPositions();
    }

    private void SetTargetPositions()
    {
        // Using the target list to set their positions
        targets[0].localPosition = new Vector3(-21f, 1.22f, -3f);
        targets[1].localPosition = new Vector3(-21f, 1.22f, 22f);
        targets[2].localPosition = new Vector3(-5f, 1.22f, 22f);
        targets[3].localPosition = new Vector3(-5f, 1.22f, 16f);
        targets[4].localPosition = new Vector3(-14f, 1.22f, 16f);
        targets[5].localPosition = new Vector3(-14f, 1.22f, 9f);
        targets[6].localPosition = new Vector3(1f, 1.22f, 9f);
        targets[7].localPosition = new Vector3(1f, 1.22f, -3f);
        targets[8].localPosition = new Vector3(1f, 1.22f, -22f);
        targets[9].localPosition = new Vector3(8f, 1.22f, -22f);
        targets[10].localPosition = new Vector3(8f, 1.22f, -3f);
        targets[11].localPosition = new Vector3(8f, 1.22f, 9f);
        targets[12].localPosition = new Vector3(16f, 1.22f, 9f);
        targets[13].localPosition = new Vector3(16f, 1.22f, -3f);
        targets[14].localPosition = new Vector3(16f, 1.22f, -22f);
        targets[15].localPosition = new Vector3(16f, 1.22f, -22f);
        targets[16].localPosition = new Vector3(22f, 1.22f, -22f);
        targets[17].localPosition = new Vector3(22f, 1.22f, 22f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add agent's position and velocity
        sensor.AddObservation(transform.localPosition);
        
        // Optionally add agent's velocity if needed
        sensor.AddObservation(rb.velocity);
        
        // Add relative position of all targets to the agent's observation
        foreach (Transform target in targets)
        {
            sensor.AddObservation(target.localPosition - transform.localPosition);
        }
    }

    private Transform GetCurrentTarget()
    {
        // Returns the current target based on the targetCounter
        if (targetCounter < targets.Count)
        {
            return targets[targetCounter];
        }
        return null;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0]; // Rotation
        float moveForward = actions.ContinuousActions[1]; // Forward movement

        // Apply movement and rotation
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * (moveSpeed / 2), 0f, Space.Self);

        // Add small time penalty to encourage faster learning
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual control for debugging
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Rotation
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // Forward
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Walls"))
        {
            Debug.Log("Hit Wall! Applying penalty.");
            AddReward(-0.5f);
            EndEpisode();
            return;
        }

        if (collision.collider.CompareTag("Goal"))
        {
            Debug.Log($"Found Target {targetCounter + 1}! Rewarding agent.");

            // Reward and deactivate current target
            AddReward(targetCounter + 1);
            DeactivateTarget(targetCounter);

            // If all targets are reached, end the episode
            if (++targetCounter >= targets.Count)
            {
                AddReward(100f); // Final reward for completing the maze
                EndEpisode();
            }
        }
    }

    private void DeactivateTarget(int targetIndex)
    {
        // Deactivate target by moving it off-screen
        targets[targetIndex].localPosition = new Vector3(-100f, -100f, 100f);
    }
}
