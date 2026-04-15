using UnityEngine;
using System.Collections.Generic;

public partial class GridManager : MonoBehaviour
{
    [Header("References")]
    public Transform gridParent;
    public RectTransform gridContentAnchor;
    public RectTransform gridBackgroundRect;
    public Transform cubesParent;
    public Transform obstaclesParent;
    public Transform rocketHintsParent;
    public Transform rocketsParent;
    public Transform effectsParent;
    public GameObject celebrationPrefab;
    public GameObject failPopup;

    [Header("Grid Layout")]
    public float gridVisualScale = 180f;

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
    public GameObject smallStarRocketTrailPrefab;
    public GameObject bigStarRocketTrailPrefab;
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;

    [Header("Particle Prefabs")]
    public GameObject[] boxShardPrefabs;
    public GameObject[] stoneShardPrefabs;
    public GameObject[] vaseShardPrefabs;
    public GameObject redCubeParticlePrefab;
    public GameObject greenCubeParticlePrefab;
    public GameObject blueCubeParticlePrefab;
    public GameObject yellowCubeParticlePrefab;

    [Header("Particle Settings")]
    public int cubeParticleCount = 13;
    public float cubeParticleScale = 0.22f;
    public float particleLifetime = 0.6f;
    public int rocketTrailStarsPerCell = 25;
    public float smallRocketTrailStarScale = 0.08f;
    public float bigRocketTrailStarScale = 0.095f;
    public float rocketTrailJitter = 0.045f;
    public float rocketTrailDestroyInterval = 0.00025f;
    public int rocketTrailDestroyBatchSize = 15;

    [Header("UI")]
    public TMPro.TextMeshProUGUI movesText;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI moveCounterText;
    public TMPro.TextMeshProUGUI goalCounterText;
    public UnityEngine.UI.Image goalIconImage;
    public Transform goalSlotsParent;
    public GoalSlotUI goalSlotPrefab;
    public Sprite genericObstacleGoalIcon;

    private LevelData currentLevelData;
    private Cube[,] allCubes;
    private Obstacle[,] allObstacles;
    private Rocket[,] allRockets;
    private float contentScale = 1f;
    private float cellWidth = 1f;
    private float cellHeight = 1f;
    private int movesLeft;
    private readonly List<GoalState> activeGoals = new List<GoalState>();
    private bool levelCompleted;

    private const float GridPadding = 0.33f;
    private const float FallSpeed = 8f;
    private const float RocketPartSpeed = 9f;
    private const float RocketPartLifetime = 0.8f;
    private const float WinReturnDelay = 1.5f;
    private const int BoardSortingBase = 100;
    private readonly string[] cubeTypes = { "r", "g", "b", "y" };
    private readonly string[] obstacleGoalTypes = { "s", "bo", "v" };

    // Loads the selected level on scene start.
    void Start()
    {
        int selectedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        LoadLevel(Mathf.Clamp(selectedLevel, 1, 10));
    }

    // Loads one level and rebuilds the board.
    public void LoadLevel(int levelNumber)
    {
        string filePath = "Levels/level_" + levelNumber.ToString("D2");
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

        if (jsonFile != null)
        {
            currentLevelData = JsonUtility.FromJson<LevelData>(jsonFile.text);
            levelCompleted = false;
            movesLeft = currentLevelData.move_count;
            InitializeGoals();
            UpdateMovesUI();
            HideFailPopup();

            if (moveCounterText != null)
            {
                moveCounterText.text = currentLevelData.move_count.ToString();
            }

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
}
