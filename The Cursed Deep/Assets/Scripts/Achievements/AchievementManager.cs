using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Achievements
{
    // -- Once we implement Steamworks Uncomment the -- //
    // -- commented lines to enable Achievements for Steam -- // 
    // -- then modify the rest of the code to work with Steam -- //
    [System.Serializable]
    public class AchievementManager : MonoBehaviour
    {
        [SerializeField] private bool isSteamEnabled;
        [SerializeField] private bool autoSave;
        public AchievementData achievementData;
        public int displayTime;
        public bool achDisplay;
        public int achDisplayNum;
        public bool showProgress;
        public DisplayLocation display;
        
        public static AchievementManager Instance;
        public AchievementUIDisplay displayUI;
        [SerializeField] UnityEvent onDisplay;
    
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
            //LoadAchievements();
        }
        public int GetAchievementCount()
        {
            //int count = (from ProgressiveAchievement i in progAch where i.isUnlocked select i).Count();
            return achievementData.achievements.Count(i => i.isUnlocked);
        }
    
        public float GetAchievementPercent()
        {
            if (achievementData.achievements.Count == 0)
            {
                return 0;
            }
            return (float)GetAchievementCount()/achievementData.achievements.Count * 100;
        }
    
        public void Unlock(string id)
        {
            //Unlock(FindAchievement(id));
            int index = FindAchievement(id);
            if (index >= 0 && index < achievementData.achievements.Count)
            {
                Unlock(index);
            }
            else
            {
                Debug.LogWarning($"Achievement with id {id} not found");
            }
        }
        private void Unlock(int id)
        {
            if (id < 0 || id >= achievementData.achievements.Count)
            {
                Debug.LogWarning($"Achievement with index {id} out of range");
                return;
            }
            
            if (isSteamEnabled)
            {
                /* var ach = new Steamworks.Data.Achievement(id);
            Debug.Log($"Achievement {id} status : " + ach.State);
            if (ach.State)
            {
                Debug.Log("Achievement already unlocked");
            }
            else
            {
                ach.Trigger();
                Debug.Log($"Achievement {id} unlocked"); 
            }*/
            }
            // Debug.Log("Steam is not enabled");
            if (achievementData.achievements[id] is ProgressiveAchievement { isUnlocked: false } achievement)
            {
                achievement.progress = achievementData.achievements[id].goal;
                achievement.isUnlocked = true;
                achDisplay = true;
                DisplayUnlock(id);
                //SaveAchievements();
                Debug.Log($"Achievement {id} unlocked");
            }
        }
    
        public void UpdateProgress(string id)
        {
            UpdateProgress(FindAchievement(id));
            // int index = FindAchievement(id);
            // if (index >= 0 && index < achievementData.achievements.Count)
            // {
            //     Unlock(index);
            // }
            // else
            // {
            //     Debug.LogWarning($"Achievement with id {id} not found, cannot update progress");
            // }
        }
        
        private void UpdateProgress(int id)
        {
            //float progress = 0;
            if (id < 0 || id >= achievementData.achievements.Count)
            {
                Debug.LogWarning($"Achievement with index {id} out of range, cannot update progress");
                return;
            }
            //progress++;
            var achievement = achievementData.achievements[id] as ProgressiveAchievement;
            if (achievement != null) achievement.progress++;
            if (achievementData.achievements[id].isProgression)
            {
                if (achievement != null && achievement.progress >= achievementData.achievements[id].goal)
                {
                    Unlock(id);
                }
                else
                {
                    //if (achievement != null) achievement.progress = progress;
                    DisplayUnlock(id);
                    //SaveAchievements();
                }
            }
        }
    
        public void AddProgress(string id, float progress)
        {
            //AddProgress(FindAchievement(id), progress);
            int index = FindAchievement(id);
            if (index >= 0 && index < achievementData.achievements.Count)
            {
                Unlock(index);
            }
            else
            {
                Debug.LogWarning($"Achievement with id {id} not found, cannot update progress");
            }
        }
        
        private void AddProgress(int id, float progress)
        {
            if (id < 0 || id >= achievementData.achievements.Count)
            {
                Debug.LogWarning($"Achievement with index {id} out of range, cannot add progress");
                return;
            }
            
            var achievement = achievementData.achievements[id] as ProgressiveAchievement;
            if (achievementData.achievements[id].isProgression)
            {
                if (achievement != null && achievement.progress + progress >= achievementData.achievements[id].goal)
                {
                    Unlock(id);
                }
                else
                {
                    if (achievement != null) achievement.progress += progress;
                    DisplayUnlock(id);
                    //SaveAchievements();
                }
            }
        }
        public int FindAchievement(string id)
        {
            return achievementData.achievements.FindIndex(a => a.id.Equals(id));
        }
        public void DisplayUnlock(int id)
        {
            if (id < 0 || id >= achievementData.achievements.Count)
            {
                return;
            }
            if (achievementData.achievements[id] is ProgressiveAchievement achievement && (achDisplay && !achievementData.achievements[id].isHidden || achievement.isUnlocked))
            {
                if (achievementData.achievements[id].isProgression && achievement.progress < achievementData.achievements[id].goal)
                {
                    int steps = (int)achievementData.achievements[id].goal / (int)achievementData.achievements[id].notify;
                    for (int i = steps; i > achievement.progressUpdate; i--)
                    {
                        if (achievement.progress >= achievementData.achievements[id].notify * i)
                        {
                            onDisplay.Invoke();
                            achievement.progressUpdate = i;
                            displayUI.ScheduleAchievementDisplay(id);
                            return;
                        }
                    }
                }
                else
                {
                    onDisplay.Invoke();
                    displayUI.ScheduleAchievementDisplay(id);
                }
            }
        }
       public void SaveAchievements()
        {
            for (int i = 0; i < achievementData.achievements.Count; i++)
            {
                PlayerPrefs.SetString("Achievements" + i, JsonUtility.ToJson(achievementData.achievements[i]));
            }
            PlayerPrefs.Save();
        }
    
        public void LoadAchievements()
        {
            achievementData.achievements.Clear();
        
            for (int i = 0; i < achievementData.achievements.Count; i++)
            {
                if(PlayerPrefs.HasKey("Achievements" + i))
                {
                    var newProgAch = JsonUtility.FromJson<ProgressiveAchievement>(PlayerPrefs.GetString("AchievementState_" + i));
                    achievementData.achievements.Add(newProgAch);
                }
                else
                {
                    achievementData.achievements.Add(new ProgressiveAchievement());
                }
            }
        }
    
        public void ResetAchievements()
        {
            if (isSteamEnabled)
            {
                //var ach = new Steamworks.Data.Achievement(achievementData.achievements[0].id);
                // ach.Clear();
                // Debug.Log($"Achievement {id} cleared");
            }
            achievementData.achievements.Clear();
            for (int i = 0; i < achievementData.achievements.Count; i++)
            {
                PlayerPrefs.DeleteKey("Achievements" + i);
                achievementData.achievements.Add(new ProgressiveAchievement());
            }
            SaveAchievements();
        }
    
        private void AutoSaveAchievements()
        {
            if (autoSave)
            {
                SaveAchievements();
            }
        }
    }

    public enum DisplayLocation
    {
        Top,
        BottomLeft,
        BottomRight
    }
}