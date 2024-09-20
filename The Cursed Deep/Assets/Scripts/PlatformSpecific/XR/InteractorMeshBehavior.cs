using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class InteractorMeshBehavior: MonoBehaviour
{
    [Serializable]
    private struct Models
    {
        public GameObject model;
    }
    
    [SerializeField] private Models[] modelArray;
    
    public void Show()
    {
        foreach (var models in modelArray) models.model.SetActive(true);
    }
    
    public void Hide()
    {
        foreach (var models in modelArray) models.model.SetActive(false);
    }
}
