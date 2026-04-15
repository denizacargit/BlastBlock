using System.Collections;
using UnityEngine;

public partial class GridManager
{
    void DamageObstacle(Obstacle obstacle)
    {
        if (obstacle == null)
        {
            return;
        }

        int x = obstacle.x;
        int y = obstacle.y;
        string obstacleType = obstacle.obstacleType;
        Vector3 particlePosition = GetCellLocalPosition(x, y);

        obstacle.TakeDamage();

        if (obstacle.health > 0)
        {
            return;
        }

        CollectGoal(obstacleType);

        if (x >= 0 && x < currentLevelData.grid_width && y >= 0 && y < currentLevelData.grid_height)
        {
            allObstacles[x, y] = null;
        }

        SpawnObstacleParticles(obstacleType, particlePosition, x, y);
    }

    void SpawnObstacleParticles(string obstacleType, Vector3 localPosition, int x, int y)
    {
        if (effectsParent == null)
        {
            return;
        }

        GameObject[] prefabs = GetObstacleParticlePrefabs(obstacleType);
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] == null)
            {
                continue;
            }

            for (int copy = 0; copy < 2; copy++)
            {
                int particleIndex = (i * 2) + copy;
                float angle = (360f / (prefabs.Length * 2f)) * particleIndex + Random.Range(-18f, 18f);
                Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
                Vector3 offset = direction * Random.Range(cellWidth * 0.12f, cellWidth * 0.24f);

                GameObject particle = Instantiate(prefabs[i], effectsParent);
                particle.transform.localPosition = localPosition + offset;
                particle.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                particle.transform.localScale = Vector3.one * cubeParticleScale * Random.Range(0.8f, 1.15f);
                SetParticleSortingOrder(particle, x, y, 6);
                StartCoroutine(AnimateParticle(particle.transform, direction, particleLifetime * 1.15f, 1.15f));
            }
        }
    }

    void SpawnCubeParticles(string cubeColor, Vector3 localPosition, int x, int y)
    {
        int count = Mathf.Max(0, cubeParticleCount);
        if (count == 0)
        {
            return;
        }

        GameObject prefab = GetCubeParticlePrefab(cubeColor);
        Sprite fallbackSprite = prefab == null ? GetCubeParticleSprite(cubeColor) : null;
        if (prefab == null && fallbackSprite == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject particle = CreateParticleInstance(prefab, fallbackSprite);
            if (particle == null)
            {
                continue;
            }

            float angle = (360f / count) * i + Random.Range(-12f, 12f);
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
            Vector3 offset = direction * Random.Range(cellWidth * 0.08f, cellWidth * 0.18f);

            particle.transform.localPosition = localPosition + offset;
            particle.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            particle.transform.localScale = Vector3.one * cubeParticleScale * Random.Range(0.75f, 1.15f);
            SetParticleSortingOrder(particle, x, y, 7);
            StartCoroutine(AnimateParticle(particle.transform, direction, particleLifetime, 0.85f));
        }
    }

    GameObject CreateParticleInstance(GameObject prefab, Sprite fallbackSprite)
    {
        if (effectsParent == null)
        {
            return null;
        }

        if (prefab != null)
        {
            return Instantiate(prefab, effectsParent);
        }

        GameObject particle = new GameObject("cube_particle");
        particle.transform.SetParent(effectsParent, false);
        SpriteRenderer spriteRenderer = particle.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = fallbackSprite;
        return particle;
    }

    IEnumerator AnimateParticle(Transform particle, Vector3 direction, float lifetime, float travelMultiplier)
    {
        if (particle == null)
        {
            yield break;
        }

        SpriteRenderer[] renderers = particle.GetComponentsInChildren<SpriteRenderer>();
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            startColors[i] = renderers[i].color;
        }

        Vector3 start = particle.localPosition;
        Vector3 end = start + direction * Random.Range(cellWidth * 0.28f, cellWidth * 0.48f) * travelMultiplier;
        float rotationSpeed = Random.Range(-360f, 360f);
        float elapsed = 0f;

        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);
            particle.localPosition = Vector3.Lerp(start, end, 1f - Mathf.Pow(1f - t, 2f));
            particle.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                Color color = startColors[i];
                color.a *= 1f - t;
                renderers[i].color = color;
            }

            yield return null;
        }

        if (particle != null)
        {
            Destroy(particle.gameObject);
        }
    }

    GameObject[] GetObstacleParticlePrefabs(string obstacleType)
    {
        switch (obstacleType)
        {
            case "bo":
                return HasAnyPrefab(boxShardPrefabs) ? boxShardPrefabs : Resources.LoadAll<GameObject>("Obstacles/Box/Particles");
            case "s":
                return HasAnyPrefab(stoneShardPrefabs) ? stoneShardPrefabs : Resources.LoadAll<GameObject>("Obstacles/Stone/Particles");
            case "v":
                return HasAnyPrefab(vaseShardPrefabs) ? vaseShardPrefabs : Resources.LoadAll<GameObject>("Obstacles/Vase/Particles");
            default:
                return null;
        }
    }

    bool HasAnyPrefab(GameObject[] prefabs)
    {
        if (prefabs == null)
        {
            return false;
        }

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                return true;
            }
        }

        return false;
    }

    GameObject GetCubeParticlePrefab(string cubeColor)
    {
        switch (cubeColor)
        {
            case "r": return redCubeParticlePrefab;
            case "g": return greenCubeParticlePrefab;
            case "b": return blueCubeParticlePrefab;
            case "y": return yellowCubeParticlePrefab;
            default: return null;
        }
    }

    Sprite GetCubeParticleSprite(string cubeColor)
    {
        switch (cubeColor)
        {
            case "r": return Resources.Load<Sprite>("Cubes/Particles/particle_red");
            case "g": return Resources.Load<Sprite>("Cubes/Particles/particle_green");
            case "b": return Resources.Load<Sprite>("Cubes/Particles/particle_blue");
            case "y": return Resources.Load<Sprite>("Cubes/Particles/particle_yellow");
            default: return null;
        }
    }

    void SetParticleSortingOrder(GameObject particle, int x, int y, int layerOffset)
    {
        SpriteRenderer[] renderers = particle.GetComponentsInChildren<SpriteRenderer>();
        int sortingOrder = GetBoardSortingOrder(x, y, layerOffset);
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}
