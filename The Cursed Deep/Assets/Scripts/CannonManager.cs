using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static ZPTools.Utility.UtilityFunctions;

public class CannonManager : MonoBehaviour
{
    [Header("Ammo:")]
    [SerializeField] private GameObject ammoEntity;
    [SerializeField] private SocketMatchInteractor _ammoSpawnSocket;
    [SerializeField] private int despawnTime = 30;
    private List<GameObject> _despawningAmmoList = new();
    private GameObject _loadedAmmo;
    private MeshFilter _ammoMeshFilter;
    private MeshRenderer _ammoMeshRenderer;
    
    [Header("Fire Physics System:")]
#if UNITY_EDITOR
    public bool solidLine;
    [Range(0, 100)] public int simulationTime = 80;
#endif
    [SerializeField, SteppedRange(rangeMin:0.0f, rangeMax:1000.0f, step:0.01f)] private float propellantForce = 10.0f;
    [SerializeField] private Transform muzzlePosition, breechPosition;
    [SerializeField] private SocketMatchInteractor reloadSocket;
    private Vector3 forceVector => !muzzlePosition || !breechPosition ? Vector3.zero : (ejectionPoint - ingitionPoint).normalized;
    private Vector3 momentumVector  => forceVector * propellantForce;
    private Vector3 ingitionPoint => !breechPosition ? Vector3.zero : breechPosition.position;
    private Vector3 ejectionPoint  => !muzzlePosition ? Vector3.zero : muzzlePosition.position;
    
    private List <GameObject> _currentAmmoList;
    private bool _isLoaded;
    private GameObject _ammoObj;
    private Coroutine _addForceCoroutine;

    [Header("Model Animation:")]
    [SerializeField] private Animator _modelAnimator;
    [SerializeField] private string _fireAnimationTrigger = "Fire";
    [SerializeField] private string _loadAnimationTrigger = "Load";
    
    [Header("State Events:")]
    public UnityEvent onSuccessfulFire;
    public UnityEvent onLoaded;
    
    private readonly WaitForFixedUpdate _waitFixedUpdate = new();
    private readonly WaitForSeconds _waitSingleSecond = new(1f);
    
    private void Awake()
    {
        if (!_modelAnimator) _modelAnimator = GetComponent<Animator>();
    } 
    
    private static bool _errorsLogged;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (_errorsLogged) return;

        if (!muzzlePosition) Debug.LogError("Muzzle Position is not set.", this);
        if (!breechPosition) Debug.LogError("Breech Position is not set.", this);
        if (!reloadSocket) Debug.LogError("Reload Socket is not set.", this);

        _errorsLogged = true;
#endif
    }

    private void OnEnable()
    {
        if (reloadSocket)
        {
            reloadSocket.ObjectSocketed += LoadCannon;
            reloadSocket.ObjectUnsocketed += UnloadCannon;
        }
        if (_ammoSpawnSocket) _ammoSpawnSocket.ObjectUnsocketed += HandleActivatedAmmo;
    }

    private void OnDisable()
    {
        if (reloadSocket)
        {
            reloadSocket.ObjectSocketed -= LoadCannon;
            reloadSocket.ObjectUnsocketed -= UnloadCannon;
        }
        if (_ammoSpawnSocket) _ammoSpawnSocket.ObjectUnsocketed -= HandleActivatedAmmo;
        _despawningAmmoList.Clear();
    }

    public SocketMatchInteractor ammoSpawnSocket
    {
        get => _ammoSpawnSocket;
        set
        {
            if (_ammoSpawnSocket)
            {
                _ammoSpawnSocket.ObjectUnsocketed -= HandleActivatedAmmo;
            }
            _ammoSpawnSocket = value;
            if (_ammoSpawnSocket)
            {
                _ammoSpawnSocket.ObjectUnsocketed += HandleActivatedAmmo;
            }
        }
    }

    public void Fire()
    {
        if (!_ammoObj)
        {
            Debug.LogWarning($"No ammo found in {gameObject.name}", this);
            return;
        }

        if (!_isLoaded)
        {
            Debug.LogWarning($"{gameObject.name} has not been loaded.", this);
            return;
        }
        
        var ammoRb = _ammoObj.GetComponent<Rigidbody>();
        
        if (_addForceCoroutine != null){ _ammoObj.SetActive(false); return;}
        _ammoObj.SetActive(true);
        if (_modelAnimator.GetBool(_loadAnimationTrigger)) _modelAnimator.ResetTrigger(_loadAnimationTrigger);
        _modelAnimator.SetTrigger(_fireAnimationTrigger);
        onSuccessfulFire.Invoke();
        _addForceCoroutine ??= StartCoroutine(AddForceToAmmo(ammoRb));
        UnloadCannon(null);
    }

    private void LoadCannon(GameObject obj)
    {
        if (reloadSocket.socketScaleMode != SocketScaleMode.Fixed)
            reloadSocket.socketScaleMode = SocketScaleMode.Fixed;
        reloadSocket.fixedScale = Vector3.one * 0.01f;
        _isLoaded = true;
        _loadedAmmo = obj;
        _modelAnimator.SetTrigger(_loadAnimationTrigger);
        _ammoObj = GetAmmo();
        _ammoMeshFilter = AdvancedGetComponent<MeshFilter>(_ammoObj);
        _ammoMeshRenderer = AdvancedGetComponent<MeshRenderer>(_ammoObj);
        var objMeshFilter = AdvancedGetComponent<MeshFilter>(obj);
        var objMeshRenderer = AdvancedGetComponent<MeshRenderer>(obj);
        if (_ammoMeshFilter && objMeshFilter)
        {
            _ammoMeshFilter.mesh = objMeshFilter.mesh;
        }
        if (_ammoMeshRenderer && objMeshRenderer)
        {
            _ammoMeshRenderer.material = objMeshRenderer.material;
        }
        onLoaded.Invoke();
    }

    private void UnloadCannon([CanBeNull] GameObject obj)
    {
        if (reloadSocket.socketScaleMode != SocketScaleMode.Fixed)
            reloadSocket.socketScaleMode = SocketScaleMode.Fixed;
        reloadSocket.fixedScale = Vector3.one / 0.01f;
        reloadSocket.UnsocketObject();
        _loadedAmmo = null;
        _isLoaded = false;
        if (!obj) return;
        AdvancedGetComponent<PooledObjectBehavior>(obj)?.TriggerRespawn();
    }

    private GameObject GetAmmo()
    {
        _currentAmmoList ??= new List<GameObject>();
        foreach (var ammoObj in _currentAmmoList.Where(ammoObj => !ammoObj.activeSelf))
        {
            ammoObj.transform.position = muzzlePosition.position;
            ammoObj.transform.rotation = muzzlePosition.rotation;
            return ammoObj;
        }
        var newAmmo = Instantiate(ammoEntity, muzzlePosition.position, muzzlePosition.rotation);
        newAmmo.SetActive(false);
        _currentAmmoList.Add(newAmmo);
        return newAmmo;
    }
    
    private IEnumerator AddForceToAmmo(Rigidbody ammoRb)
    {
        ammoRb.isKinematic = false;
        ammoRb.useGravity = true;
        ammoRb.velocity = Vector3.zero;
        ammoRb.angularVelocity = Vector3.zero;
        
        yield return _waitFixedUpdate;
        yield return _waitFixedUpdate;
        yield return _waitFixedUpdate;
        yield return null;
        
        ammoRb.AddForce(momentumVector, ForceMode.Impulse);
        _addForceCoroutine = null; 
    }

    private void HandleActivatedAmmo(GameObject ammo) => StartCoroutine(DespawnAmmo(ammo));
    
    private IEnumerator DespawnAmmo(GameObject ammo)
    {
        if (_despawningAmmoList.Contains(ammo)) yield break;
        _despawningAmmoList.Add(ammo);

        var doNotDeactivate = false;
        var time = 0;
        while (time < despawnTime)
        {
            if ((_isLoaded && _loadedAmmo == ammo) || !ammo.activeInHierarchy)
            {
                doNotDeactivate = true;
                break;
            }

            time++;
            yield return _waitSingleSecond;
        }
        AdvancedGetComponent<PooledObjectBehavior>(ammo)?.TriggerRespawn();
        _despawningAmmoList.Remove(ammo);
        yield return _waitFixedUpdate;
        if(!doNotDeactivate) ammo.SetActive(false);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!muzzlePosition || !breechPosition) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(ingitionPoint, 0.2f);
        Gizmos.DrawLine(ingitionPoint, ejectionPoint);

        // Simulate the trajectory
        Vector3 position = ejectionPoint;
        Vector3 newposition = position;
        Vector3 velocity = momentumVector;
        float timeStep = 0.025f;
        var count = Mathf.Clamp(propellantForce * (simulationTime * 0.01f), 0, 100);
        for (int i = 0; i < count; i++)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.yellow, i / (count * 0.9f));
            float radius = Mathf.Lerp(0.2f, 0.01f, i / (count * 0.9f));
            if (Physics.Raycast(position, velocity, out var hit, 0.1f))
            {
                newposition = hit.point;
                if (solidLine) Gizmos.DrawLine(position, newposition);
                else Gizmos.DrawSphere(position, radius);
                break;
                
            }
            newposition += velocity * timeStep;
            if (solidLine) Gizmos.DrawLine(position, newposition);
            else Gizmos.DrawSphere(position, radius);
            position = newposition;
            velocity += Physics.gravity * timeStep;
        }
    }
#endif
}