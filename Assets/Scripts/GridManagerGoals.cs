using UnityEngine;
using System.Collections.Generic;

public partial class GridManager
{
    private class GoalState
    {
        public string type;
        public int remaining;
        public GoalSlotUI slot;
    }

    // Builds the goal list for the loaded level.
    void InitializeGoals()
    {
        activeGoals.Clear();
        levelCompleted = false;
        ClearGoalSlots();

        AddObstacleGoalsFromGrid();

        if (activeGoals.Count == 0 && currentLevelData.goals != null && currentLevelData.goals.Length > 0)
        {
            foreach (GoalData goalData in currentLevelData.goals)
            {
                if (goalData != null)
                {
                    AddGoal(goalData.type, goalData.count);
                }
            }
        }
        else if (activeGoals.Count == 0 && !string.IsNullOrEmpty(currentLevelData.goal_type))
        {
            AddGoal(currentLevelData.goal_type, currentLevelData.goal_count);
        }

        UpdateLegacyGoalUI();
        PositionGoalSlots();
    }

    // Adds goals based on obstacle counts in the grid.
    void AddObstacleGoalsFromGrid()
    {
        foreach (string goalType in obstacleGoalTypes)
        {
            int count = CountGoalsForType(goalType);

            if (count > 0)
            {
                AddGoal(goalType, count);
            }
        }
    }

    // Removes old goal UI slots.
    void ClearGoalSlots()
    {
        if (goalSlotsParent == null)
        {
            return;
        }

        for (int i = goalSlotsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(goalSlotsParent.GetChild(i).gameObject);
        }
    }

    // Registers one goal and creates its slot.
    void AddGoal(string type, int count)
    {
        if (string.IsNullOrEmpty(type))
        {
            return;
        }

        int resolvedCount = count > 0 ? count : CountGoalsForType(type);
        GoalSlotUI prefab = goalSlotPrefab != null ? goalSlotPrefab : Resources.Load<GoalSlotUI>("Cubes/GoalSlot");
        GoalState state = new GoalState
        {
            type = type,
            remaining = resolvedCount
        };

        if (prefab != null && goalSlotsParent != null)
        {
            GoalSlotUI slot = Instantiate(prefab, goalSlotsParent);
            slot.Setup(LoadGoalIcon(type), resolvedCount);
            state.slot = slot;
        }

        activeGoals.Add(state);
    }

    // Loads the icon used by a goal.
    Sprite LoadGoalIcon(string type)
    {
        Sprite icon = Resources.Load<Sprite>("Sprites/Icons/" + type + "_icon");

        if (icon == null)
        {
            icon = Resources.Load<Sprite>(GetDefaultGoalIconPath(type));
        }

        if (icon == null && type == "obstacle")
        {
            icon = genericObstacleGoalIcon;
        }

        return icon;
    }

    // Returns the fallback icon path for built-in goals.
    string GetDefaultGoalIconPath(string type)
    {
        switch (type)
        {
            case "bo":
                return "Obstacles/Box/box";
            case "s":
                return "Obstacles/Stone/stone";
            case "v":
                return "Obstacles/Vase/vase_01";
            default:
                return string.Empty;
        }
    }

    // Places goal slots inside the goal card.
    void PositionGoalSlots()
    {
        int totalCount = Mathf.Min(activeGoals.Count, 3);

        for (int i = 0; i < totalCount; i++)
        {
            GoalSlotUI slot = activeGoals[i].slot;

            if (slot == null)
            {
                continue;
            }

            RectTransform rect = slot.GetComponent<RectTransform>();

            if (rect == null)
            {
                continue;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            ApplyGoalSlotLayout(rect, i, totalCount);
        }
    }

    // Applies the layout for one, two, or three goals.
    void ApplyGoalSlotLayout(RectTransform rect, int index, int totalCount)
    {
        Vector2 slotSize;

        if (totalCount == 1)
        {
            rect.anchoredPosition = Vector2.zero;
            slotSize = new Vector2(70f, 70f);
            rect.sizeDelta = slotSize;
            ApplyGoalSlotVisualSize(rect, slotSize);
            return;
        }

        if (totalCount == 2)
        {
            float x = index == 0 ? -28f : 28f;
            rect.anchoredPosition = new Vector2(x, 3f);
            slotSize = new Vector2(55f, 55f);
            rect.sizeDelta = slotSize;
            ApplyGoalSlotVisualSize(rect, slotSize);
            return;
        }

        if (totalCount == 3)
        {
            Vector2[] positions =
            {
                new Vector2(-25f, 21f),
                new Vector2(25f, 21f),
                new Vector2(0f, -29f)
            };

            rect.anchoredPosition = positions[index];
            slotSize = new Vector2(45f, 45f);
            rect.sizeDelta = slotSize;
            ApplyGoalSlotVisualSize(rect, slotSize);
        }
    }

    // Resizes a goal slot's inner visuals.
    void ApplyGoalSlotVisualSize(RectTransform rect, Vector2 slotSize)
    {
        GoalSlotUI slot = rect.GetComponent<GoalSlotUI>();

        if (slot != null)
        {
            slot.ApplyVisualSize(slotSize);
        }
    }

    // Counts matching cells in the level grid.
    int CountGoalsForType(string goalType)
    {
        int count = 0;

        foreach (string itemType in currentLevelData.grid)
        {
            if (DoesTypeMatchGoal(itemType, goalType))
            {
                count++;
            }
        }

        return count;
    }

    // Checks whether a token is an obstacle.
    bool IsObstacleType(string type)
    {
        return type == "bo" || type == "s" || type == "v";
    }

    // Compares a grid token with a goal token.
    bool DoesTypeMatchGoal(string itemType, string goalType)
    {
        if (string.IsNullOrEmpty(itemType) || string.IsNullOrEmpty(goalType))
        {
            return false;
        }

        if (goalType == "obstacle")
        {
            return IsObstacleType(itemType);
        }

        return itemType == goalType;
    }

    // Reduces progress for a collected item.
    void CollectGoal(string type)
    {
        if (levelCompleted)
        {
            return;
        }

        bool changed = false;

        foreach (GoalState goal in activeGoals)
        {
            if (goal.remaining <= 0 || !DoesTypeMatchGoal(type, goal.type))
            {
                continue;
            }

            goal.remaining--;
            changed = true;

            if (goal.slot != null)
            {
                goal.slot.SetCount(goal.remaining);
            }
        }

        if (!changed)
        {
            return;
        }

        UpdateLegacyGoalUI();

        if (AreAllGoalsComplete())
        {
            CompleteLevel();
        }
    }

    // Checks whether every goal is complete.
    bool AreAllGoalsComplete()
    {
        if (activeGoals.Count == 0)
        {
            return false;
        }

        foreach (GoalState goal in activeGoals)
        {
            if (goal.remaining > 0)
            {
                return false;
            }
        }

        return true;
    }

    // Updates the older single-goal UI fields.
    void UpdateLegacyGoalUI()
    {
        int totalRemaining = 0;

        foreach (GoalState goal in activeGoals)
        {
            totalRemaining += Mathf.Max(0, goal.remaining);
        }

        if (goalCounterText != null)
        {
            goalCounterText.text = totalRemaining.ToString();
        }

        if (goalIconImage != null && activeGoals.Count > 0)
        {
            Sprite icon = LoadGoalIcon(activeGoals[0].type);

            if (icon != null)
            {
                goalIconImage.sprite = icon;
            }
        }
    }
}
