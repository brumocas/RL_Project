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
    private GameObject gridContainer;


    public override void Initialize()
    {
        base.Initialize();

        // first method to be called
        SetupComponents();
        InitializeGrid();
        FindTargets();
        CreateGridVisualization();

        // Total cells in grid = (gridSize + 1) * (gridSize + 1)
        int totalGridObservations = (gridSize + 1) * (gridSize + 1); 
        GetComponent<BehaviorParameters>().BrainParameters.VectorObservationSize = 2 + 6 + totalGridObservations;
    }

    private void CreateGridVisualization()
    {
        // Create a container for the grid cells if it doesn't exist
        GameObject gridContainer = new GameObject("GridVisualization");
        gridContainer.transform.parent = mazeParent;

        // Create cells for the entire grid
        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cell.transform.parent = gridContainer.transform;
                
                // Position each cell in the world using your existing GridToWorld method
                Vector2Int gridPos = new Vector2Int(x, z);
                Vector3 worldPos = new Vector3(
                    x * cellSize + MAZE_MIN,
                    1.22f,  // Same height as your maze
                    z * cellSize + MAZE_MIN
                );
                cell.transform.position = mazeParent.TransformPoint(worldPos);
                
                // Rotate to lay flat
                cell.transform.rotation = Quaternion.Euler(90, 0, 0);
                
                // Scale to fit your cell size
                cell.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1);

                // Set a transparent material                
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Mode", 3); // Set transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.color = new Color(0, 0, 0, 0); // Start fully transparent
                
                var renderer = cell.GetComponent<Renderer>();
                renderer.material = mat;
                
                // Remove the collider since we don't need physics
                Destroy(cell.GetComponent<Collider>());
                
                // Store reference in your array
                visitedMarkers[x, z] = cell;
            }
        }
    }
    private void InitializeGrid()
    {
        gridSize = Mathf.RoundToInt((MAZE_MAX - MAZE_MIN) / cellSize);
        visitedMarkers = new GameObject[gridSize + 1, gridSize + 1];
        ResetVisitedCells();
    }

    private void ResetVisitedCells()
    {
        visitedCells = new bool[gridSize + 1, gridSize + 1];
        
 
        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                if (visitedMarkers[x, z] != null)
                {
                    var renderer = visitedMarkers[x, z].GetComponent<Renderer>();
                    renderer.material.color = new Color(0f, 0f, 0f, 0f); // Reset to fully transparent
                }
            }
        }
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
            MarkCellAsVisited(newCell);
        }
        visitedCells[newCell.x, newCell.y] = true;
        
        // Time step penalty
        AddReward(stepPenalty);
    }

    // Modify how you mark visited cells
    private void MarkCellAsVisited(Vector2Int cell)
    {
        if (!visitedCells[cell.x, cell.y])
        {
            visitedCells[cell.x, cell.y] = true;

            if (visitedMarkers[cell.x, cell.y] != null)
            {
                var renderer = visitedMarkers[cell.x, cell.y].GetComponent<Renderer>();
                var mat = renderer.material;
                
                // Ensure transparency settings are correct
                if (!mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
                
                mat.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
            }
        }
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
    
}