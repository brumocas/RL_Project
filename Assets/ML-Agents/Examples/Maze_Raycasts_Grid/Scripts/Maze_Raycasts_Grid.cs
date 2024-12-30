using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using Unity.MLAgents.Policies;

public class Maze_Raycasts_Grid : Agent
{
    [Header("Agent Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float visitedPenalty = -0.2f;
    [SerializeField] private float nonVisitedReward = 0.5f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float stepPenalty = -0.001f;
    [SerializeField] private float wallCollisionPenalty = -10f;
    [SerializeField] private float findTargetReward= 100f;

    [Header("Maze Boundaries")]
    private const float MAZE_MIN = -23f;
    private const float MAZE_MAX = 23f;

    private Transform mazeParent;
    private Transform[] targets;
    private Rigidbody rb;
    private int gridSize;
    private bool[,] visitedCells;
    private GameObject[,] visitedMarkers;


    public override void Initialize()
    {
        SetupComponents();
        InitializeGrid();
        FindTargets();

        base.Initialize();
        // Total cells in grid = (gridSize + 1) * (gridSize + 1)
        int totalGridObservations = (gridSize + 1) * (gridSize + 1); 
        GetComponent<BehaviorParameters>().BrainParameters.VectorObservationSize = 2 + 6 + totalGridObservations;
    }

    private void SetupComponents()
    {
        mazeParent = transform.parent;
        if (mazeParent == null)
        {
            Debug.LogError("Agent must be a child of a maze object!");
            return;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }
    }

    private void InitializeGrid()
    {
        gridSize = Mathf.RoundToInt((MAZE_MAX - MAZE_MIN) / cellSize);
        visitedMarkers = new GameObject[gridSize + 1, gridSize + 1];
        ResetVisitedCells();
    }

    private void ConfigureRigidbody()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.mass = 1f;
        rb.drag = 1f;
    }

    


    private void FindTargets()
    {
        targets = mazeParent.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("Goal"))
            .ToArray();

        if (targets.Length == 0)
        {
            CreateDefaultTargets();
        }
    }

    private void CreateDefaultTargets()
    {
        Vector3[] defaultPositions = {
            new Vector3(-13f, 1.22f, 8f),
            new Vector3(12f, 1.22f, 10f),
            new Vector3(4.2f, 1.22f, -17.5f),
            new Vector3(-12f, 1.22f, -22f)
        };

        targets = new Transform[defaultPositions.Length];

        for (int i = 0; i < defaultPositions.Length; i++)
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.transform.SetParent(mazeParent, false);
            target.transform.localPosition = defaultPositions[i];
            target.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            target.tag = "Goal";
            
            target.GetComponent<Renderer>().material.color = Color.green;
            targets[i] = target.transform;
        }
    }

    


    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        // Convert grid position to local space position
        Vector3 localPos = new Vector3(
            gridPos.x * cellSize + MAZE_MIN,
            0.05f,
            gridPos.y * cellSize + MAZE_MIN
        );

        // Convert local space to world space using the maze parent
        return mazeParent.TransformPoint(localPos);
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPos = mazeParent.InverseTransformPoint(worldPosition);
        
        int x = Mathf.RoundToInt((localPos.x - MAZE_MIN) / cellSize);
        int z = Mathf.RoundToInt((localPos.z - MAZE_MIN) / cellSize); // Notice we use z here
        
        return new Vector2Int(
            Mathf.Clamp(x, 0, gridSize),
            Mathf.Clamp(z, 0, gridSize) // And we return z directly, not using y
        );
    }

    private void ResetVisitedCells()
    {
        if (visitedMarkers != null)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                for (int z = 0; z <= gridSize; z++)
                {
                    if (visitedMarkers[x, z] != null)
                    {
                        if (Application.isPlaying)
                            Destroy(visitedMarkers[x, z]);
                        else
                            DestroyImmediate(visitedMarkers[x, z]);
                        visitedMarkers[x, z] = null;
                    }
                }
            }
        }
        
        visitedCells = new bool[gridSize + 1, gridSize + 1];
        visitedMarkers = new GameObject[gridSize + 1, gridSize + 1];
    }

    private void CreateVisitMarker(Vector2Int cell)
    {
        if (visitedMarkers[cell.x, cell.y] != null) return;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        // First set the parent, then the position
        marker.transform.parent = mazeParent;
        marker.transform.position = GridToWorld(cell);
        marker.transform.localScale = new Vector3(cellSize * 0.9f, 0.02f, cellSize * 0.9f);

        // Create a new transparent material
        Material transparentMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        transparentMaterial.color = new Color(1f, 0f, 0f, 0.5f);
        
        // Assign the material
        var renderer = marker.GetComponent<Renderer>();
        renderer.material = transparentMaterial;
        
        // Remove collider
        Destroy(marker.GetComponent<Collider>());
        
        visitedMarkers[cell.x, cell.y] = marker;
    }

    


public override void OnEpisodeBegin()
{
    // Reset the visited cells and markers
    ResetVisitedCells();

    // Randomize the agent's starting position
    transform.localPosition = GenerateValidPosition(0.5f);
    transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

    // Set a fixed position for the first target
    if (targets != null && targets.Length > 0)
    {
        targets[0].localPosition = new Vector3(0f, 1.22f, 0f); // Example fixed position
    }
}


    public override void CollectObservations(VectorSensor sensor)
    {
        // Add agent's position
        sensor.AddObservation(transform.localPosition);

        // Add agent's velocity
        sensor.AddObservation(rb.velocity);


        Vector2Int currentCell = WorldToGrid(transform.localPosition);
        
        // Full grid state
        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                sensor.AddObservation(visitedCells[x, z] ? 1.0f : 0.0f);
            }
        }

        // Add target position
        // Add target positions (2 observations per target: relative x and z)
        foreach (var target in targets)
        {
            Vector3 relativePos = mazeParent.InverseTransformPoint(target.position) - mazeParent.InverseTransformPoint(transform.position);
            // Normalize relative positions to improve learning (optional)
            sensor.AddObservation(relativePos.x / MAZE_MAX);
            sensor.AddObservation(relativePos.z / MAZE_MAX);
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);

        // Rotation penalty
        float rotationPenalty = Mathf.Abs(actions.ContinuousActions[0]);
        AddReward(-rotationPenalty * 0.001f);
        
        // Grid reward/penalty
        Vector2Int newCell = WorldToGrid(transform.position);
        if (visitedCells[newCell.x, newCell.y])
        {
            AddReward(visitedPenalty);
        }
        else
        {
            AddReward(nonVisitedReward);
            CreateVisitMarker(newCell);
        }
        visitedCells[newCell.x, newCell.y] = true;
        
        // Time step penalty
        AddReward(stepPenalty);
    }

    private Vector3 GenerateValidPosition(float yHeight)
    {
        const int maxAttempts = 100;
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            Vector3 potentialPosition = new Vector3(
                Random.Range(-15f, 18f),
                yHeight,
                Random.Range(-23f, 12f)
            );

            if (IsValidPosition(potentialPosition))
            {
                return potentialPosition;
            }
        }

        return new Vector3(0f, yHeight, 0f);
    }

    private bool IsValidPosition(Vector3 position, float radius = 0.5f)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius);

        foreach (Collider hitCollider in hitColliders)
        {
            // Exclude the agent's own collider
            if (hitCollider.gameObject == gameObject) 
                continue;

            // Check for collision with walls or goals
            if (hitCollider.CompareTag("Walls") || hitCollider.CompareTag("Goal"))
            {
                return false;
            }
        }

        return true; // No collision detected
    }


    


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Walls"))
        {
            AddReward(wallCollisionPenalty);
            EndEpisode();
        }
        else if (collision.collider.CompareTag("Goal"))
        {   
            AddReward(findTargetReward);
            EndEpisode();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || visitedCells == null) return;

        // Solid red with some transparency
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red with 30% opacity

        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                if (visitedCells[x, z])
                {
                    // Convert grid position to world space using maze transform
                    Vector3 worldPos = GridToWorld(new Vector2Int(x, z));
                    
                    // Calculate the size of the gizmo in world space
                    Vector3 gizmoSize = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);
                    
                    // Draw the gizmo
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(worldPos, mazeParent.rotation, gizmoSize);
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                }
            }
        }
        // Reset Gizmos matrix
        Gizmos.matrix = Matrix4x4.identity;
    }
    
}