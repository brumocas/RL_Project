using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class SimpleAgentMovement : Agent
{
    [SerializeField] private float moveSpeed = 2f;       // Speed of forward/backward movement
    [SerializeField] private float rotationSpeed = 200f; // Speed of left/right rotation

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the action values for forward/backward movement and left/right rotation
        float moveForward = actions.ContinuousActions[0]; // Move forward or backward
        float rotate = actions.ContinuousActions[1];      // Rotate left or right

        Debug.Log($"OnActionReceived: moveForward={moveForward}, rotate={rotate}");

        // Apply movement and rotation
        transform.localPosition += transform.forward * moveForward * Time.deltaTime * moveSpeed;
        transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual controls for testing in the editor (using WASD or arrow keys)
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");   // Forward/backward movement
        continuousActionsOut[1] = Input.GetAxis("Horizontal"); // Left/right rotation

        Debug.Log($"Heuristic: Vertical={Input.GetAxis("Vertical")}, Horizontal={Input.GetAxis("Horizontal")}");
    }
}

