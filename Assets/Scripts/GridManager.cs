using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    public Transform gridParent;
    public RectTransform gridContentAnchor;
    public RectTransform gridBackgroundRect;
    public Transform cubesParent;
    public Transform obstaclesParent;
    public Transform rocketHintsParent;
    public Transform rocketsParent;

    [Header("Prefabs")]
    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject yellowCubePrefab;
    public GameObject redRocketHintPrefab;
    public GameObject greenRocketHintPrefab;
    public GameObject blueRocketHintPrefab;
    public GameObject yellowRocketHintPrefab;
    public GameObject horizontalRocketPrefab;
    public GameObject verticalRocketPrefab;
    public GameObject horizontalRocketLeftPartPrefab;
    public GameObject horizontalRocketRightPartPrefab;
    public GameObject verticalRocketUpPartPrefab;
    public GameObject verticalRocketDownPartPrefab;
    public GameObject boxPrefab;
    public GameObject stonePrefab; 
    public GameObject vasePrefab;  

    [Header("UI")]
    public TMPro.TextMeshProUGUI movesText;

    private LevelData currentLevelData;
    private Cube[,] allCubes;
    private Obstacle[,] allObstacles;
    private Rocket[,] allRockets;
    private float contentScale = 1f;
    private float cellWidth = 1f;
    private float cellHeight = 1f;
    private const float GridPadding = 0.35f;
    private const float FallSpeed = 8f;
    private const float RocketPartSpeed = 3f;
    private const float RocketPartLifetime = 0.8f;
    private readonly string[] cubeTypes = { "r", "g", "b", "y" };

    [Header("UI References")]
    public TMPro.TextMeshProUGUI moveCounterText; // The counter for move (get from JSON file)
    public TMPro.TextMeshProUGUI goalCounterText;
    public UnityEngine.UI.Image goalIconImage;     // The goal icon according to the level
    private int movesLeft; // This will change as we play
    private string activeGoalType;
    private int goalsLeft;
    private bool levelCompleted;
    void Start()
    {
        int selectedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        LoadLevel(Mathf.Clamp(selectedLevel, 1, 10));
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
            InitializeGoal();
            UpdateMovesUI();   // helper function to update moves
            UpdateGoalUI();

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
            CalculateCellSizeFromBoxPrefab();
            PrepareGridHierarchy();
            allCubes = new Cube[currentLevelData.grid_width, currentLevelData.grid_height];
            allObstacles = new Obstacle[currentLevelData.grid_width, currentLevelData.grid_height];
            allRockets = new Rocket[currentLevelData.grid_width, currentLevelData.grid_height];
            
            ClearGrid();
            GenerateGrid();
            ResizeBackground();
            RefreshRocketHints();
        }
        else
        {
            Debug.LogError($"Cannot find Level JSON at {filePath}");
        }
    }

    void GenerateGrid()
    {
        for (int y = 0; y < currentLevelData.grid_height; y++)
        {
            for (int x = 0; x < currentLevelData.grid_width; x++)
            {
                int index = y * currentLevelData.grid_width + x;
                string itemType = currentLevelData.grid[index];

                if (itemType == "rand")
                {
                    itemType = GetRandomCubeType();
                }

                SpawnItem(itemType, x, y);
            }
        }
    }

    string GetRandomCubeType()
    {
        return cubeTypes[Random.Range(0, cubeTypes.Length)];
    }

    void InitializeGoal()
    {
        activeGoalType = currentLevelData.goal_type;
        goalsLeft = currentLevelData.goal_count;
        levelCompleted = false;

        if (string.IsNullOrEmpty(activeGoalType))
        {
            activeGoalType = InferGoalTypeFromGrid();
        }

        if (goalsLeft <= 0)
        {
            goalsLeft = CountGoalsInGrid();
        }
    }

    string InferGoalTypeFromGrid()
    {
        bool hasObstacle = false;

        foreach (string itemType in currentLevelData.grid)
        {
            if (IsObstacleType(itemType))
            {
                hasObstacle = true;
                break;
            }
        }

        return hasObstacle ? "obstacle" : string.Empty;
    }

    int CountGoalsInGrid()
    {
        int count = 0;

        foreach (string itemType in currentLevelData.grid)
        {
            if (IsGoalType(itemType))
            {
                count++;
            }
        }

        return count;
    }

    bool IsObstacleType(string type)
    {
        return type == "bo" || type == "s" || type == "v";
    }

    bool IsCubeType(string type)
    {
        return type == "r" || type == "g" || type == "b" || type == "y";
    }

    bool IsGoalType(string type)
    {
        if (string.IsNullOrEmpty(activeGoalType))
        {
            return false;
        }

        if (activeGoalType == "obstacle")
        {
            return IsObstacleType(type);
        }

        return type == activeGoalType;
    }

    void CollectGoal(string type)
    {
        if (levelCompleted || goalsLeft <= 0 || !IsGoalType(type))
        {
            return;
        }

        goalsLeft--;
        UpdateGoalUI();

        if (goalsLeft <= 0)
        {
            CompleteLevel();
        }
    }

    void UpdateGoalUI()
    {
        if (goalCounterText != null)
        {
            goalCounterText.text = Mathf.Max(0, goalsLeft).ToString();
        }
    }

    void CompleteLevel()
    {
        if (levelCompleted)
        {
            return;
        }

        levelCompleted = true;
        int nextLevel = currentLevelData.level_number + 1;
        PlayerPrefs.SetInt("CurrentLevel", nextLevel);
        PlayerPrefs.Save();
        Debug.Log($"Level {currentLevelData.level_number} complete!");
    }

    void PrepareGridHierarchy()
    {
        if (gridParent != null && gridParent.name == "GridParent" && gridParent.parent != null && gridParent.parent.parent != null)
        {
            gridParent = gridParent.parent.parent;
        }

        if (gridParent == null)
        {
            GameObject gridObject = GameObject.Find("grid");
            if (gridObject != null)
            {
                gridParent = gridObject.transform;
            }
        }

        if (gridContentAnchor == null && gridParent != null)
        {
            foreach (RectTransform rectTransform in gridParent.GetComponentsInChildren<RectTransform>(true))
            {
                if (rectTransform.name == "GridContentAnchor")
                {
                    gridContentAnchor = rectTransform;
                    break;
                }
            }
        }

        if (gridBackgroundRect == null && gridParent != null)
        {
            gridBackgroundRect = gridParent as RectTransform;
        }

        AlignBackgroundToContentAnchor();
        cubesParent = GetOrCreateGridChild("Cubes", cubesParent);
        obstaclesParent = GetOrCreateGridChild("Obstacles", obstaclesParent);
        rocketHintsParent = GetOrCreateGridChild("RocketHints", rocketHintsParent);
        rocketsParent = GetOrCreateGridChild("Rockets", rocketsParent);
        UpdateContentParents();
    }

    void AlignBackgroundToContentAnchor()
    {
        if (gridBackgroundRect == null || gridContentAnchor == null || gridContentAnchor.parent != gridBackgroundRect)
        {
            return;
        }

        Vector2 anchorOffset = gridContentAnchor.anchoredPosition;
        if (anchorOffset == Vector2.zero)
        {
            return;
        }

        gridBackgroundRect.anchoredPosition += anchorOffset;
        gridContentAnchor.anchoredPosition = Vector2.zero;
    }

    Transform GetOrCreateGridChild(string childName, Transform current)
    {
        if (gridParent == null)
        {
            return current;
        }

        Transform child = gridParent.Find(childName);
        if (child == null && current != null && current.name == childName)
        {
            child = current;
            child.SetParent(gridParent, false);
        }

        if (child == null)
        {
            GameObject childObject = new GameObject(childName);
            child = childObject.transform;
            child.SetParent(gridParent, false);
        }

        return child;
    }

    void UpdateContentParents()
    {
        if (currentLevelData == null)
        {
            return;
        }

        contentScale = CalculateContentScale();
        ConfigureContentParent(cubesParent);
        ConfigureContentParent(obstaclesParent);
        ConfigureContentParent(rocketHintsParent);
        ConfigureContentParent(rocketsParent);
    }

    void ConfigureContentParent(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        parent.localRotation = Quaternion.identity;
        parent.localScale = Vector3.one * contentScale;

        if (gridContentAnchor != null)
        {
            parent.position = gridContentAnchor.position;
        }
        else
        {
            parent.localPosition = Vector3.zero;
        }
    }

    float CalculateContentScale()
    {
        Canvas canvas = gridBackgroundRect != null ? gridBackgroundRect.GetComponentInParent<Canvas>() : null;
        if (canvas != null)
        {
            UnityEngine.UI.CanvasScaler canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                return canvasScaler.referencePixelsPerUnit;
            }
        }

        return 100f;
    }

    void CalculateCellSizeFromBoxPrefab()
    {
        Vector2 boxSize = GetPrefabRendererSize(boxPrefab);
        cellWidth = Mathf.Max(0.1f, boxSize.x);
        cellHeight = Mathf.Max(0.1f, boxSize.y);
    }

    Vector2 GetPrefabRendererSize(GameObject prefab)
    {
        if (prefab == null)
        {
            return Vector2.zero;
        }

        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return Vector2.zero;
        }

        Vector3 scale = prefab.transform.localScale;
        Vector2 spriteSize = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.size : spriteRenderer.size;
        return new Vector2(spriteSize.x * scale.x, spriteSize.y * scale.y);
    }

    float GetGridContentWidth()
    {
        return currentLevelData.grid_width * cellWidth;
    }

    float GetGridContentHeight()
    {
        return currentLevelData.grid_height * cellHeight;
    }

    void ClearGrid()
    {
        ClearChildren(cubesParent);
        ClearChildren(obstaclesParent);
        ClearChildren(rocketHintsParent);
        ClearChildren(rocketsParent);
    }

    void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
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
            Transform targetParent = isObstacle ? obstaclesParent : cubesParent;
            GameObject item = Instantiate(prefab, targetParent);
            
            item.transform.localPosition = GetCellLocalPosition(x, y);
            item.transform.localRotation = Quaternion.identity;
            item.name = $"{type}_{x}_{y}";

            // 4. Sorting Order Ayarı (Görsel üst üste binmeleri engeller)
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Y ekseni arttıkça arkada kalması için (y * -10) mantığı kullanılabilir
                int baseOrder = (y * 10) + x;
                sr.sortingOrder = isObstacle ? baseOrder + 1 : baseOrder;
            }

            // 5. Veri Yapılarını Güncelleme (En Kritik Kısım)
            if (isObstacle)
            {
                Obstacle obstacleScript = item.GetComponent<Obstacle>();
                if (obstacleScript != null)
                {
                    obstacleScript.x = x; // Koordinatları scriptin içine de yazıyoruz
                    obstacleScript.y = y;
                    allObstacles[x, y] = obstacleScript;
                }
                // Engel olan yerde küp olamaz, bu hücreyi küp dizisinde temizle
                allCubes[x, y] = null;
                allRockets[x, y] = null;
            }
            else
            {
                Cube cubeScript = item.GetComponent<Cube>();
                if (cubeScript != null)
                {
                    cubeScript.x = x;
                    cubeScript.y = y;
                    cubeScript.color = type;
                    allCubes[x, y] = cubeScript;
                }
                // Küp olan yerde engel olamaz (başlangıç için)
                allObstacles[x, y] = null;
                allRockets[x, y] = null;
            }
        }
    }

    Cube SpawnCubeAt(string type, int x, int y, int spawnRowOffset = 0)
    {
        GameObject prefab = GetCubePrefab(type);
        if (prefab == null || cubesParent == null)
        {
            return null;
        }

        GameObject item = Instantiate(prefab, cubesParent);
        item.transform.localPosition = GetCellLocalPosition(x, y + spawnRowOffset);
        item.transform.localRotation = Quaternion.identity;
        item.name = $"{type}_{x}_{y}";

        Cube cubeScript = item.GetComponent<Cube>();
        if (cubeScript != null)
        {
            cubeScript.x = x;
            cubeScript.y = y;
            cubeScript.color = type;
            allCubes[x, y] = cubeScript;
        }

        UpdateCubeVisual(cubeScript, x, y, true);
        return cubeScript;
    }

    Rocket SpawnRocketAt(int x, int y, RocketDirection direction)
    {
        GameObject prefab = direction == RocketDirection.Horizontal ? horizontalRocketPrefab : verticalRocketPrefab;
        if (prefab == null || rocketsParent == null)
        {
            return null;
        }

        GameObject item = Instantiate(prefab, rocketsParent);
        item.transform.localPosition = GetCellLocalPosition(x, y);
        item.transform.localRotation = Quaternion.identity;
        item.name = $"rocket_{direction}_{x}_{y}";

        Rocket rocket = item.GetComponent<Rocket>();
        if (rocket != null)
        {
            rocket.x = x;
            rocket.y = y;
            rocket.direction = direction;
            allRockets[x, y] = rocket;
            allCubes[x, y] = null;
            allObstacles[x, y] = null;
            UpdateRocketVisual(rocket, x, y, false);
        }

        return rocket;
    }

    RocketDirection GetRandomRocketDirection()
    {
        return Random.value < 0.5f ? RocketDirection.Horizontal : RocketDirection.Vertical;
    }

    void UpdateRocketVisual(Rocket rocket, int x, int y, bool animate = false)
    {
        if (rocket == null)
        {
            return;
        }

        rocket.x = x;
        rocket.y = y;
        Vector3 targetPosition = GetCellLocalPosition(x, y);

        if (animate)
        {
            StartCoroutine(MoveCubeTo(rocket.transform, targetPosition));
        }
        else
        {
            rocket.transform.localPosition = targetPosition;
        }

        SpriteRenderer sr = rocket.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = (y * 10) + x + 1;
        }
    }

    GameObject GetCubePrefab(string type)
    {
        switch (type)
        {
            case "r": return redCubePrefab;
            case "g": return greenCubePrefab;
            case "b": return blueCubePrefab;
            case "y": return yellowCubePrefab;
            default: return null;
        }
    }

    GameObject GetRocketHintPrefab(string type)
    {
        switch (type)
        {
            case "r": return redRocketHintPrefab;
            case "g": return greenRocketHintPrefab;
            case "b": return blueRocketHintPrefab;
            case "y": return yellowRocketHintPrefab;
            default: return null;
        }
    }

    void RefreshRocketHints()
    {
        ClearChildren(rocketHintsParent);

        if (rocketHintsParent == null || currentLevelData == null || allCubes == null)
        {
            return;
        }

        HashSet<Cube> visited = new HashSet<Cube>();

        for (int y = 0; y < currentLevelData.grid_height; y++)
        {
            for (int x = 0; x < currentLevelData.grid_width; x++)
            {
                Cube cube = allCubes[x, y];
                if (cube == null || visited.Contains(cube))
                {
                    continue;
                }

                List<Cube> group = new List<Cube>();
                FindConnectedCubes(cube.x, cube.y, cube.color, group);

                foreach (Cube groupCube in group)
                {
                    visited.Add(groupCube);
                }

                if (group.Count >= 4)
                {
                    SpawnRocketHintsForGroup(group);
                }
            }
        }
    }

    void FindConnectedCubes(int x, int y, string color, List<Cube> connected)
    {
        if (x < 0 || x >= currentLevelData.grid_width || y < 0 || y >= currentLevelData.grid_height)
        {
            return;
        }

        Cube cube = allCubes[x, y];
        if (cube == null || connected.Contains(cube) || cube.color != color)
        {
            return;
        }

        connected.Add(cube);

        FindConnectedCubes(x + 1, y, color, connected);
        FindConnectedCubes(x - 1, y, color, connected);
        FindConnectedCubes(x, y + 1, color, connected);
        FindConnectedCubes(x, y - 1, color, connected);
    }

    void SpawnRocketHintsForGroup(List<Cube> group)
    {
        foreach (Cube cube in group)
        {
            GameObject prefab = GetRocketHintPrefab(cube.color);
            if (prefab == null)
            {
                continue;
            }

            GameObject hint = Instantiate(prefab, rocketHintsParent);
            hint.transform.localPosition = GetCellLocalPosition(cube.x, cube.y);
            hint.transform.localRotation = Quaternion.identity;
            hint.name = $"rocket_hint_{cube.color}_{cube.x}_{cube.y}";

            SpriteRenderer sr = hint.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = (cube.y * 10) + cube.x + 2;
            }
        }
    }

    void UpdateCubeVisual(Cube cube, int x, int y, bool animate = false)
    {
        if (cube == null)
        {
            return;
        }

        cube.x = x;
        cube.y = y;
        Vector3 targetPosition = GetCellLocalPosition(x, y);

        if (animate)
        {
            StartCoroutine(MoveCubeTo(cube.transform, targetPosition));
        }
        else
        {
            cube.transform.localPosition = targetPosition;
        }

        SpriteRenderer sr = cube.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = (y * 10) + x;
        }
    }

    IEnumerator MoveCubeTo(Transform cubeTransform, Vector3 targetPosition)
    {
        while (cubeTransform != null && Vector3.Distance(cubeTransform.localPosition, targetPosition) > 0.01f)
        {
            cubeTransform.localPosition = Vector3.MoveTowards(
                cubeTransform.localPosition,
                targetPosition,
                FallSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (cubeTransform != null)
        {
            cubeTransform.localPosition = targetPosition;
        }
    }

    Vector3 GetCellLocalPosition(int x, int y)
    {
        float halfWidth = (currentLevelData.grid_width - 1) * cellWidth / 2f;
        float halfHeight = (currentLevelData.grid_height - 1) * cellHeight / 2f;
        float posX = (x * cellWidth) - halfWidth;
        float posY = (y * cellHeight) - halfHeight;

        return new Vector3(posX, posY, 0f);
    }

    void ResizeBackground()
    {
        if (gridBackgroundRect == null || currentLevelData == null) return;

        float totalWidth = GetGridContentWidth() * contentScale;
        float totalHeight = GetGridContentHeight() * contentScale;
        float padding = GridPadding * contentScale;

        gridBackgroundRect.sizeDelta = new Vector2(totalWidth + padding, totalHeight + padding);
    }

    public void OnCubeClicked(Cube clickedCube)
    {
        if (levelCompleted)
        {
            return;
        }

        List<Cube> matches = new List<Cube>();
        FindMatches(clickedCube.x, clickedCube.y, clickedCube.color, matches);

        if (matches.Count >= 2)
        {
            ClearChildren(rocketHintsParent);
            DecrementMoves();
            bool shouldCreateRocket = matches.Count >= 4;
            
            // Bu sette hasar alanları tutarak aynı hamlede bir objeye birden fazla hasar gitmesini engelliyoruz
            HashSet<Obstacle> obstaclesToDamage = new HashSet<Obstacle>();
            Vector3 rocketCreatePosition = GetCellLocalPosition(clickedCube.x, clickedCube.y);

            foreach (Cube c in matches)
            {
                // Küpün 4 yanındaki komşuları tara
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (Vector2Int dir in directions)
                {
                    int nx = c.x + dir.x;
                    int ny = c.y + dir.y;

                    // Sınır kontrolü
                    if (nx >= 0 && nx < currentLevelData.grid_width && ny >= 0 && ny < currentLevelData.grid_height)
                    {
                        Obstacle obs = allObstacles[nx, ny];
                        if (obs != null)
                        {
                            obstaclesToDamage.Add(obs);
                        }
                    }
                }

                // Küpü diziden ve sahneden kaldır
                CollectGoal(c.color);
                allCubes[c.x, c.y] = null;

                if (shouldCreateRocket)
                {
                    StartCoroutine(MoveAndDestroyCube(c.transform, rocketCreatePosition));
                }
                else
                {
                    Destroy(c.gameObject);
                }
            }

            // Belirlenen obstacle'lara hasar ver
            foreach (Obstacle obs in obstaclesToDamage)
            {
                  obs.TakeDamage(); 
                
                if (obs.health <= 0)
                {
                    CollectGoal(obs.obstacleType);
                    // Eğer öldüyse referansını temizle
                    // Not: Obstacle scriptinin içinde x ve y koordinatlarını tutuyor olmalısın
                    // Eğer tutmuyorsan Obstacle class'ına da x,y eklemelisin.
                    // Şimdilik koordinatları bildiğimizi varsayalım:
                    // allObstacles[obs.x, obs.y] = null; 
                }
            }

            if (shouldCreateRocket && !levelCompleted)
            {
                SpawnRocketAt(clickedCube.x, clickedCube.y, GetRandomRocketDirection());
            }

            if (!levelCompleted)
            {
                ApplyGravity();
                RefreshRocketHints();
            }
        }
    }

    IEnumerator MoveAndDestroyCube(Transform cubeTransform, Vector3 targetPosition)
    {
        while (cubeTransform != null && Vector3.Distance(cubeTransform.localPosition, targetPosition) > 0.01f)
        {
            cubeTransform.localPosition = Vector3.MoveTowards(
                cubeTransform.localPosition,
                targetPosition,
                FallSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (cubeTransform != null)
        {
            Destroy(cubeTransform.gameObject);
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
                Obstacle obs = allObstacles[nx, ny];
                if (obs != null && !damagedList.Contains(obs))
                {
                    obs.TakeDamage();
                    damagedList.Add(obs);

                    if (obs.health <= 0)
                    {
                        allObstacles[nx, ny] = null;
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
            int targetY = 0;

            for (int y = 0; y < currentLevelData.grid_height; y++)
            {
                if (allObstacles[x, y] != null)
                {
                    targetY = y + 1;
                    continue;
                }

                Cube cube = allCubes[x, y];
                Rocket rocket = allRockets[x, y];
                if (cube == null && rocket == null)
                {
                    continue;
                }

                if (y != targetY)
                {
                    allCubes[x, targetY] = cube;
                    allRockets[x, targetY] = rocket;
                    allCubes[x, y] = null;
                    allRockets[x, y] = null;
                }

                if (cube != null)
                {
                    UpdateCubeVisual(cube, x, targetY, true);
                }

                if (rocket != null)
                {
                    UpdateRocketVisual(rocket, x, targetY, true);
                }

                targetY++;
            }

            for (int y = targetY; y < currentLevelData.grid_height; y++)
            {
                if (allObstacles[x, y] != null || allCubes[x, y] != null || allRockets[x, y] != null)
                {
                    continue;
                }

                SpawnCubeAt(GetRandomCubeType(), x, y, currentLevelData.grid_height - y);
            }
        }
    }

    public void OnRocketClicked(Rocket rocket)
    {
        if (levelCompleted || rocket == null)
        {
            return;
        }

        ClearChildren(rocketHintsParent);
        DecrementMoves();

        List<Rocket> comboRockets = new List<Rocket>();
        FindAdjacentRockets(rocket.x, rocket.y, comboRockets);

        if (comboRockets.Count >= 2)
        {
            ExplodeRocketCombo(comboRockets, rocket.x, rocket.y);
        }
        else
        {
            ExplodeRocket(rocket);
        }

        if (!levelCompleted)
        {
            ApplyGravity();
            RefreshRocketHints();
        }
    }

    void FindAdjacentRockets(int x, int y, List<Rocket> rockets)
    {
        if (x < 0 || x >= currentLevelData.grid_width || y < 0 || y >= currentLevelData.grid_height)
        {
            return;
        }

        Rocket rocket = allRockets[x, y];
        if (rocket == null || rockets.Contains(rocket))
        {
            return;
        }

        rockets.Add(rocket);

        FindAdjacentRockets(x + 1, y, rockets);
        FindAdjacentRockets(x - 1, y, rockets);
        FindAdjacentRockets(x, y + 1, rockets);
        FindAdjacentRockets(x, y - 1, rockets);
    }

    void ExplodeRocket(Rocket rocket)
    {
        if (rocket == null)
        {
            return;
        }

        int originX = rocket.x;
        int originY = rocket.y;
        RocketDirection direction = rocket.direction;

        RemoveRocket(rocket);
        DamageCell(originX, originY, null);

        if (direction == RocketDirection.Horizontal)
        {
            SpawnRocketPart(originX, originY, Vector2Int.left, horizontalRocketLeftPartPrefab);
            SpawnRocketPart(originX, originY, Vector2Int.right, horizontalRocketRightPartPrefab);
            DamageLine(originX, originY, Vector2Int.left);
            DamageLine(originX, originY, Vector2Int.right);
        }
        else
        {
            SpawnRocketPart(originX, originY, Vector2Int.down, verticalRocketDownPartPrefab);
            SpawnRocketPart(originX, originY, Vector2Int.up, verticalRocketUpPartPrefab);
            DamageLine(originX, originY, Vector2Int.down);
            DamageLine(originX, originY, Vector2Int.up);
        }
    }

    void ExplodeRocketCombo(List<Rocket> comboRockets, int originX, int originY)
    {
        foreach (Rocket rocket in comboRockets)
        {
            RemoveRocket(rocket);
        }

        for (int y = originY - 1; y <= originY + 1; y++)
        {
            if (y < 0 || y >= currentLevelData.grid_height)
            {
                continue;
            }

            SpawnRocketPart(originX, y, Vector2Int.left, horizontalRocketLeftPartPrefab);
            SpawnRocketPart(originX, y, Vector2Int.right, horizontalRocketRightPartPrefab);
            DamageCell(originX, y, null);
            DamageLine(originX, y, Vector2Int.left);
            DamageLine(originX, y, Vector2Int.right);
        }

        for (int x = originX - 1; x <= originX + 1; x++)
        {
            if (x < 0 || x >= currentLevelData.grid_width)
            {
                continue;
            }

            SpawnRocketPart(x, originY, Vector2Int.down, verticalRocketDownPartPrefab);
            SpawnRocketPart(x, originY, Vector2Int.up, verticalRocketUpPartPrefab);
            DamageCell(x, originY, null);
            DamageLine(x, originY, Vector2Int.down);
            DamageLine(x, originY, Vector2Int.up);
        }
    }

    void DamageLine(int startX, int startY, Vector2Int direction)
    {
        int x = startX + direction.x;
        int y = startY + direction.y;

        while (x >= 0 && x < currentLevelData.grid_width && y >= 0 && y < currentLevelData.grid_height)
        {
            DamageCell(x, y, null);
            x += direction.x;
            y += direction.y;
        }
    }

    void DamageCell(int x, int y, Rocket sourceRocket)
    {
        if (x < 0 || x >= currentLevelData.grid_width || y < 0 || y >= currentLevelData.grid_height)
        {
            return;
        }

        Rocket rocket = allRockets[x, y];
        if (rocket != null && rocket != sourceRocket)
        {
            ExplodeRocket(rocket);
            return;
        }

        Obstacle obstacle = allObstacles[x, y];
        if (obstacle != null)
        {
            obstacle.TakeDamage();

            if (obstacle.health <= 0)
            {
                CollectGoal(obstacle.obstacleType);
                allObstacles[x, y] = null;
            }

            return;
        }

        Cube cube = allCubes[x, y];
        if (cube != null)
        {
            CollectGoal(cube.color);
            allCubes[x, y] = null;
            Destroy(cube.gameObject);
        }
    }

    void RemoveRocket(Rocket rocket)
    {
        if (rocket == null)
        {
            return;
        }

        if (rocket.x >= 0 && rocket.x < currentLevelData.grid_width && rocket.y >= 0 && rocket.y < currentLevelData.grid_height)
        {
            allRockets[rocket.x, rocket.y] = null;
        }

        Destroy(rocket.gameObject);
    }

    void SpawnRocketPart(int x, int y, Vector2Int direction, GameObject prefab)
    {
        if (prefab == null || rocketsParent == null)
        {
            return;
        }

        GameObject part = Instantiate(prefab, rocketsParent);
        part.transform.localPosition = GetCellLocalPosition(x, y);
        part.transform.localRotation = Quaternion.identity;

        RocketPart rocketPart = part.GetComponent<RocketPart>();
        if (rocketPart != null)
        {
            rocketPart.Initialize(new Vector3(direction.x * cellWidth, direction.y * cellHeight, 0f), RocketPartSpeed, RocketPartLifetime);
        }
    }

    public void ClearObstacleAt(int x, int y)
    {
        if (x >= 0 && x < currentLevelData.grid_width && y >= 0 && y < currentLevelData.grid_height)
        {
            allObstacles[x, y] = null;
        }
    }
}
