using System;
using System.Collections.Generic;

[System.Serializable]
public class GoalData
{
    public string type;
    public int count;
}

[System.Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    
    // ADD THESE TWO LINES:
    public string goal_type; // e.g., "r" for red cubes, "bo" for boxes
    public int goal_count;   // e.g., 10
    public GoalData[] goals;
    
    public string[] grid;
}
