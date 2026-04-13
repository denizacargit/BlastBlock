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
    private float spacing = 0.48f; 

    [Header("UI References")]
    public TMPro.TextMeshProUGUI moveCounterText; // The counter for move (get from JSON file)
    public UnityEngine.UI.Image goalIconImage;     // The goal icon according to the level
    private int movesLeft; // This will change as we play
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

            //Initialized the counter
            movesLeft = currentLevelData.move_count;
            UpdateMovesUI();   // helper function to update moves

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
        string[] colors = { "r", "g", "b", "y" };
        int totalCells = currentLevelData.grid_width * currentLevelData.grid_height;

        for (int y = 0; y < currentLevelData.grid_height; y++)
        {
            for (int x = 0; x < currentLevelData.grid_width; x++)
            {
                int normalIndex = y * currentLevelData.grid_width + x;
                int reversedIndex = totalCells - 1 - normalIndex;

                string itemType = currentLevelData.grid[reversedIndex];

                if (itemType == "rand")
                {
                    itemType = colors[Random.Range(0, colors.Length)];
                }

                SpawnItem(itemType, x, y);
            }
        }
    }

    void SpawnItem(string type, int x, int y)
    {
        GameObject prefab = null;
        bool isObstacle = false;

        // 1. Determine the Prefab Type
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
            // 2. Centering Math
            // We find the total width/height and divide by 2 to find the offset from center (0,0)
            float halfWidth = (currentLevelData.grid_width - 1) * spacing / 2f;
            float halfHeight = (currentLevelData.grid_height - 1) * spacing / 2f;

            // posX: Starts negative (left) and moves positive (right) as x increases
            float posX = (x * spacing) - halfWidth;
            
            // posY: Starts positive (top) and moves negative (bottom) as y increases
            // This ensures JSON index 0 is at the top and index 8 is at the bottom
            float posY = halfHeight - (y * spacing);

            Vector3 position = new Vector3(posX, posY, 0);

            // 3. Spawning
            Transform targetParent = isObstacle ? obstaclesParent : cubesParent;
            GameObject item = Instantiate(prefab, position, Quaternion.identity, targetParent);

            // 4. Visual Adjustments
            item.transform.localScale = new Vector3(0.34f, 0.34f, 1f);

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Higher y (bottom of screen) should have lower sorting order 
                // so cubes "behind" stay behind cubes "in front"
                int baseOrder = ((currentLevelData.grid_height - y) * 10) + x;
                sr.sortingOrder = isObstacle ? baseOrder - 1 : baseOrder;
            }

            item.name = $"{type}_{x}_{y}";

            // 5. Data Link
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
            DecrementMoves();   // decrease moves as we play
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

    void DecrementMoves()
    {
        movesLeft--;

        UpdateMovesUI();

        if (movesLeft <= 0)
        {
            Debug.Log("Out of moves! Game over");
            // Trigger "Game over" UI in here.
        
        }
    }

    void UpdateMovesUI()
    {
        if (moveCounterText != null)
        {
            moveCounterText.text = movesLeft.ToString();
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

                            float targetY = ((currentLevelData.grid_height / 2f) - y - 0.5f) * spacing + 0.5f;
                            cubeToMove.transform.position = new Vector3(
                                cubeToMove.transform.position.x,
                                targetY,
                                0
                            );

                            SpriteRenderer sr = cubeToMove.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                int baseOrder = ((currentLevelData.grid_height - y) * 10) + x;
                                sr.sortingOrder = baseOrder;
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}