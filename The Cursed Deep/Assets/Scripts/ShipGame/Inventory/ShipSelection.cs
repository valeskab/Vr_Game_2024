using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct ShipSelection
    {
        [SerializeField] private string _name;
        
        // Number of lanes in the ship
        [SerializeField] private int numberOfLanes;
        
        // Base health of the ship
        [SerializeField] private int _health;
        
        // Cost of the ship
        [SerializeField] private int _cost;
        
        // Prefab that determines all other data within this selection
        public PrefabData prefab;
            
        // Instantiates a cannon in every lane of the ship dependent on and used in the prefab below
        // Requires the cannon selection's prefab and offset
        public InstancerData cannonInstancerData;
            
        // Ship specific spawner data for ammo
        // Requires the ammo selection's prefab list
        public SpawnerData ammoSpawnerData;
            
        // Ship specific spawner data for enemies
        // Requires the enemy selection's prefab list
        public SpawnerData enemySpawnerData;
        
        public string name => _name;
        public int laneCount => numberOfLanes;
        public int health => _health;
        public int cost => _cost;
    }
    
    
}