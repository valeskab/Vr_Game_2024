using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObject
    {
        [System.Serializable]
        private struct Level
        {
            public int spawnCount;
            public float spawnRateMin, spawnRateMax;
            public int laneActiveLimit;
            public int spawnBaseHealth;
            public int spawnBaseDamage;
            public int spawnValue;
            public int spawnScore;
        }
        
        public IntData currentLevel;

        public int spawnCount => levels[currentLevel].spawnCount;
        public float spawnRateMin => levels[currentLevel].spawnRateMin;
        public float spawnRateMax => levels[currentLevel].spawnRateMax;
        public int laneActiveLimit => levels[currentLevel].laneActiveLimit;
        public int spawnBaseHealth => levels[currentLevel].spawnBaseHealth;
        public int spawnBaseDamage => levels[currentLevel].spawnBaseDamage;
        public int spawnBounty => levels[currentLevel].spawnValue;
        public int spawnScore => levels[currentLevel].spawnScore;

        [SerializeField] private Level[] levels;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!currentLevel) Debug.LogError("Current Level is null. Please assign a value.", this);
#endif
        }
    }
}