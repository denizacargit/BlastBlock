using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public partial class GridManager
{
    // Starts rocket or combo explosion.
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
        StartCoroutine(HandleRocketExplosion(comboRockets, rocket));
    }

    // Runs the rocket explosion flow.
    IEnumerator HandleRocketExplosion(List<Rocket> comboRockets, Rocket clickedRocket)
    {
        if (comboRockets.Count >= 2)
        {
            yield return ExplodeRocketCombo(comboRockets, clickedRocket.x, clickedRocket.y);
        }
        else
        {
            yield return ExplodeRocket(clickedRocket);
        }

        if (!levelCompleted)
        {
            ApplyGravity();
            RefreshRocketHints();
        }
    }

    // Finds rockets connected to the tapped one.
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

    // Splits a rocket into two moving parts.
    IEnumerator ExplodeRocket(Rocket rocket)
    {
        if (rocket == null)
        {
            yield break;
        }

        int originX = rocket.x;
        int originY = rocket.y;
        RocketDirection direction = rocket.direction;

        RemoveRocket(rocket);
        DamageCell(originX, originY, null);

        if (direction == RocketDirection.Horizontal)
        {
            Coroutine left = StartCoroutine(AnimateRocketLine(originX, originY, Vector2Int.left, horizontalRocketLeftPartPrefab));
            Coroutine right = StartCoroutine(AnimateRocketLine(originX, originY, Vector2Int.right, horizontalRocketRightPartPrefab));
            yield return left;
            yield return right;
        }
        else
        {
            Coroutine down = StartCoroutine(AnimateRocketLine(originX, originY, Vector2Int.down, verticalRocketDownPartPrefab));
            Coroutine up = StartCoroutine(AnimateRocketLine(originX, originY, Vector2Int.up, verticalRocketUpPartPrefab));
            yield return down;
            yield return up;
        }
    }

    // Creates the cross-shaped rocket combo.
    IEnumerator ExplodeRocketCombo(List<Rocket> comboRockets, int originX, int originY)
    {
        foreach (Rocket rocket in comboRockets)
        {
            RemoveRocket(rocket);
        }

        List<Coroutine> activeLines = new List<Coroutine>();
        HashSet<Vector2Int> damagedCells = new HashSet<Vector2Int>();
        int comboTrailStars = Mathf.Max(0, comboRocketTrailStarsPerCell);

        for (int y = originY - 1; y <= originY + 1; y++)
        {
            if (y < 0 || y >= currentLevelData.grid_height)
            {
                continue;
            }

            DamageCellOnce(originX, y, damagedCells);
            activeLines.Add(StartCoroutine(AnimateRocketLine(originX, y, Vector2Int.left, horizontalRocketLeftPartPrefab, damagedCells, comboTrailStars)));
            activeLines.Add(StartCoroutine(AnimateRocketLine(originX, y, Vector2Int.right, horizontalRocketRightPartPrefab, damagedCells, comboTrailStars)));
        }

        for (int x = originX - 1; x <= originX + 1; x++)
        {
            if (x < 0 || x >= currentLevelData.grid_width)
            {
                continue;
            }

            DamageCellOnce(x, originY, damagedCells);
            activeLines.Add(StartCoroutine(AnimateRocketLine(x, originY, Vector2Int.down, verticalRocketDownPartPrefab, damagedCells, comboTrailStars)));
            activeLines.Add(StartCoroutine(AnimateRocketLine(x, originY, Vector2Int.up, verticalRocketUpPartPrefab, damagedCells, comboTrailStars)));
        }

        foreach (Coroutine line in activeLines)
        {
            yield return line;
        }
    }

    // Moves one rocket half across the board.
    IEnumerator AnimateRocketLine(int startX, int startY, Vector2Int direction, GameObject prefab, HashSet<Vector2Int> damagedCells = null, int trailStarsPerCell = -1)
    {
        GameObject part = SpawnRocketPart(startX, startY, prefab);
        if (part == null)
        {
            yield break;
        }

        List<GameObject> trailStars = new List<GameObject>();
        Vector3 previousPosition = GetCellLocalPosition(startX, startY);
        int x = startX + direction.x;
        int y = startY + direction.y;

        while (x >= 0 && x < currentLevelData.grid_width && y >= 0 && y < currentLevelData.grid_height)
        {
            SetRocketPartSortingOrder(part, x, y);
            DamageCellOnce(x, y, damagedCells);

            Vector3 targetPosition = GetCellLocalPosition(x, y);
            SpawnRocketTrailSegment(previousPosition, targetPosition, direction, trailStars, x, y, trailStarsPerCell);
            while (part != null && Vector3.Distance(part.transform.localPosition, targetPosition) > 0.01f)
            {
                part.transform.localPosition = Vector3.MoveTowards(
                    part.transform.localPosition,
                    targetPosition,
                    RocketPartSpeed * Time.deltaTime
                );
                yield return null;
            }

            if (part == null)
            {
                yield return DestroyRocketTrail(trailStars);
                yield break;
            }

            part.transform.localPosition = targetPosition;
            previousPosition = targetPosition;
            x += direction.x;
            y += direction.y;
        }

        Vector3 exitPosition = previousPosition + new Vector3(direction.x * cellWidth * 0.5f, direction.y * cellHeight * 0.5f, 0f);
        SpawnRocketTrailSegment(previousPosition, exitPosition, direction, trailStars, Mathf.Clamp(x - direction.x, 0, currentLevelData.grid_width - 1), Mathf.Clamp(y - direction.y, 0, currentLevelData.grid_height - 1), trailStarsPerCell);
        while (part != null && Vector3.Distance(part.transform.localPosition, exitPosition) > 0.01f)
        {
            part.transform.localPosition = Vector3.MoveTowards(
                part.transform.localPosition,
                exitPosition,
                RocketPartSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (part != null)
        {
            Destroy(part);
        }

        yield return DestroyRocketTrail(trailStars);
    }

    // Damages a combo cell only once.
    void DamageCellOnce(int x, int y, HashSet<Vector2Int> damagedCells)
    {
        if (damagedCells == null)
        {
            DamageCell(x, y, null);
            return;
        }

        Vector2Int cell = new Vector2Int(x, y);
        if (!damagedCells.Add(cell))
        {
            return;
        }

        DamageCell(x, y, null);
    }

    // Damages one board cell.
    void DamageCell(int x, int y, Rocket sourceRocket)
    {
        if (x < 0 || x >= currentLevelData.grid_width || y < 0 || y >= currentLevelData.grid_height)
        {
            return;
        }

        Rocket rocket = allRockets[x, y];
        if (rocket != null && rocket != sourceRocket)
        {
            StartCoroutine(ExplodeRocket(rocket));
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

    // Removes a rocket from the grid.
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

    // Creates one visual rocket half.
    GameObject SpawnRocketPart(int x, int y, GameObject prefab)
    {
        if (prefab == null || rocketsParent == null)
        {
            return null;
        }

        GameObject part = Instantiate(prefab, rocketsParent);
        part.transform.localPosition = GetCellLocalPosition(x, y);
        part.transform.localRotation = Quaternion.identity;
        part.SetActive(true);
        SetRocketPartSortingOrder(part, x, y);

        RocketPart rocketPart = part.GetComponent<RocketPart>();
        if (rocketPart != null)
        {
            rocketPart.enabled = false;
        }

        return part;
    }

    // Keeps rocket halves above the board.
    void SetRocketPartSortingOrder(GameObject part, int x, int y)
    {
        if (part == null)
        {
            return;
        }

        int sortingOrder = GetBoardSortingOrder(x, y, 20);
        SpriteRenderer[] renderers = part.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = sortingOrder;
        }
    }

    // Places star trail along one segment.
    void SpawnRocketTrailSegment(Vector3 start, Vector3 end, Vector2Int direction, List<GameObject> trailStars, int x, int y, int starsPerCell = -1)
    {
        int starCount = starsPerCell >= 0 ? starsPerCell : rocketTrailStarsPerCell;
        if (rocketsParent == null || starCount <= 0)
        {
            return;
        }

        Sprite bigStarSprite = GetRocketTrailStarSprite(bigStarRocketTrailPrefab);
        if (bigStarSprite == null && bigStarRocketTrailPrefab == null)
        {
            return;
        }

        Vector3 perpendicular = direction.x != 0 ? Vector3.up : Vector3.right;
        int sortingOrder = GetBoardSortingOrder(x, y, 19);

        for (int i = 0; i < starCount; i++)
        {
            float t = (i + Random.Range(0.15f, 0.85f)) / starCount;
            Vector3 position = Vector3.Lerp(start, end, t);
            position += perpendicular * Random.Range(-rocketTrailJitter, rocketTrailJitter);

            GameObject star = CreateRocketTrailStar(bigStarSprite != null ? null : bigStarRocketTrailPrefab, bigStarSprite);

            if (star == null)
            {
                continue;
            }

            star.transform.localPosition = position;
            star.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            star.transform.localScale = Vector3.one * bigRocketTrailStarScale * Random.Range(0.75f, 1.25f);

            SpriteRenderer[] renderers = star.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.sortingOrder = sortingOrder;
            }

            trailStars.Add(star);
        }
    }

    // Creates one trail star.
    GameObject CreateRocketTrailStar(GameObject spritePrefab, Sprite starSprite)
    {
        if (spritePrefab != null)
        {
            return Instantiate(spritePrefab, rocketsParent);
        }

        GameObject star = new GameObject("rocket_trail_star");
        star.transform.SetParent(rocketsParent, false);
        SpriteRenderer spriteRenderer = star.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = starSprite;
        return star;
    }

    // Reads a sprite from a trail prefab.
    Sprite GetRocketTrailStarSprite(GameObject starPrefab)
    {
        if (starPrefab == null)
        {
            return null;
        }

        SpriteRenderer spriteRenderer = starPrefab.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.sprite;
        }

        Image image = starPrefab.GetComponentInChildren<Image>();
        return image != null ? image.sprite : null;
    }

    // Clears trail stars in spawn order.
    IEnumerator DestroyRocketTrail(List<GameObject> trailStars)
    {
        int batchSize = Mathf.Max(1, rocketTrailDestroyBatchSize);
        int destroyedThisBatch = 0;

        for (int i = 0; i < trailStars.Count; i++)
        {
            GameObject star = trailStars[i];
            if (star != null)
            {
                Destroy(star);
            }

            destroyedThisBatch++;
            if (destroyedThisBatch < batchSize)
            {
                continue;
            }

            destroyedThisBatch = 0;
            if (rocketTrailDestroyInterval > 0f)
            {
                yield return new WaitForSeconds(rocketTrailDestroyInterval);
            }
            else
            {
                yield return null;
            }
        }
    }
}
