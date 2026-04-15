using UnityEngine;
using System.Collections.Generic;

public partial class GridManager
{
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
            DamageObstacle(obstacle);
            return;
        }

        Cube cube = allCubes[x, y];
        if (cube != null)
        {
            CollectGoal(cube.color);
            allCubes[x, y] = null;
            SpawnCubeParticles(cube.color, GetCellLocalPosition(x, y), x, y);
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
}
