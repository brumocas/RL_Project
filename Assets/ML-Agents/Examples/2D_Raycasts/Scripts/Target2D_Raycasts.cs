using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Target2D_Raycasts : Agent
{
    [SerializeField] private Transform target_1;        // The target the agent seeks
    [SerializeField] private float moveSpeed = 5f;    // Movement speed of the agent
    private float previousDistanceToTarget;
    private float distance2Target;
    private float minDeltaTime;
    private bool foudPosition = false;
    private float target_1_x;
    private float target_1_y;
    private float target_1_z;

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
        
        // Reset Pos
        foudPosition = false;

        // Reset the agent's position
        transform.localPosition = new Vector3(Random.Range(-23f, 23f), 0.5f, Random.Range(-23f, 23f));

        // Reset the agent's rotation
        transform.localRotation = Quaternion.Euler(
            0, // rotation in X-axis
            0, // rotation in Y-axis
            0 // rotation in Z-axis
        );
        
        // Target y position already defined (plane)
        target_1_y = 1.22f;
        while(!foudPosition)
        {
            // Randomize the target position
            target_1_x = Random.Range(-23f, 23f);
            target_1_z = Random.Range(-23f, 23f);
            
            // Calculate distance to target_1
            distance2Target = Vector2.Distance(new Vector2(transform.localPosition.x, transform.localPosition.z), new Vector2(target_1_x, target_1_z));

            // Threshold
            if (distance2Target > 0.2)
                foudPosition = true;

            // Calculate minDeltaTime
            minDeltaTime = distance2Target/moveSpeed;
        }

        target_1.localPosition = new Vector3(target_1_x, target_1_y, target_1_z);
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

        // Rotation Penalty 
        float rotationPenalty = Mathf.Abs(actions.ContinuousActions[0]);
        AddReward(-rotationPenalty * 0.01f);

        // Apply Penalty
        AddReward(-2*Time.deltaTime/minDeltaTime);
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
