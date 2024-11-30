using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Maze_Raycasts : Agent
{
    [SerializeField] private Transform target_1;        // The target the agent seeks
    [SerializeField] private Transform target_2;        // The target the agent seeks
    [SerializeField] private Transform target_3;        // The target the agent seeks
    [SerializeField] private Transform target_4;        // The target the agent seeks
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
    transform.localPosition = GenerateValidPosition(0.5f);

    // Reset the agent's rotation
    transform.localRotation = Quaternion.Euler(
        0, // rotation in X-axis
        0, // rotation in Y-axis
        0 // rotation in Z-axis
    );

    // Set the target position
    //target_1.localPosition = GenerateValidPosition(1.22f);
    target_1.localPosition = new Vector3(-13f, 1.22f, 8f);
    // Set the target position
    //target_2.localPosition = GenerateValidPosition(1.22f);
    target_2.localPosition = new Vector3(12f, 1.22f, 10f);
    // Set the target position
    //target_3.localPosition = GenerateValidPosition(1.22f);
    target_3.localPosition = new Vector3(4.2f, 1.22f, -17.5f);
    // Set the target position
    //target_4.localPosition = GenerateValidPosition(1.22f);
    target_4.localPosition = new Vector3(-12f, 1.22f, -22f);

    // Print positions for debugging
    //Debug.Log($"Episode Start - Agent Position: {transform.localPosition}, Target Position: {target_1.localPosition}");
    }

    // Method to generate a valid position
    private Vector3 GenerateValidPosition(float yHeight)
    {
    const int maxAttempts = 100; // Limit attempts to prevent infinite loops
    int attempts = 0;

    while (attempts < maxAttempts)
    {
        attempts++;

        // Generate a random position within defined bounds
        Vector3 potentialPosition = new Vector3(
            //Random.Range(-23f, 23f),  // Adjust bounds based on maze size
            Random.Range(-23f, 23f),
            yHeight,
            Random.Range(-23f, 23f)
            //Random.Range(-23f, 23f)
        );

        // Check if position is valid
        if (IsValidPosition(potentialPosition))
        {
            return potentialPosition;
        }
    }

    Debug.LogWarning("Failed to find a valid position after max attempts. Returning default position.");
    return new Vector3(0f, yHeight, 0f); // Default fallback position
    }


    // Method to validate a position
    private bool IsValidPosition(Vector3 position)
    {
    // Check for collisions using a small sphere
    Collider[] hitColliders = Physics.OverlapSphere(position, 0.5f); // Adjust radius as needed
    foreach (Collider hitCollider in hitColliders)
    {
        if (hitCollider.CompareTag("Walls")) // Invalid if overlapping a wall
        {
            return false;
        }

        if (hitCollider.CompareTag("Goal")) // Invalid if overlapping a Target
        {
            return false;
        }
    }
    return true;
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

        AddReward(-0.001f);
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
            AddReward(-1f);
            // Uncomment to end the episode if hitting a wall is critical
            EndEpisode();
        }

        if (collision.collider.CompareTag("Goal"))
        {
            Debug.Log("Found Target! Great");
            AddReward(10f);
            EndEpisode();
        }
    }
}
