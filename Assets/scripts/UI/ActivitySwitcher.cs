using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ActivitySwitcher : MonoBehaviour
{
    [Header("Toggle Configuration")]
    [SerializeField]
    private bool _initialState = false;
    
    [SerializeField]
    private bool _selfDeactivate = false;
    
    [Header("Actions")]
    [Tooltip("List of actions to perform when toggling states")]
    public List<ToggleAction> _toggleActions = new List<ToggleAction>();
    
    [Header("Events")]
    [Tooltip("Event triggered when state changes to ON")]
    public UnityEvent _onStateChangedToOn = new UnityEvent();
    
    [Tooltip("Event triggered when state changes to OFF")]
    public UnityEvent _onStateChangedToOff = new UnityEvent();

    private bool _activity;

    public bool CurrentState => _activity;

    private void Start()
    {
        _activity = _initialState;
        ApplyCurrentStateActions();
    }

    public void Toggle()
    {
        _activity = !_activity;
        ApplyCurrentStateActions();
        TriggerStateChangeEvent();
    }

    public void SelfToggle()
    {
        if(_activity && !_selfDeactivate)
        {
            return;
        }

        Toggle();
    }

    public void TurnOn()
    {
        if (_activity != true)
        {
            _activity = true;
            ApplyCurrentStateActions();
            TriggerStateChangeEvent();
        }
    }

    public void TurnOff()
    {
        if (_activity != false)
        {
            _activity = false;
            ApplyCurrentStateActions();
            TriggerStateChangeEvent();
        }
    }

    private void ApplyCurrentStateActions()
    {
        foreach (var action in _toggleActions)
        {
            ApplyAction(action);
        }
    }

    private void ApplyAction(ToggleAction action)
    {
        bool shouldApply = _activity ? action.applyOnEnable : !action.applyOnEnable;

        switch (action.actionType)
        {
            case ToggleActionType.GameObjectActivation:
                if (action.targetGameObject != null)
                {
                    action.targetGameObject.SetActive(shouldApply);
                }
                break;

            case ToggleActionType.ColorChange:
                if (action.targetGameObject != null)
                {
                    ApplyColorChange(action.targetGameObject, shouldApply ? action.targetColor : GetOriginalColor(action.targetGameObject));
                }
                break;

            case ToggleActionType.CustomEvent:
                if (shouldApply)
                {
                    action.customEvent?.Invoke();
                }
                break;
        }
    }

    private void ApplyColorChange(GameObject target, Color color)
    {
        if (target == null) return;

        var image = target.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = color;
            return;
        }

        var textMeshPro = target.GetComponent<TMPro.TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.color = color;
            return;
        }
    }

    private Color GetOriginalColor(GameObject target)
    {
        if (target == null) return Color.white;

        var image = target.GetComponent<UnityEngine.UI.Image>();
        if (image != null) return image.color;

        var textMeshPro = target.GetComponent<TMPro.TextMeshProUGUI>();
        if (textMeshPro != null) return textMeshPro.color;

        return Color.white;
    }

    private void TriggerStateChangeEvent()
    {
        if (_activity)
        {
            _onStateChangedToOn.Invoke();
        }
        else
        {
            _onStateChangedToOff.Invoke();
        }
    }

    public void SetActivity(bool state)
    {
        if (_activity != state)
        {
            _activity = state;
            ApplyCurrentStateActions();
            TriggerStateChangeEvent();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_toggleActions != null)
        {
            foreach (var action in _toggleActions)
            {
                if (action.actionType == ToggleActionType.CustomEvent && action.customEvent == null)
                {
                    action.customEvent = new UnityEvent();
                }
            }
        }
    }
#endif
}

public enum ToggleActionType
{
    GameObjectActivation,
    ColorChange,
    CustomEvent
}

[System.Serializable] public class ToggleAction
{
    public ToggleActionType actionType;
    public GameObject targetGameObject;
    public Color targetColor;
    public UnityEvent customEvent;
    public bool applyOnEnable = true;
}
