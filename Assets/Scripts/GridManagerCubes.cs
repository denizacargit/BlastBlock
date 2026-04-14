using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class GridManager
{
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
                sr.sortingOrder = GetBoardSortingOrder(cube.x, cube.y, 2);
            }
        }
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
}
