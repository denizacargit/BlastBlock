using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    public Transform gridParent;
    public RectTransform gridBackgroundRect; 
    public Transform cubesParent;     // Drag "Cubes" empty object here
    public Transform obstaclesParent; // Drag "Obstacles" empty object here

    [Header("Prefabs")]
    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject yellowCubePrefab;
    public GameObject boxPrefab;
    public GameObject stonePrefab; 
    public GameObject vasePrefab;  

    [Header("UI")]
    public TMPro.TextMeshProUGUI movesText;

    private LevelData currentLevelData;
    private Cube[,] allCubes; 
    private float spacing = 0.55f; 

    [Header("UI References")]
    public TMPro.TextMeshProUGUI moveCounterText; // The counter for move (get from JSON file)
    public UnityEngine.UI.Image goalIconImage;     // The goal icon according to the level
    void Start()
    {
        LoadLevel(1); 
    }

    public void LoadLevel(int levelNumber)
    {
        string filePath = "Levels/level_" + levelNumber.ToString("D2"); 
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

        if (jsonFile != null)
        {
            currentLevelData = JsonUtility.FromJson<LevelData>(jsonFile.text);

            // 1. Update the Move Counter (The number in the right box)
            if(moveCounterText != null)
            {
                moveCounterText.text = currentLevelData.move_count.ToString();
            }
            

            if (goalIconImage != null)
            {
                // Load the sprite for the goal icon based on the JSON goal type
                // This assumes you have sprites in Resources/Sprites/Icons named like "r_icon"
                Sprite goalSprite = Resources.Load<Sprite>("Sprites/Icons/" + currentLevelData.goal_type + "_icon");
                if (goalSprite != null)
                {
                    goalIconImage.sprite = goalSprite;
                }
            }
            
            // 3. Initialize Grid Logic
            allCubes = new Cube[currentLevelData.grid_width, currentLevelData.grid_height];
            
            GenerateGrid();
            ResizeBackground();
        }
        else
        {
            Debug.LogError($"Cannot find Level JSON at {filePath}");
        }
    }

    void GenerateGrid()
    {
        int index = 0;
        string[] colors = { "r", "g", "b", "y" };

        for (int y = 0; y < currentLevelData.grid_height; y++)
        {
            for (int x = 0; x < currentLevelData.grid_width; x++)
            {
                string itemType = currentLevelData.grid[index];

                if (itemType == "rand") 
                {
                    itemType = colors[Random.Range(0, colors.Length)];
                }

                SpawnItem(itemType, x, y);
                index++;
            }
        }
    }

    void SpawnItem(string type, int x, int y)
    {
        GameObject prefab = null;
        bool isObstacle = false;

        switch (type)
        {
            case "r": prefab = redCubePrefab; break;
            case "g": prefab = greenCubePrefab; break;
            case "b": prefab = blueCubePrefab; break;
            case "y": prefab = yellowCubePrefab; break;
            case "bo": prefab = boxPrefab; isObstacle = true; break;
            case "s": prefab = stonePrefab; isObstacle = true; break;
            case "v": prefab = vasePrefab; isObstacle = true; break;
        }

        if (prefab != null)
        {
            float posX = (x - (currentLevelData.grid_width / 2f) + 0.5f) * spacing;
            float posY = (-(y - (currentLevelData.grid_height / 2f)) - 0.5f) * spacing + 0.5f;

            Vector3 position = new Vector3(posX, posY, 0);
            
            // Assign to the correct folder in the hierarchy
            Transform targetParent = isObstacle ? obstaclesParent : cubesParent;
            GameObject item = Instantiate(prefab, position, Quaternion.identity, targetParent);
            
            item.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            item.GetComponent<SpriteRenderer>().sortingOrder = ((20 - y) * 10) + x;
            item.name = $"{type}_{x}_{y}";

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

    void ResizeBackground()
    {
        if (gridBackgroundRect == null || currentLevelData == null) return;

        float ppu = 100f; 
        float totalWidth = currentLevelData.grid_width * spacing;
        float totalHeight = currentLevelData.grid_height * spacing;
        float padding = 0.5f;

        gridBackgroundRect.sizeDelta = new Vector2((totalWidth + padding) * ppu, (totalHeight + padding) * ppu);
        gridBackgroundRect.anchoredPosition = new Vector2(0, 0.5f * ppu);
    }

    public void OnCubeClicked(Cube clickedCube)
    {
        List<Cube> matches = new List<Cube>();
        FindMatches(clickedCube.x, clickedCube.y, clickedCube.color, matches);

        if (matches.Count >= 2)
        {
            // Use a HashSet to ensure we don't damage the same obstacle twice in one click
            HashSet<Obstacle> damagedObstacles = new HashSet<Obstacle>();

            foreach (Cube c in matches)
            {
                CheckForObstacleNeighbors(c.x, c.y, damagedObstacles);
                allCubes[c.x, c.y] = null;
                Destroy(c.gameObject);
            }
            ApplyGravity();
        }
    }

    void CheckForObstacleNeighbors(int x, int y, HashSet<Obstacle> damagedList)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (nx >= 0 && nx < currentLevelData.grid_width && ny >= 0 && ny < currentLevelData.grid_height)
            {
                // Calculate position to look for a collider
                float checkX = (nx - (currentLevelData.grid_width / 2f) + 0.5f) * spacing;
                float checkY = (-(ny - (currentLevelData.grid_height / 2f)) - 0.5f) * spacing + 0.5f;

                Collider2D hit = Physics2D.OverlapPoint(new Vector2(checkX, checkY));
                if (hit != null)
                {
                    Obstacle obs = hit.GetComponent<Obstacle>();
                    if (obs != null && !damagedList.Contains(obs))
                    {
                        obs.TakeDamage();
                        damagedList.Add(obs);
                    }
                }
            }
        }
    }

    void FindMatches(int x, int y, string color, List<Cube> matches)
    {
        if (x < 0 || x >= currentLevelData.grid_width || y < 0 || y >= currentLevelData.grid_height) return;

        Cube cube = allCubes[x, y];
        if (cube == null || matches.Contains(cube) || cube.color != color) return;

        matches.Add(cube);

        FindMatches(x + 1, y, color, matches);
        FindMatches(x - 1, y, color, matches);
        FindMatches(x, y + 1, color, matches);
        FindMatches(x, y - 1, color, matches);
    }

    public void ApplyGravity()
    {
        for (int x = 0; x < currentLevelData.grid_width; x++)
        {
            for (int y = currentLevelData.grid_height - 1; y >= 0; y--)
            {
                if (allCubes[x, y] == null) 
                {
                    for (int nextY = y - 1; nextY >= 0; nextY--)
                    {
                        if (allCubes[x, nextY] != null)
                        {
                            Cube cubeToMove = allCubes[x, nextY];
                            allCubes[x, y] = cubeToMove;
                            allCubes[x, nextY] = null;

                            cubeToMove.y = y;
                            float targetY = (-(y - (currentLevelData.grid_height / 2f)) - 0.5f) * spacing + 0.5f;
                            cubeToMove.transform.position = new Vector3(cubeToMove.transform.position.x, targetY, 0);
                            break;
                        }
                    }
                }
            }
        }
    }
}