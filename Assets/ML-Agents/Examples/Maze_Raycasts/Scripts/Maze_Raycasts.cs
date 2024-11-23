using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Maze_Raycasts : Agent
{
    [SerializeField] private Transform target;        // The target the agent seeks
    [SerializeField] private float moveSpeed = 5f;    // Movement speed of the agent
    private float previousDistanceToTarget;

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
        // Reset the agent's position
        transform.localPosition = new Vector3(0f, 0.5f, 0f);

        // Randomize the target position
        target.localPosition = new Vector3(5f, 1.22f, -10f);
        //target.localPosition = new Vector3(7.5f, 1.22f, -1.5f);   
        // Print positions
        Debug.Log($"Episode Start - Agent Position: {transform.localPosition}, Target Position: {target.localPosition}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition);

        // Target position
        //sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0]; // Rotation
        float moveForward = actions.ContinuousActions[1]; // Forward movement

        // Apply movement
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);

        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Provide manual control for debugging
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
            // Uncomment to end the episode if hitting a wall is critical
            EndEpisode();
        }

        if (collision.collider.CompareTag("Goal"))
        {
            Debug.Log("Found Target! Great");
            AddReward(5f);
            EndEpisode();
        }
    }
}
