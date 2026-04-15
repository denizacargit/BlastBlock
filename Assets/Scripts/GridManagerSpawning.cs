using UnityEngine;
using System.Collections;

public partial class GridManager
{
    // Builds the initial board from level data.
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

    // Picks one of the playable cube colors.
    string GetRandomCubeType()
    {
        return cubeTypes[Random.Range(0, cubeTypes.Length)];
    }

    // Checks whether a token is a cube.
    bool IsCubeType(string type)
    {
        return type == "r" || type == "g" || type == "b" || type == "y";
    }

    // Removes all board visuals.
    void ClearGrid()
    {
        ClearChildren(cubesParent);
        ClearChildren(obstaclesParent);
        ClearChildren(rocketHintsParent);
        ClearChildren(rocketsParent);
        ClearChildren(effectsParent);
    }

    // Removes every child under a parent.
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

    // Creates the correct item for one grid cell.
    void SpawnItem(string type, int x, int y)
    {
        if (type == "hro")
        {
            SpawnRocketAt(x, y, RocketDirection.Horizontal);
            return;
        }

        if (type == "vro")
        {
            SpawnRocketAt(x, y, RocketDirection.Vertical);
            return;
        }

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

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = GetBoardSortingOrder(x, y, isObstacle ? 1 : 0);
            }

            if (isObstacle)
            {
                Obstacle obstacleScript = item.GetComponent<Obstacle>();
                if (obstacleScript != null)
                {
                    obstacleScript.x = x;
                    obstacleScript.y = y;
                    allObstacles[x, y] = obstacleScript;
                }
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
                allObstacles[x, y] = null;
                allRockets[x, y] = null;
            }
        }
    }

    // Spawns a new cube above the board when needed.
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

    // Creates a rocket at a grid cell.
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

    // Chooses a rocket direction.
    RocketDirection GetRandomRocketDirection()
    {
        return Random.value < 0.5f ? RocketDirection.Horizontal : RocketDirection.Vertical;
    }

    // Updates a rocket's grid and visual position.
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
            sr.sortingOrder = GetBoardSortingOrder(x, y, 1);
        }
    }

    // Updates a movable obstacle's grid and visual position.
    void UpdateObstacleVisual(Obstacle obstacle, int x, int y, bool animate = false)
    {
        if (obstacle == null)
        {
            return;
        }

        obstacle.x = x;
        obstacle.y = y;
        Vector3 targetPosition = GetCellLocalPosition(x, y);

        if (animate)
        {
            StartCoroutine(MoveCubeTo(obstacle.transform, targetPosition));
        }
        else
        {
            obstacle.transform.localPosition = targetPosition;
        }

        SpriteRenderer sr = obstacle.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = GetBoardSortingOrder(x, y, 1);
        }
    }

    // Returns the prefab for a cube token.
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

    // Returns the hint prefab for a cube color.
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

    // Updates a cube's grid and visual position.
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
            sr.sortingOrder = GetBoardSortingOrder(x, y);
        }
    }

    // Slides a board item into place.
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
}
