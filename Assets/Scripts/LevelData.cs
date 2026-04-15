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
    public string goal_type;
    public int goal_count;
    public GoalData[] goals;
    public string[] grid;
}
