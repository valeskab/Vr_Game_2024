using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Achievement Data")]
public class AchievementData : ScriptableObject
{
    [SerializeReference, SubclassSelector]
    public List<Achievement> achievements = new List<Achievement>();
}

[System.Serializable]
public abstract class Achievement
{
    public string iD;
    public int progress;
    public string description;
    public bool isUnlocked;
    public GameAction action;
    
    private void OnEnable() => action.Raise += CheckProgress;
    private void OnDisable() => action.Raise -= CheckProgress;

    protected abstract void CheckProgress(GameAction _);
}

[System.Serializable]
public class FloatComparison : Achievement
{
    public int targetValue;
    
    protected override void CheckProgress(GameAction _)
    {
        if (progress >= targetValue) return;
        progress++;
        if (progress >= targetValue)
        {
            isUnlocked = true;
            Debug.Log($"Achievement {iD} unlocked");
        }
    }
}