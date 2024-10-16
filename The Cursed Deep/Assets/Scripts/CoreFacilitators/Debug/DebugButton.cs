using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;

public class DebugButton : MonoBehaviour, INeedButton
{
    public UnityEvent onButtonPress;


    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
            (() => onButtonPress.Invoke(), "OnButtonPress")
        };
    }

}
