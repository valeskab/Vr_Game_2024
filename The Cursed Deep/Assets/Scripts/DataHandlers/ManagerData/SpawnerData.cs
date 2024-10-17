using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpawnManager")]
[CreateAssetMenu (fileName = "SpawnerData", menuName = "Data/ManagerData/SpawnerData")]
public class SpawnerData : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField] internal bool allowDebug;
#endif
    
    [SerializeField] private bool randomizeSpawnRate;
    public bool usePriority;
    public IntData numToSpawn;
    [SerializeField] private FloatData spawnRateMin, spawnRateMax;
    [SerializeField] private  IntData _activeCount;
    public IntData globalLaneActiveLimit;
    public PrefabDataList prefabList;
    public void SetPrefabDataList(PrefabDataList data) => prefabList = data;

    private readonly List<WaitForSeconds> _spawnRates = new();
    private WaitForSeconds _waitForSpawnRate;
    private float spawnRate => spawnRateMin == spawnRateMax || spawnRateMin < spawnRateMax ? spawnRateMin : Random.Range(spawnRateMin, spawnRateMin);
    
    internal void GenerateSpawnRates()
    {
        if (!randomizeSpawnRate)
        {
            _waitForSpawnRate = _spawnRates.Count > 0 ? _spawnRates[0] : new WaitForSeconds(spawnRate);
            return;
        }
        if (totalCountToSpawn < _spawnRates.Count) return;
        
        var count = totalCountToSpawn - _spawnRates.Count;
        for (var i = 0; i < count; i++)
            _spawnRates.Add(new WaitForSeconds(spawnRate));
    }
    
    internal WaitForSeconds GetWaitForSpawnRate() => randomizeSpawnRate ? _spawnRates[Random.Range(0, _spawnRates.Count)] : _waitForSpawnRate;
    
    internal int totalCountToSpawn { get; set; }
    internal int spawnedCount { get; set; }
    internal int amountLeftToSpawn => totalCountToSpawn - spawnedCount;
    internal int activeCount
    {
        get => _activeCount;
        set => _activeCount.value = value < 0 ? 0 : value;
    }
    internal bool canSpawn => spawnedCount < totalCountToSpawn;
    internal bool spawningComplete => spawnedCount >= totalCountToSpawn;
    

    [System.Serializable]
    public class Spawner
    {
        public string spawnerID;
        public int laneActiveLimitAdjustment;
        public TransformData spawnLocation, pathingTarget;
        private int _currentSpawnCount;
        
        public int GetActiveLimit(int globalActiveLimit) => globalActiveLimit + laneActiveLimitAdjustment;
        public int GetAliveCount() => _currentSpawnCount;
        public void IncrementCount() => _currentSpawnCount++;
        public void DecrementCount() => _currentSpawnCount--;
        public void ResetCount() => _currentSpawnCount = 0;
    }
    
    public List<Spawner> spawners = new();
    private readonly List<Spawner> _availableSpawners = new();

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!_activeCount) Debug.LogError("Missing IntData for activeCount on SpawnerData" + name, this);
        if (!prefabList) Debug.LogError("Missing PrefabDataList for prefabList on SpawnerData" + name, this);
#endif
    }
    
    private void OnEnable()
    {
        if (globalLaneActiveLimit)
            globalLaneActiveLimit.value = globalLaneActiveLimit < 1 ? globalLaneActiveLimit.value : 1;
        
        if (!numToSpawn && totalCountToSpawn < 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"numToSpawn is null on {name}. Setting spawn count to {spawners.Count}.", this);
#endif
            totalCountToSpawn = spawners.Count;
            return;
        }
        totalCountToSpawn = numToSpawn;
    }
    
    public void ResetSpawnerData()
    {
        activeCount = 0;
        foreach (var spawner in spawners)
        {
            spawner.ResetCount();
        }
    }
    
    
    internal int GetSpawnerActiveLimit(Spawner spawner) => spawner.GetActiveLimit(globalLaneActiveLimit.value);

    internal Spawner GetInactiveSpawner()
    {
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Checking for available spawners.\nPotential Spawners: {spawners.Count}", this);
#endif
        if (spawners.Count <= 0) return null;
        _availableSpawners.Clear();
        
        foreach (var spawner in spawners)
        {
            var currentSpawnerActiveCount = spawner.GetAliveCount();
            if (currentSpawnerActiveCount < GetSpawnerActiveLimit(spawner))
            {
                _availableSpawners.Add(spawner);
            }
#if UNITY_EDITOR
            if (allowDebug) Debug.Log($"Spawner: {spawner.spawnerID} has {currentSpawnerActiveCount} active.", this);
#endif
        }
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Found {_availableSpawners.Count} Available Spawners.", this);
#endif
        var output = _availableSpawners.Count == 0 ? null: _availableSpawners[Random.Range(0, _availableSpawners.Count)];
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Selected Spawner: {output?.spawnerID}", this);
#endif
        return output;
    }
    
    public void HandleSuccessfulSpawn(ref Spawner spawner)
    {
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Handling removal of spawn from {spawner.spawnerID}.", this);
#endif
        spawner.IncrementCount();
        activeCount++;
        spawnedCount++;
    }
    
    public void HandleSpawnRemoval(ref Spawner spawner, bool validDeath = true)
    {
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Handling removal of spawn from {spawner.spawnerID}.", this);
#endif
        spawner.DecrementCount();
        activeCount--;
        if (validDeath) spawnedCount--;
    }
}