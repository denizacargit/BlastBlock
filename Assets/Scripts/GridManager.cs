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
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;

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
    private const float RocketPartSpeed = 3f;
    private const float RocketPartLifetime = 0.8f;
    private const float WinReturnDelay = 1.5f;
    private const int BoardSortingBase = 100;
    private readonly string[] cubeTypes = { "r", "g", "b", "y" };
    private readonly string[] obstacleGoalTypes = { "s", "bo", "v" };

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
