using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using ShipGame.ScriptObj;

namespace ShipGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        private readonly WaitForFixedUpdate _wffu = new();

        [SerializeField] private CoreData coreData;
        [SerializeField] private GameAction initializeCannonAction, initializeAmmoAction, levelCompleteAction, levelFailedAction;

        private ObjectInstancer _shipInstancer;
        
        public UnityEvent 
            onAwake,
            onStart,
            onLateStart,
            onInitializeLevel,
            onLevelInitialized,
            onLevelComplete,
            onLevelFailed;

        private void OnValidate()
        {
            if (!coreData) Debug.LogError("Core Data is missing. One must be provided.", this);
            if (!initializeCannonAction) Debug.LogError("Initialize Cannon Action is missing. One must be provided.", this);
            if (!initializeAmmoAction) Debug.LogError("Initialize Ammo Action is missing. One must be provided.", this);
            if (!levelCompleteAction) Debug.LogError("Level Complete Action is missing. One must be provided.", this);
            if (!levelFailedAction) Debug.LogError("Level Failed Action is missing. One must be provided.", this);
        }

        private void Awake()
        {
            onAwake.Invoke();
            _shipInstancer = this.AddComponent<ObjectInstancer>();
            _shipInstancer.SetInstancerData(coreData.shipInstancerData);
        }
        
        private void OnEnable()
        {
            levelCompleteAction.RaiseEvent += LevelComplete;
            levelFailedAction.RaiseEvent += LevelFailed;
        }
        
        private void OnDisable()
        {
            levelCompleteAction.RaiseEvent -= LevelComplete;
            levelFailedAction.RaiseEvent -= LevelFailed;
        }

        private void Start()
        {
            onStart.Invoke();
            StartCoroutine(LateInit());
            StartCoroutine(InitializeLevelCoroutine());
        }

        private IEnumerator LateInit()
        {
            yield return _wffu;
            yield return _wffu;
            yield return _wffu;
            onLateStart.Invoke();
        }

        private IEnumerator InitializeLevelCoroutine()
        {
            onInitializeLevel.Invoke();
            yield return null;
            coreData.Setup();
            
            // Force the ship to initialize first before the cannon and ammo
            yield return StartCoroutine(InitializeShipCoroutine());

            // Asynchronously Initialize the cannon and ammo only after the ship is done
            StartCoroutine(InitializeCannon());
            StartCoroutine(InitializeAmmo());
            yield return _wffu;

            onLevelInitialized.Invoke();
            
            if (coreData.playerInitializePositionAction)
                coreData.playerInitializePositionAction.RaiseAction();
        }

        private IEnumerator InitializeShipCoroutine()
        {
            _shipInstancer.InstantiateObjects();
            yield return null;
        }

        private IEnumerator InitializeCannon()
        {
            initializeCannonAction.RaiseAction();
            yield return null;
        }

        private IEnumerator InitializeAmmo()
        {
            initializeAmmoAction.RaiseAction();
            yield return null;
        }

        private void LevelComplete(GameAction action)
        {
            coreData.LevelCompleted();
            onLevelComplete.Invoke();
        }

        private void LevelFailed(GameAction action)
        {
            coreData.LevelFailed();
            onLevelFailed.Invoke();
        }
    }
}