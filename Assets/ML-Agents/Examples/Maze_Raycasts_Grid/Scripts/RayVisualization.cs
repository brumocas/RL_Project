using UnityEngine;

public class RayVisualization : MonoBehaviour
{
    private LineRenderer[] rayLines;
    [SerializeField] private int raysPerDirection = 20;
    [SerializeField] private float maxRayDegrees = 50f;
    [SerializeField] private float rayLength = 50f;
    [SerializeField] private LayerMask rayLayerMask = -1; // Everything
    [SerializeField] private float sphereCastRadius = 0.5f;

    private void Start()
    {
        InitializeRayLines();
    }

    private void InitializeRayLines()
    {
        // Calculate total number of rays (one forward ray + rays on each side)
        int totalRays = 2 * raysPerDirection + 1;
        rayLines = new LineRenderer[totalRays];

        for (int i = 0; i < totalRays; i++)
        {
            GameObject rayObj = new GameObject($"Ray_{i}");
            rayObj.transform.parent = transform;
            rayObj.transform.localPosition = Vector3.zero;

            LineRenderer line = rayObj.AddComponent<LineRenderer>();
            SetupLineRenderer(line);
            rayLines[i] = line;
        }
    }

    private void SetupLineRenderer(LineRenderer line)
    {
        line.useWorldSpace = true;
        line.positionCount = 2;
        
        // Create material for the line
        Material lineMaterial = new Material(Shader.Find("Standard"));
        lineMaterial.SetFloat("_Mode", 3); // Transparent mode
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.DisableKeyword("_ALPHATEST_ON");
        lineMaterial.EnableKeyword("_ALPHABLEND_ON");
        lineMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        lineMaterial.renderQueue = 3000;

        line.material = lineMaterial;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
    }

    private void Update()
    {
        UpdateRayVisuals();
    }

    private void UpdateRayVisuals()
    {
        float startAngle = -maxRayDegrees;
        float angleStep = (2f * maxRayDegrees) / (2f * raysPerDirection);

        for (int i = 0; i < rayLines.Length; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 rayStart = transform.position;
            
            RaycastHit hit;
            bool didHit = Physics.SphereCast(
                rayStart, 
                sphereCastRadius, 
                rayDirection, 
                out hit, 
                rayLength, 
                rayLayerMask
            );

            LineRenderer line = rayLines[i];
            line.SetPosition(0, rayStart);

            if (didHit)
            {
                line.SetPosition(1, hit.point);
                line.material.color = new Color(1f, 0f, 0f, 0.5f); // Red for hits
            }
            else
            {
                line.SetPosition(1, rayStart + rayDirection * rayLength);
                line.material.color = new Color(1f, 1f, 1f, 0.5f); // White for misses
            }
        }
    }
}