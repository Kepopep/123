using System.Collections.Generic;
using UnityEngine;

public class ScrollRectIndicatorHolder : MonoBehaviour
{
    [Header("References")]
    public ScrollRectNavigator _scrollRectNavigator;

    [Header("Indicator Settings")]
    public ActivitySwitcher _indicator;

    private List<ActivitySwitcher> _indicators = new List<ActivitySwitcher>();

    void Start()
    {
        InitializeIndicators();
        SetupEventListeners();
    }

    void OnDestroy()
    {
        RemoveEventListeners();
    }

    private void InitializeIndicators()
    {
        CreateIndicators();
        UpdateIndicators();
    }

    private void CreateIndicators()
    {
        _indicators.Clear();

        var elementCount = _scrollRectNavigator.GetTotalDisplayElements();
        for (int i = 0; i < elementCount; i++)
        {
            var indicator = Instantiate(_indicator, gameObject.transform);
            _indicators.Add(indicator);
        }
    }

    private void SetupEventListeners()
    {
        if (_scrollRectNavigator != null)
        {
            _scrollRectNavigator.OnNextElement += OnElementChanged;
            _scrollRectNavigator.OnPreviousElement += OnElementChanged;
        }
    }

    private void RemoveEventListeners()
    {
        if (_scrollRectNavigator != null)
        {
            _scrollRectNavigator.OnNextElement -= OnElementChanged;
            _scrollRectNavigator.OnPreviousElement -= OnElementChanged;
        }
    }

    private void OnElementChanged()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        var activeIndex = _scrollRectNavigator.GetCurrentDisplayIndex();

        for (int i = 0; i < _indicators.Count; i++)
        {
            if (activeIndex == i)
            {
                _indicators[i].TurnOn();
            }
            else
            {
                _indicators[i].TurnOff();
            }
        }
    }
}
