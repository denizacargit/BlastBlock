using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GridManager : MonoBehaviour
{
    public Transform gridParent; // Drag 'GridParent' here
    public GameObject redCubePrefab, greenCubePrefab, blueCubePrefab, yellowCubePrefab, boxPrefab; // We will link these soon

    private LevelData currentLevelData;

    void Start()
    {
        LoadLevel(1); // For now, let's just force-load Level 1
    }

    void LoadLevel(int levelNumber)
{
    // Matches your folder: Assets/Resources/Levels/level_1
    // No leading zero, just the number.
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
        // Your JSON is 9 wide and 10 high
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
        // Try increasing the multiplier (1.1f adds a small gap)
        float spacing = 1.1f; 
        float posX = (x - 4f) * spacing; 
        float posY = (-y + 4.5f) * spacing;

        Vector3 position = new Vector3(posX, posY, 0);
        GameObject item = Instantiate(prefab, position, Quaternion.identity);
        
        // This puts the cube inside the GridParent folder in the Hierarchy
        item.transform.SetParent(gridParent);
        
        // Rename it so you can see the coordinates in the Hierarchy (helpful for debugging)
        item.name = $"Cube_{x}_{y}";
    }
}
}