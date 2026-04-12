using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Transform gridParent;
    public GameObject redCubePrefab, greenCubePrefab, blueCubePrefab, yellowCubePrefab, boxPrefab;

    private LevelData currentLevelData;
    
    // This is our "Mental Map" of the grid. [9 columns, 10 rows]
    public Cube[,] allCubes = new Cube[9, 10]; 

    void Start()
    {
        LoadLevel(1); 
    }

    void LoadLevel(int levelNumber)
    {
        string filePath = "Levels/level_" + levelNumber; 
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

        if (jsonFile != null)
        {
            currentLevelData = JsonUtility.FromJson<LevelData>(jsonFile.text);
            GenerateGrid();
        }
        else
        {
            Debug.LogError("Cannot find Level JSON at " + filePath);
        }
    }

    void GenerateGrid()
    {
        int index = 0;
        for (int y = 0; y < currentLevelData.grid_height; y++)
        {
            for (int x = 0; x < currentLevelData.grid_width; x++)
            {
                string itemType = currentLevelData.grid[index];
                SpawnItem(itemType, x, y);
                index++;
            }
        }
    }

    void SpawnItem(string type, int x, int y)
    {
        GameObject prefab = null;
        switch (type)
        {
            case "r": prefab = redCubePrefab; break;
            case "g": prefab = greenCubePrefab; break;
            case "b": prefab = blueCubePrefab; break;
            case "y": prefab = yellowCubePrefab; break;
            case "bo": prefab = boxPrefab; break;
        }

        if (prefab != null)
        {
            // 1. Fixed Spacing (This matches the size of a standard 1-unit sprite)
            float spacing = 1.0f; 

            // 2. Calculate Center Offset
            // (x - 4.0f) centers 9 columns (0 to 8)
            // (y - 4.5f) centers 10 rows (0 to 9)
            float posX = (x - 4.0f) * spacing; 
            float posY = (-(y - 4.5f)) * spacing; // Invert Y so 0 is top

            // 3. Move the whole grid down slightly to clear the Purple Header
            posY -= 1.5f; 

            Vector3 position = new Vector3(posX, posY, 0);
            GameObject item = Instantiate(prefab, position, Quaternion.identity);
            item.transform.SetParent(gridParent);
            
            // 4. FIX THE OVERLAP: Set a clean sorting order
            item.GetComponent<SpriteRenderer>().sortingOrder = (y * 10) + x;

            // 5. Link the Script
            Cube cubeScript = item.GetComponent<Cube>();
            if (cubeScript != null)
            {
                cubeScript.x = x;
                cubeScript.y = y;
                cubeScript.color = type;
                allCubes[x, y] = cubeScript;
            }
        }
    }

    // --- NEW LOGIC: CLICKING AND MATCHING ---

    public void OnCubeClicked(Cube clickedCube)
    {
        // 1. Create a list to hold all the cubes in the group
        List<Cube> matches = new List<Cube>();

        // 2. Use "Flood Fill" to find all touching cubes of the same color
        FindMatches(clickedCube.x, clickedCube.y, clickedCube.color, matches);

        // 3. Toon Blast rule: Only blast if the group has 2 or more
        if (matches.Count >= 2)
        {
            foreach (Cube c in matches)
            {
                // Remove from our mental map
                allCubes[c.x, c.y] = null;
                // Remove from the screen
                Destroy(c.gameObject);
            }
            
            // TODO: Add "ApplyGravity()" here next to make cubes fall!
            Debug.Log($"Blasted {matches.Count} cubes!");
        }
    }

    // This looks at neighbors recursively to find the group
    void FindMatches(int x, int y, string color, List<Cube> matches)
    {
        // Boundary checks
        if (x < 0 || x >= 9 || y < 0 || y >= 10) return;

        Cube cube = allCubes[x, y];

        // Stop if: No cube here, already in our list, or wrong color
        if (cube == null || matches.Contains(cube) || cube.color != color) return;

        // Add this cube to the group
        matches.Add(cube);

        // Check all 4 neighbors (Up, Down, Left, Right)
        FindMatches(x + 1, y, color, matches);
        FindMatches(x - 1, y, color, matches);
        FindMatches(x, y + 1, color, matches);
        FindMatches(x, y - 1, color, matches);
    }
}