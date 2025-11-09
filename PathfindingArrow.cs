using System.Collections.Generic;
using UnityEngine;

public class PathfindArrows : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowHeight = 2f;
    [SerializeField] private float arrowSpacing = 2f;
    [SerializeField] private int maxArrowsToShow = 10;
    [SerializeField] private bool showFullPath = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Material pathArrowMaterial;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float arrowPulseSpeed = 2f;
    
    private MazeGenerator maze;
    private List<GameObject> activeArrows = new List<GameObject>();
    private Vector2Int playerGridPos;
    private Vector2Int targetGridPos;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    
    void Start()
    {
        maze = FindObjectOfType<MazeGenerator>();
        if (maze == null)
        {
            Debug.LogError("MazeGenerator not found in scene!");
            return;
        }
        
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab not assigned!");
            return;
        }
        
        // Set target to the exit position
        targetGridPos = new Vector2Int(maze.GetWidth() - 1, maze.GetHeight() - 1);
        
        Debug.Log("PathfindArrows initialized. Target: " + targetGridPos);
    }
    
    void Update()
    {
        if (maze == null) return;
        
        // Update player grid position
        Vector3 playerWorldPos = transform.position;
        Vector3 mazeOrigin = maze.transform.position;
        float cellSize = maze.GetCellSize();
        
        playerGridPos = new Vector2Int(
            Mathf.RoundToInt((playerWorldPos.x - mazeOrigin.x) / cellSize),
            Mathf.RoundToInt((playerWorldPos.z - mazeOrigin.z) / cellSize)
        );
        
        // Clamp player position to maze bounds
        playerGridPos.x = Mathf.Clamp(playerGridPos.x, 0, maze.GetWidth() - 1);
        playerGridPos.y = Mathf.Clamp(playerGridPos.y, 0, maze.GetHeight() - 1);
        
        // Update path and arrows
        UpdatePathAndArrows();
    }
    
    private void UpdatePathAndArrows()
    {
        // Clear old arrows
        ClearArrows();
        
        // Calculate new path using BFS
        currentPath = FindPathBFS(playerGridPos, targetGridPos);
        
        if (currentPath == null || currentPath.Count <= 1)
        {
            // No path found or already at target
            return;
        }
        
        // Create arrows along the path
        CreatePathArrows(currentPath);
    }
    
    private List<Vector2Int> FindPathBFS(Vector2Int start, Vector2Int target)
    {
        if (start == target) return new List<Vector2Int> { start };
        
        int width = maze.GetWidth();
        int height = maze.GetHeight();
        
        // BFS data structures
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = start;
        
        // BFS algorithm
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            // Check if we reached the target
            if (current == target)
            {
                return ReconstructPath(cameFrom, start, target);
            }
            
            // Get all valid neighbors
            List<Vector2Int> neighbors = GetWalkableNeighbors(current);
            
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        // No path found
        Debug.LogWarning("No path found from " + start + " to " + target);
        return null;
    }
    
    private List<Vector2Int> GetWalkableNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int x = cell.x;
        int y = cell.y;
        
        // Check all four directions
        // North
        if (y + 1 < maze.GetHeight() && CanMoveTo(cell, new Vector2Int(x, y + 1)))
            neighbors.Add(new Vector2Int(x, y + 1));
        
        // East
        if (x + 1 < maze.GetWidth() && CanMoveTo(cell, new Vector2Int(x + 1, y)))
            neighbors.Add(new Vector2Int(x + 1, y));
        
        // South
        if (y - 1 >= 0 && CanMoveTo(cell, new Vector2Int(x, y - 1)))
            neighbors.Add(new Vector2Int(x, y - 1));
        
        // West
        if (x - 1 >= 0 && CanMoveTo(cell, new Vector2Int(x - 1, y)))
            neighbors.Add(new Vector2Int(x - 1, y));
        
        return neighbors;
    }
    
    private bool CanMoveTo(Vector2Int from, Vector2Int to)
    {
        // This is a simplified version - you might want to check actual wall data
        // For now, we'll assume all cells are walkable (no walls between them)
        // In a real implementation, you'd check the maze's wall data
        
        return true; // Temporary - always allow movement
    }
    
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = target;
        
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        
        path.Add(start);
        path.Reverse();
        
        return path;
    }
    
    private void CreatePathArrows(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;
        
        int arrowsToCreate = showFullPath ? path.Count - 1 : Mathf.Min(maxArrowsToShow, path.Count - 1);
        
        for (int i = 0; i < arrowsToCreate; i++)
        {
            Vector2Int fromCell = path[i];
            Vector2Int toCell = path[i + 1];
            
            CreateArrowBetweenCells(fromCell, toCell, i, arrowsToCreate);
        }
    }
    
    private void CreateArrowBetweenCells(Vector2Int fromCell, Vector2Int toCell, int arrowIndex, int totalArrows)
    {
        Vector3 fromWorldPos = GridToWorldPosition(fromCell);
        Vector3 toWorldPos = GridToWorldPosition(toCell);
        
        // Calculate arrow position (midpoint between cells)
        Vector3 arrowPos = (fromWorldPos + toWorldPos) * 0.5f;
        arrowPos.y = arrowHeight;
        
        // Calculate arrow rotation to point towards next cell
        Vector3 direction = (toWorldPos - fromWorldPos).normalized;
        Quaternion arrowRotation = Quaternion.LookRotation(direction, Vector3.up);
        
        // Instantiate arrow
        GameObject arrow = Instantiate(arrowPrefab, arrowPos, arrowRotation, transform);
        arrow.name = $"PathArrow_{arrowIndex}";
        
        // Apply visual settings
        ApplyArrowVisuals(arrow, (float)arrowIndex / totalArrows);
        
        activeArrows.Add(arrow);
    }
    
    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        Vector3 mazeOrigin = maze.transform.position;
        float cellSize = maze.GetCellSize();
        
        return mazeOrigin + new Vector3(
            gridPos.x * cellSize + cellSize * 0.5f,
            0,
            gridPos.y * cellSize + cellSize * 0.5f
        );
    }
    
    private void ApplyArrowVisuals(GameObject arrow, float progress)
    {
        Renderer renderer = arrow.GetComponent<Renderer>();
        if (renderer != null && pathArrowMaterial != null)
        {
            // Create material instance
            Material arrowMaterial = new Material(pathArrowMaterial);
            
            // Interpolate color based on progress
            Color arrowColor = Color.Lerp(startColor, endColor, progress);
            arrowMaterial.color = arrowColor;
            
            // Add emission for glow effect
            arrowMaterial.EnableKeyword("_EMISSION");
            arrowMaterial.SetColor("_EmissionColor", arrowColor * 0.5f);
            
            renderer.material = arrowMaterial;
        }
        
        // Add pulsing animation
        StartCoroutine(ArrowPulseAnimation(arrow));
    }
    
    private System.Collections.IEnumerator ArrowPulseAnimation(GameObject arrow)
    {
        Vector3 originalScale = arrow.transform.localScale;
        float pulseIntensity = 0.1f;
        
        while (arrow != null)
        {
            float pulse = Mathf.PingPong(Time.time * arrowPulseSpeed, 1f);
            float scaleMultiplier = 1f + pulse * pulseIntensity;
            arrow.transform.localScale = originalScale * scaleMultiplier;
            
            yield return null;
        }
    }
    
    private void ClearArrows()
    {
        foreach (GameObject arrow in activeArrows)
        {
            if (arrow != null)
            {
                Destroy(arrow);
            }
        }
        activeArrows.Clear();
    }
    
    // Public methods for external control
    public void SetTarget(Vector2Int newTarget)
    {
        targetGridPos = newTarget;
        UpdatePathAndArrows();
    }
    
    public void SetTargetWorldPosition(Vector3 worldPosition)
    {
        Vector3 mazeOrigin = maze.transform.position;
        float cellSize = maze.GetCellSize();
        
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt((worldPosition.x - mazeOrigin.x) / cellSize),
            Mathf.RoundToInt((worldPosition.z - mazeOrigin.z) / cellSize)
        );
        
        SetTarget(gridPos);
    }
    
    public void TogglePathVisibility(bool visible)
    {
        foreach (GameObject arrow in activeArrows)
        {
            if (arrow != null)
            {
                arrow.SetActive(visible);
            }
        }
    }
    
    public List<Vector2Int> GetCurrentPath()
    {
        return new List<Vector2Int>(currentPath);
    }
    
    public Vector2Int GetPlayerGridPosition()
    {
        return playerGridPos;
    }
    
    public Vector2Int GetTargetGridPosition()
    {
        return targetGridPos;
    }
    
    void OnDestroy()
    {
        ClearArrows();
    }
}