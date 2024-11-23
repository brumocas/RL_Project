using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Target2D_Raycasts : Agent
{
    [SerializeField] private Transform target_1;        // The target the agent seeks
    //[SerializeField] private Transform target_2;        // The target the agent seeks
    //[SerializeField] private Transform target_3;        // The target the agent seeks
    //[SerializeField] private Transform target_4;        // The target the agent seeks
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
        transform.localPosition = new Vector3(Random.Range(-23f, 23f), 0.5f, Random.Range(-23f, 23f));

        // Reset the agent's rotation
        transform.localRotation = Quaternion.Euler(
            0, // rotation in X-axis
            0, // rotation in Y-axis
            0 // rotation in Z-axis
        );

        // Randomize the target position
        target_1.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, Random.Range(-23f, 23f));

        // Randomize the target position
        //target_2.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, Random.Range(-23f, 23f));

        // Randomize the target position
        //target_3.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, Random.Range(-23f, 23f));

        // Randomize the target position
        //target_4.localPosition = new Vector3(Random.Range(-23f, 23f), 1.22f, Random.Range(-23f, 23f));


        // Print positions
        //Debug.Log($"Episode Start - Agent Position: {transform.localPosition}, Target Position: {target.localPosition}");
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
            AddReward(-5f);
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
