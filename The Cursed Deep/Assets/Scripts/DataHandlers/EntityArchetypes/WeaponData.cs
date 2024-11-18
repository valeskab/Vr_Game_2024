using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Data/Entity/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
    public float health, damage;
    
    [Header("Total Stats")]
    [SerializeField] [ReadOnly] private int totalFired, totalWhiffs;
    [SerializeField] [ReadOnly] private float totalDamageDealt;
    
    public void IncrementFiredTotal() => totalFired++;
    public void IncrementWhiffTotal() => totalWhiffs++;
    public void IncreaseDamageDealt() => totalDamageDealt += damage;
    public void DecrementFiredTotal() => totalFired--;
    public void DecrementWhiffTotal() => totalWhiffs--;
    public void DecreaseDamageDealt() => totalDamageDealt -= damage;
}
