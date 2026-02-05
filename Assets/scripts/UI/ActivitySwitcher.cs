using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ActivitySwitcher : MonoBehaviour
{
    [Header("Toggle Configuration")]
    [Tooltip("Starting state of the toggle")]
    public bool initialState = false;
    public bool selfDeactivate = false;
    
    [Header("Actions")]
    [Tooltip("List of actions to perform when toggling states")]
    public List<ToggleAction> toggleActions = new List<ToggleAction>();
    
    [Header("Events")]
    [Tooltip("Event triggered when state changes to ON")]
    public UnityEvent onStateChangedToOn = new UnityEvent();
    
    [Tooltip("Event triggered when state changes to OFF")]
    public UnityEvent onStateChangedToOff = new UnityEvent();

    private bool activity;
    private Animator targetAnimator;

    public bool CurrentState => activity;

    private void Start()
    {
        activity = initialState;
        InitializeActions();
        ApplyCurrentStateActions();
    }

    private void InitializeActions()
    {
        foreach (var action in toggleActions)
        {
            if (action.actionType == ToggleActionType.Animation && action.targetGameObject != null)
            {
                targetAnimator = action.targetGameObject.GetComponent<Animator>();
            }
        }
    }

    public void Toggle()
    {
        activity = !activity;
        ApplyCurrentStateActions();
        TriggerStateChangeEvent();
    }

    public void SelfToggle()
    {
        if(activity && !selfDeactivate)
        {
            return;
        }

        Toggle();
    }

    public void TurnOn()
    {
        if (activity != true)
        {
            activity = true;
            ApplyCurrentStateActions();
            TriggerStateChangeEvent();
        }
    }

    public void TurnOff()
    {
        if (activity != false)
        {
            activity = false;
            ApplyCurrentStateActions();
            TriggerStateChangeEvent();
        }
    }

    private void ApplyCurrentStateActions()
    {
        foreach (var action in toggleActions)
        {
            ApplyAction(action);
        }
    }

    private void ApplyAction(ToggleAction action)
    {
        bool shouldApply = activity ? action.applyOnEnable : !action.applyOnEnable;

        switch (action.actionType)
        {
            case ToggleActionType.Animation:
                if (shouldApply && action.targetGameObject != null)
                {
                    var animator = action.targetGameObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger(action.animationTrigger);
                    }
                    else
                    {
                        Debug.LogWarning($"ToggleStateController: No Animator found on {action.targetGameObject.name} for animation trigger '{action.animationTrigger}'");
                    }
                }
                break;

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
        if (activity)
        {
            onStateChangedToOn.Invoke();
        }
        else
        {
            onStateChangedToOff.Invoke();
        }
    }

    public void SetActivity(bool state)
    {
        if (activity != state)
        {
            activity = state;
            ApplyCurrentStateActions();
            TriggerStateChangeEvent();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (toggleActions != null)
        {
            foreach (var action in toggleActions)
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
    Animation,
    GameObjectActivation,
    ColorChange,
    CustomEvent
}

[System.Serializable] public class ToggleAction
{
    public ToggleActionType actionType;
    public GameObject targetGameObject;
    public string animationTrigger;
    public Color targetColor;
    public UnityEvent customEvent;
    public bool applyOnEnable = true;
}
