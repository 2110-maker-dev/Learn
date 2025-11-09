using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 15;
    [SerializeField] private float cellSize = 4f;
    [SerializeField] private float wallHeight = 3f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject exitMarkerPrefab;
    
    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool createExitOpening = true; // Remove walls around exit
    
    private Cell[,] grid;
    private Vector3 mazeOrigin;
    
    private class Cell
    {
        public int x, z;
        public bool visited;
        public bool[] walls = { true, true, true, true }; // N, E, S, W
        
        public Cell(int x, int z)
        {
            this.x = x;
            this.z = z;
            this.visited = false;
        }
    }
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateMaze();
        }
    }
    
    public void GenerateMaze()
    {
        Debug.Log("Starting maze generation...");
        ClearMaze();
        InitializeGrid();
        GenerateMazeWithDFS();
        BuildMaze3D();
        PlaceExitMarker();
        Debug.Log("Maze generation complete!");
    }
    
    private void ClearMaze()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void InitializeGrid()
    {
        grid = new Cell[width, height];
        mazeOrigin = transform.position;
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                grid[x, z] = new Cell(x, z);
            }
        }
    }
    
    private void GenerateMazeWithDFS()
    {
        Stack<Cell> stack = new Stack<Cell>();
        Cell current = grid[0, 0];
        current.visited = true;
        stack.Push(current);
        
        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Cell> unvisitedNeighbors = GetUnvisitedNeighbors(current);
            
            if (unvisitedNeighbors.Count > 0)
            {
                stack.Push(current);
                
                Cell next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                RemoveWallBetween(current, next);
                
                next.visited = true;
                stack.Push(next);
            }
        }
    }
    
    private List<Cell> GetUnvisitedNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        
        // North
        if (cell.z + 1 < height && !grid[cell.x, cell.z + 1].visited)
            neighbors.Add(grid[cell.x, cell.z + 1]);
        
        // East
        if (cell.x + 1 < width && !grid[cell.x + 1, cell.z].visited)
            neighbors.Add(grid[cell.x + 1, cell.z]);
        
        // South
        if (cell.z - 1 >= 0 && !grid[cell.x, cell.z - 1].visited)
            neighbors.Add(grid[cell.x, cell.z - 1]);
        
        // West
        if (cell.x - 1 >= 0 && !grid[cell.x - 1, cell.z].visited)
            neighbors.Add(grid[cell.x - 1, cell.z]);
        
        return neighbors;
    }
    
    private void RemoveWallBetween(Cell current, Cell next)
    {
        int dx = next.x - current.x;
        int dz = next.z - current.z;
        
        if (dz == 1) // North
        {
            current.walls[0] = false;
            next.walls[2] = false;
        }
        else if (dx == 1) // East
        {
            current.walls[1] = false;
            next.walls[3] = false;
        }
        else if (dz == -1) // South
        {
            current.walls[2] = false;
            next.walls[0] = false;
        }
        else if (dx == -1) // West
        {
            current.walls[3] = false;
            next.walls[1] = false;
        }
    }
    
    private void BuildMaze3D()
    {
        // Optionally remove walls around exit to create opening
        if (createExitOpening)
        {
            Cell exitCell = grid[width - 1, height - 1];
            // Only remove north wall - keep east wall intact!
            exitCell.walls[0] = false; // North (where the green gate will be)
            // exitCell.walls[1] = false; // East - REMOVED THIS LINE
        }
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Cell cell = grid[x, z];
                Vector3 cellPos = mazeOrigin + new Vector3(x * cellSize, 0, z * cellSize);
                
                // Create floor
                if (floorPrefab != null)
                {
                    GameObject floor = Instantiate(floorPrefab, cellPos + new Vector3(0, 0f, 0), Quaternion.identity, transform);
                    floor.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    floor.name = $"Floor_{x}_{z}";
                }
                
                // Create walls
                if (wallPrefab != null)
                {
                    // North wall
                    if (cell.walls[0])
                    {
                        CreateWall(cellPos + new Vector3(0, wallHeight / 2, cellSize / 2), 
                                   new Vector3(cellSize, wallHeight, 0.2f), 
                                   $"Wall_N_{x}_{z}");
                    }
                    
                    // East wall
                    if (cell.walls[1])
                    {
                        CreateWall(cellPos + new Vector3(cellSize / 2, wallHeight / 2, 0), 
                                   new Vector3(0.2f, wallHeight, cellSize), 
                                   $"Wall_E_{x}_{z}");
                    }
                    
                    // South wall
                    if (cell.walls[2])
                    {
                        CreateWall(cellPos + new Vector3(0, wallHeight / 2, -cellSize / 2), 
                                   new Vector3(cellSize, wallHeight, 0.2f), 
                                   $"Wall_S_{x}_{z}");
                    }
                    
                    // West wall
                    if (cell.walls[3])
                    {
                        CreateWall(cellPos + new Vector3(-cellSize / 2, wallHeight / 2, 0), 
                                   new Vector3(0.2f, wallHeight, cellSize), 
                                   $"Wall_W_{x}_{z}");
                    }
                }
            }
        }
    }
    
    private void CreateWall(Vector3 position, Vector3 scale, string name)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
        wall.transform.localScale = scale;
        wall.name = name;
    }
    
    private void PlaceExitMarker()
    {
        if (exitMarkerPrefab != null)
        {
            // Position at the exit opening (North wall of exit cell)
            Vector3 exitPos = mazeOrigin + new Vector3((width - 1) * cellSize, wallHeight / 2, (height - 1) * cellSize + cellSize / 2);
            
            // Create a thin vertical panel as the gate (facing inward)
            GameObject exit = Instantiate(exitMarkerPrefab, exitPos, Quaternion.identity, transform);
            exit.transform.localScale = new Vector3(cellSize * 0.95f, wallHeight * 0.95f, 0.05f); // Very thin panel
            exit.name = "Exit_Gate";
            
            // Remove ALL colliders so player can walk through
            Collider[] colliders = exit.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                Destroy(col);
            }
            
            // Force apply semi-transparent glowing material
            Renderer exitRenderer = exit.GetComponent<Renderer>();
            if (exitRenderer != null)
            {
                // Create new material instance to avoid changing prefab material
                Material gateMaterial = new Material(Shader.Find("Standard"));
                
                // Set transparent rendering mode
                gateMaterial.SetFloat("_Mode", 3); // Transparent
                gateMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                gateMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                gateMaterial.SetInt("_ZWrite", 0);
                gateMaterial.DisableKeyword("_ALPHATEST_ON");
                gateMaterial.EnableKeyword("_ALPHABLEND_ON");
                gateMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                gateMaterial.renderQueue = 3000;
                
                // Set color to semi-transparent green
                Color gateColor = new Color(0, 1, 0, 0.3f); // Green with 30% opacity
                gateMaterial.color = gateColor;
                
                // Add emission for glow
                gateMaterial.EnableKeyword("_EMISSION");
                gateMaterial.SetColor("_EmissionColor", Color.green * 1.5f);
                
                exitRenderer.material = gateMaterial;
            }
            
            Debug.Log("Exit gate placed at: " + exitPos + " - Walk through to exit!");
        }
    }
    
    // Public helper methods for other systems
    public Vector3 GetStartPosition()
    {
        return mazeOrigin + new Vector3(0, 1f, 0);
    }
    
    public Vector3 GetExitPosition()
    {
        return mazeOrigin + new Vector3((width - 1) * cellSize, 1f, (height - 1) * cellSize);
    }
    
    public float GetCellSize()
    {
        return cellSize;
    }
    
    public int GetWidth()
    {
        return width;
    }
    
    public int GetHeight()
    {
        return height;
    }
    
    public bool[,] GetWalkableGrid()
    {
        bool[,] walkable = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                walkable[x, z] = true;
            }
        }
        return walkable;
    }
}