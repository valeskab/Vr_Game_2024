using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;

namespace UI.DialogueSystem
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueData")]
    public class DialogueData : ScriptableObject, IResetOnNewGame, INeedButton
    {
        public bool playOnlyOncePerGame;
        [SerializeField, InspectorReadOnly] private bool _hasPlayed;
        
        public void ResetToNewGameValues(int tier = 2)
        {
            if (tier < 2) return;
            _hasPlayed = false;
        }

        public bool hasPlayed
        {
            get => _hasPlayed;
            set => _hasPlayed = value;
        }
        
        [Header("Dialogue Data")]
        [SerializeField] private string dialogueName;
        [SerializeField] private GameAction firstAction, lastAction;
        
        [SerializeField] [TextArea] private string[] dialogue;
        [SerializeField] private Response[] responses;
        [SerializeField] private UnityEvent onTrigger, firstTrigger, lastTrigger;
        
        public string[] Dialogue => dialogue;

        public bool hasResponses => responses is { Length: > 0 };
        public Response[] Responses => responses;

        private void OnEnable()
        {
            if (firstAction == null) return;
            firstAction.Raise += FirstDialogueEvent;
            
            if (lastAction == null) return;
            lastAction.Raise += LastDialogueEvent;
        }

        public void DialogueEvent(GameAction _) => onTrigger.Invoke();
        public void FirstDialogueEvent(GameAction _) => firstTrigger.Invoke();
        public void LastDialogueEvent(GameAction _) => lastTrigger.Invoke();
        
        public List<(System.Action, string)> GetButtonActions()
        {
            return new List<(System.Action, string)> { (() => {_hasPlayed = false;}, "Set Has Played to False") };
        }
    }
}