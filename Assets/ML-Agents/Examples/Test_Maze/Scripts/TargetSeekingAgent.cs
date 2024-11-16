using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TargetSeekingAgent : Agent
{
    [SerializeField] private Transform target;        // The target the agent seeks
    [SerializeField] private float moveSpeed = 2f;    // Movement speed of the agent

    public override void OnEpisodeBegin()
    {
        // Reset the agent’s position and rotation at the start of each episode
        transform.localPosition = new Vector3(Random.Range(-23f, 23f), 0.5f, 0f);
        transform.localRotation = Quaternion.identity;

        // Randomize target position within a specified range on the X-axis only
        target.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, 0f);

        // Log the positions to verify they are being reset correctly
        Debug.Log($"Episode Start - Agent Position: {transform.localPosition}, Target Position: {target.localPosition}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect the agent's position along the X-axis
        sensor.AddObservation(transform.localPosition.x);

        // Collect the target's position along the X-axis
        sensor.AddObservation(target.localPosition.x);

        // Collect the agent's forward direction in the X-axis only
        sensor.AddObservation(transform.forward.x);

        // Log the observations for debugging
        Debug.Log($"Agent X Position: {transform.localPosition.x}, Target X Position: {target.localPosition.x}");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the continuous action for movement in the X direction and rotation
        float moveX = actions.ContinuousActions[0];

        // Apply movement along the X-axis only
        Vector3 move = new Vector3(moveX, 0, 0) * Time.deltaTime * moveSpeed;
        transform.localPosition += move;

        // Calculate the distance to the target along the X-axis for reward calculation
        float distanceToTarget = Mathf.Abs(transform.localPosition.x - target.localPosition.x);

        // Give a small penalty for each step to encourage faster completion
        AddReward(-0.001f); 

        // Log the distance to the target for debugging purposes
        Debug.Log($"Distance to Target (X-axis): {distanceToTarget}");

        // Give a reward and end the episode if the agent reaches the target within a certain threshold on the X-axis
        if (distanceToTarget < 1.0f)
        {
            Debug.Log("Target Reached! Adding Reward.");
            AddReward(1.0f);  // Add reward when the agent reaches the target
            EndEpisode();     // End the episode
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Provide manual control for testing in the editor (left and right arrow keys for X-axis movement)
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Move in X direction only
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the agent collided with an object tagged "Walls"
        if (collision.collider.CompareTag("Walls"))
        {
            // Apply a penalty for hitting a wall
            Debug.Log("Hit Wall! Applying penalty.");
            AddReward(-0.5f);  // Penalize the agent for hitting the wall

            // Optionally, you can end the episode here if a wall hit should terminate the episode
            EndEpisode();
        }
    }

    /*
    private void OnDrawGizmos()
    {
        if (target != null)
        {
            // Draw a line from the agent to the target for visual debugging
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);

            // Draw a small sphere at the target's position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(target.position, 0.3f);
        }
    }
    */
}
