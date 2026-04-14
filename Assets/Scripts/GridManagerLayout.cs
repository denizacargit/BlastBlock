using UnityEngine;

public partial class GridManager
{
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
        return gridVisualScale;
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

    int GetBoardSortingOrder(int x, int y, int layerOffset = 0)
    {
        return BoardSortingBase + (y * 10) + x + layerOffset;
    }
}
