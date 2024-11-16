using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/*
public class TargetSeekingAgent : Agent
{
    [SerializeField] private Transform target;        // The target the agent seeks
    [SerializeField] private float moveSpeed = 2f;    // Movement speed of the agent
    [SerializeField] private float rotationSpeed = 200f; // Rotation speed of the agent

    public override void OnEpisodeBegin()
    {
        // Reset the agent’s position and rotation at the start of each episode
        transform.localPosition = new Vector3(Random.Range(-23f, 23f), 0.5f, Random.Range(-23f, 23f));
        transform.localRotation = Quaternion.identity;

        // Randomize target position within a specified range
        target.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, Random.Range(-23f, 23f));

        // Log the positions to verify they are being reset correctly
        Debug.Log($"Episode Start - Agent Position: {transform.localPosition}, Target Position: {target.localPosition}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect the agent's position as an observation
        Vector3 agentPos = transform.localPosition;
        sensor.AddObservation(agentPos);

        // Collect the target's position as an observation
        Vector3 targetPos = target.localPosition;
        sensor.AddObservation(targetPos);

        // Collect the agent's rotation (facing direction) as an observation
        sensor.AddObservation(transform.forward);

        // Log the observations for debugging
        Debug.Log($"Agent Position: {agentPos}, Target Position: {targetPos}");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the continuous actions for movement in the x and z directions and rotation
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];

        // Apply the movement based on the actions received
        Vector3 move = new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
        transform.localPosition += move;

        // Apply rotation based on actions (only when rotate value is non-zero)
        if (Mathf.Abs(rotate) > 0.5f)  // Threshold to prevent very small rotations
        {
            transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);
        }

        // Calculate the distance to the target for reward calculation
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

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
        // Provide manual control for testing in the editor (WASD or arrow keys)
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Move in X direction
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // Move in Z direction

        // Rotation control (Q and E for rotating left and right)
        /*
        continuousActionsOut[2] = 0f; // Reset the rotation action value by default
        if (Input.GetKey(KeyCode.Q)) // Rotate left
        {
            continuousActionsOut[2] = -1f;
        }
        else if (Input.GetKey(KeyCode.E)) // Rotate right
        {
            continuousActionsOut[2] = 1f;
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
    
}
*/
