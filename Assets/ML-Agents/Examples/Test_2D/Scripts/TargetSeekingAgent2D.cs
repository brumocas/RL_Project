using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TargetSeekingAgent2D : Agent
{
    [SerializeField] private Transform target;        // The target the agent seeks
    [SerializeField] private float moveSpeed = 2f;    // Movement speed of the agent

    public override void OnEpisodeBegin()
    {
        // Reset the agent's position at the start of each episode
        transform.localPosition = new Vector3(Random.Range(-23f, 23f), 0.5f, Random.Range(-23f, 23f));

        // Randomize the target position within a specified range in the X-Z plane
        target.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, Random.Range(-23f, 23f));

        // Log the positions to verify they are being reset correctly
        Debug.Log($"Episode Start - Agent Position: {transform.localPosition}, Target Position: {target.localPosition}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect the agent's position (X and Z only) as observations
        Vector2 agentPos = new Vector2(transform.localPosition.x, transform.localPosition.z);
        sensor.AddObservation(agentPos);

        // Collect the target's position (X and Z only) as observations
        Vector2 targetPos = new Vector2(target.localPosition.x, target.localPosition.z);
        sensor.AddObservation(targetPos);

        // Log the observations for debugging
        Debug.Log($"Agent Position: {agentPos}, Target Position: {targetPos}");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the continuous actions for movement along the X and Z axes
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Apply movement based on the actions received
        Vector3 move = new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;
        transform.localPosition += move;

        // Calculate the distance to the target for reward calculation
        float distanceToTarget = Vector2.Distance(new Vector2(transform.localPosition.x, transform.localPosition.z), 
                                                  new Vector2(target.localPosition.x, target.localPosition.z));

        // Give a small penalty for each step to encourage faster completion
        AddReward(-0.001f);

        // Log the distance to the target for debugging purposes
        Debug.Log($"Distance to Target: {distanceToTarget}");

        // Give a reward and end the episode if the agent reaches the target
        if (distanceToTarget < 1.0f)
        {
            Debug.Log("Target Reached! Adding Reward.");
            AddReward(1.0f);  // Add reward when the agent reaches the target
            EndEpisode();     // End the episode
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Provide manual control for testing in the editor (arrow keys for movement in X-Z plane)
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Move along the X direction
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // Move along the Z direction
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
            //EndEpisode();
        }
    }

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
}




